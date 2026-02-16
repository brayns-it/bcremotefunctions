using FluentFTP;
using System.Text.RegularExpressions;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace BCRemoteFunctions
{
    public class FTP : Codeunit
    {
        private static object _lockConnections = new();
        private static Dictionary<string, object> _connections = new();
        private static Dictionary<string, DateTime> _connAging = new();

        private void Cleanup()
        {
            var toRemove = new List<string>();

            foreach (string id in _connAging.Keys)
            {
                if (DateTime.Now.Subtract(_connAging[id]).TotalMinutes > 10)
                    toRemove.Add(id);
            }

            foreach (string id in toRemove)
            {
                try
                {
                    Disconnect(id);
                }
                catch
                {
                }
            }
        }

        [ApiMethod(Method = RequestMethod.Post, Route = "ftp/Disconnect")]
        public void Disconnect(string connectionId)
        {
            object? conn = null;

            lock (_lockConnections)
            {
                if (_connections.ContainsKey(connectionId))
                {
                    conn = _connections[connectionId];
                    _connections.Remove(connectionId);
                }

                if (_connAging.ContainsKey(connectionId))
                    _connAging.Remove(connectionId);
            }

            switch (conn)
            {
                case FtpClient ftp:
                    ftp.Disconnect();
                    break;

                case SftpClient sftp:
                    sftp.Disconnect();
                    break;
            }
        }

        [ApiMethod(Method = RequestMethod.Post, Route = "ftp/Connect")]
        public string Connect(string uri, string login, string password)
        {
            Cleanup();

            string id = Guid.NewGuid().ToString();
            object? conn;

            var u = new Uri(uri);
            if (u.Scheme.ToLower().StartsWith("ftp"))
            {
                FtpClient client = new FtpClient(u.Host, login, password, u.Port);
                client.Config.EncryptionMode = FtpEncryptionMode.Auto;
                client.Config.ValidateAnyCertificate = true;
                client.Connect();
                conn = client;
            }
            else if (u.Scheme.ToLower().StartsWith("sftp"))
            {
                SftpClient client = new SftpClient(u.Host, u.Port, login, password);
                client.Connect();
                conn = client;
            }
            else
            {
                throw new Error(Label("Invalid scheme {0}", u.Scheme));
            }


            lock (_lockConnections)
            {
                _connections[id] = conn;
                _connAging[id] = DateTime.Now;
            }

            return id;
        }

        private object GetConnection(string connectionId)
        {
            _connAging[connectionId] = DateTime.Now;
            return _connections[connectionId];
        }

        [ApiMethod(Method = RequestMethod.Post, Route = "ftp/FileExists")]
        public bool FileExists(string connectionId, string path)
        {
            bool result = false;

            var conn = GetConnection(connectionId);
            switch (conn)
            {
                case FtpClient ftp:
                    result = ftp.FileExists(path);
                    break;

                case SftpClient sftp:
                    result = sftp.Exists(path);
                    break;
            }

            return result;
        }

        private bool IsFileInFilters(string name, List<string> filters)
        {
            if (filters.Count == 0)
                return true;

            foreach (string f in filters)
            {
                var r = f.Replace(".", "\\.");
                r = r.Replace("*", ".*");
                if (Regex.IsMatch(name, r))
                    return true;
            }

            return false;
        }

        [ApiMethod(Method = RequestMethod.Post, Route = "ftp/ListFiles")]
        public List<string> ListFiles(string connectionId, string path, List<string> filters)
        {
            var result = new List<string>();

            var conn = GetConnection(connectionId);

            switch (conn)
            {
                case FtpClient ftp:
                    foreach (FtpListItem itm in ftp.GetListing(path))
                        if (IsFileInFilters(itm.Name, filters))
                            result.Add(itm.Name);
                    break;

                case SftpClient sftp:
                    foreach (ISftpFile itm in sftp.ListDirectory(path))
                        if (IsFileInFilters(itm.Name, filters))
                            result.Add(itm.Name);
                    break;
            }

            return result;
        }

        [ApiMethod(Method = RequestMethod.Post, Route = "ftp/MoveFile")]
        public void MoveFile(string connectionId, string oldPath, string newPath)
        {
            var conn = GetConnection(connectionId);

            switch (conn)
            {
                case FtpClient ftp:
                    if (!ftp.MoveFile(oldPath, newPath, FtpRemoteExists.Skip))
                        throw new Error(Label("Unable to move in {0}", newPath));
                    break;

                case SftpClient sftp:
                    sftp.RenameFile(oldPath, newPath);
                    break;
            }
        }

        [ApiMethod(Method = RequestMethod.Post, Route = "ftp/ReadFile")]
        public string ReadFile(string connectionId, string path)
        {
            byte[] buf = new byte[0];

            var conn = GetConnection(connectionId);

            switch (conn)
            {
                case FtpClient ftp:
                    if (!ftp.DownloadBytes(out buf, path))
                        throw new Error(Label("Unable to read {0}", path));
                    break;

                case SftpClient sftp:
                    MemoryStream ms = new MemoryStream();
                    sftp.DownloadFile(path, ms);
                    buf = ms.ToArray();
                    ms.Close();
                    break;
            }

            return Convert.ToBase64String(buf);
        }

        [ApiMethod(Method = RequestMethod.Post, Route = "ftp/DeleteFile")]
        public void DeleteFile(string connectionId, string path)
        {
            var conn = GetConnection(connectionId);

            switch (conn)
            {
                case FtpClient ftp:
                    if (ftp.FileExists(path))
                        ftp.DeleteFile(path);
                    break;

                case SftpClient sftp:
                    if (sftp.Exists(path))
                        sftp.Delete(path);
                    break;
            }
        }

        [ApiMethod(Method = RequestMethod.Post, Route = "ftp/WriteFile")]
        public void WriteFile(string connectionId, string path, string content)
        {
            var conn = GetConnection(connectionId);

            switch (conn)
            {
                case FtpClient ftp:
                    if (ftp.UploadBytes(Convert.FromBase64String(content), path, FtpRemoteExists.Skip) != FtpStatus.Success)
                        throw new Error(Label("Unable to write {0}", path));
                    break;

                case SftpClient sftp:
                    MemoryStream ms = new MemoryStream(Convert.FromBase64String(content));
                    ms.Position = 0;
                    sftp.UploadFile(ms, path);
                    ms.Close();
                    break;
            }
        }
    }
}
