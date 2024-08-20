namespace BCRemoteFunctions
{
    public class FileSystem : Codeunit
    {
        [ApiMethod(Method = RequestMethod.Post, Route = "fileSystem/FileExists")]
        public bool FileExists(string path)
        {
            CheckPathSecurity(path);
            return File.Exists(path);
        }

        [ApiMethod(Method = RequestMethod.Post, Route = "fileSystem/MoveFile")]
        public void MoveFile(string oldPath, string newPath)
        {
            CheckPathSecurity(oldPath);
            CheckPathSecurity(newPath);

            var fi = new FileInfo(oldPath);
            if (fi.Exists)
                fi.MoveTo(newPath);
        }

        [ApiMethod(Method = RequestMethod.Post, Route = "fileSystem/ReadFile")]
        public string ReadFile(string path)
        {
            CheckPathSecurity(path);

            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] buf = new byte[fs.Length];
            fs.Read(buf);
            fs.Close();

            return Convert.ToBase64String(buf);
        }

        [ApiMethod(Method = RequestMethod.Post, Route = "fileSystem/DeleteFile")]
        public void DeleteFile(string path)
        {
            CheckPathSecurity(path);

            var fi = new FileInfo(path);
            if (fi.Exists)
                fi.Delete();
        }

        [ApiMethod(Method = RequestMethod.Post, Route = "fileSystem/WriteFile")]
        public void WriteFile(string path, string content)
        {
            CheckPathSecurity(path);

            if (File.Exists(path))
                throw new Error(Label("File {0} already exists", path));

            FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            byte[] buf = Convert.FromBase64String(content);
            fs.Write(buf);
            fs.Close();
        }

        private void CheckPathSecurity(string path)
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
                        return;
                }

            throw new Error(Label("Unauthorized access to {0}", path));
        }
    }
}
