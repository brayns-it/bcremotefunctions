namespace BCRemoteFunctions
{
    public class FileSystem : Codeunit
    {
        [ApiMethod(Method = RequestMethod.Post, Route = "fileSystem/FileExists")]
        public bool FileExists(string path)
        {
            bool result = false;

            SafeRun(path, () =>
            {
                result = File.Exists(path);
            });

            return result;
        }

        [ApiMethod(Method = RequestMethod.Post, Route = "fileSystem/ListFiles")]
        public List<string> ListFiles(string path, List<string> filters)
        {
            var result = new List<string>();

            SafeRun(path, () =>
            {
                var di = new DirectoryInfo(path);
                foreach (var filter in filters)
                    foreach (var fi in di.GetFiles(filter))
                        result.Add(fi.Name);
            });

            return result;
        }

        [ApiMethod(Method = RequestMethod.Post, Route = "fileSystem/MoveFile")]
        public void MoveFile(string oldPath, string newPath)
        {
            SafeRun(oldPath);
            SafeRun(newPath, () =>
            {
                var fi = new FileInfo(oldPath);
                if (fi.Exists)
                    fi.MoveTo(newPath);
            });
        }

        [ApiMethod(Method = RequestMethod.Post, Route = "fileSystem/ReadFile")]
        public string ReadFile(string path)
        {
            byte[] buf = new byte[0];

            SafeRun(path, () =>
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                buf = new byte[fs.Length];
                fs.Read(buf);
                fs.Close();
            });

            return Convert.ToBase64String(buf);
        }

        [ApiMethod(Method = RequestMethod.Post, Route = "fileSystem/DeleteFile")]
        public void DeleteFile(string path)
        {
            SafeRun(path, () =>
            {
                var fi = new FileInfo(path);
                if (fi.Exists)
                    fi.Delete();
            });
        }

        [ApiMethod(Method = RequestMethod.Post, Route = "fileSystem/WriteFile")]
        public void WriteFile(string path, string content)
        {
            SafeRun(path, () =>
            {
                if (File.Exists(path))
                    throw new Error(Label("File {0} already exists", path));

                FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                byte[] buf = Convert.FromBase64String(content);
                fs.Write(buf);
                fs.Close();
            });
        }

        private void SafeRun(string path, Action? action = null)
        {
            string p2 = path.Replace("\\", "/").Trim();
            if (p2.Contains(".."))
                throw new Error(Label("Unallowed path {0}", path));

            var allPath = new AllowedPath();
            allPath.UserID.SetRange(CurrentSession.UserId);
            if (allPath.FindSet())
                while (allPath.Read())
                {
                    string p1 = allPath.Path.Value.Replace("\\", "/").Trim();
                    if (!p1.EndsWith("/")) p1 += "/";

                    if (p2.StartsWith(p1, StringComparison.OrdinalIgnoreCase))
                    {
                        if (action != null)
                        {
                            if (allPath.CredentialCode.Value.Length > 0)
                            {
                                var credMgmt = new Brayns.System.CredentialMgmt(allPath.CredentialCode.Value);
                                credMgmt.RunAs(action);
                            }
                            else
                                action.Invoke();
                        }

                        return;
                    }
                }

            throw new Error(Label("Unauthorized access to {0}", path));
        }
    }
}
