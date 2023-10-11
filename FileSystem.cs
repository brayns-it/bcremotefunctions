using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Cms;
using System.Text;

namespace BCRemoteFunctions
{
    public static class FileSystem
    {
        public static async Task<JObject> Execute(string function, JObject request, Tokens.Token token)
        {
            switch (function)
            {
                case "FileExists":
                    return await FileExists(request, token);
                case "MoveFile":
                    return await MoveFile(request, token);
                case "ReadFile":
                    return await ReadFile(request, token);
                case "DeleteFile":
                    return await DeleteFile(request, token);
                case "WriteFile":
                    return await WriteFile(request, token);
                default:
                    throw new Exception("Invalid function " + function);
            }
        }

        public static async Task<JObject> FileExists(JObject request, Tokens.Token token)
        {
            string path = request["path"]!.ToString();

            FileInfo fi = new FileInfo(path);
            Auth.VerifySchema(token, "file://" + fi.DirectoryName);
                        
            JObject res = new();
            res["exists"] = fi.Exists;
            return res;
        }

        public static async Task<JObject> ReadFile(JObject request, Tokens.Token token)
        {
            string path = request["path"]!.ToString();

            FileInfo fi = new FileInfo(path);
            Auth.VerifySchema(token, "file://" + fi.DirectoryName);

            FileStream fs = new(fi.FullName, FileMode.Open, FileAccess.Read);
            MemoryStream ms = new();
            await fs.CopyToAsync(ms);
            fs.Close();

            JObject res = new();
            res["content"] = Convert.ToBase64String(ms.ToArray());

            ms.Close();
            return res;
        }

        public static async Task<JObject> DeleteFile(JObject request, Tokens.Token token)
        {
            string path = request["path"]!.ToString();

            FileInfo fi = new FileInfo(path);
            Auth.VerifySchema(token, "file://" + fi.DirectoryName);

            if (fi.Exists)
                fi.Delete();

            return new();
        }

        public static async Task<JObject> MoveFile(JObject request, Tokens.Token token)
        {
            string oldPath = request["oldPath"]!.ToString();
            string newPath = request["newPath"]!.ToString();

            FileInfo fiOld = new FileInfo(oldPath);
            FileInfo fiNew = new FileInfo(newPath);
            Auth.VerifySchema(token, "file://" + fiOld.DirectoryName);
            Auth.VerifySchema(token, "file://" + fiNew.DirectoryName);

            if (fiOld.Exists)
                fiOld.MoveTo(fiNew.FullName);

            return new();
        }

        public static async Task<JObject> WriteFile(JObject request, Tokens.Token token)
        {
            string path = request["path"]!.ToString();
            string b64content = request["content"]!.ToString();
            byte[] buf = Convert.FromBase64String(b64content);

            FileInfo fi = new FileInfo(path);
            Auth.VerifySchema(token, "file://" + fi.DirectoryName);

            if (fi.Exists)
                throw new Exception("File " + path + " already exists");

            FileStream fs = new(fi.FullName, FileMode.Create, FileAccess.Write);
            await fs.WriteAsync(buf, 0, buf.Length);
            fs.Close();

            return new();
        }
    }
}
