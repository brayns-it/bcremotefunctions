using Newtonsoft.Json.Linq;

namespace BCRemoteFunctions
{
    public class ErrorResponse
    {
        public class ErrorMessage
        {
            public string message = "";
        }

        public ErrorMessage error = new();

        public ErrorResponse(Exception ex)
        {
            error.message = ex.Message;
        }
    }

    public class Tokens
    {
        public class Token
        {
            public string token = "";
            public bool genericEnabled = false;
            public List<Schema> authorizedSchema = new();
        }

        public class Schema
        {
            public string prefix = "";
            public bool enabled = false;
        }

        public List<Token> tokens = new();
    }

    public static class Auth
    {
        public static string TokensFile { get; set; } = "";
        public static Tokens Tokens { get; set; } = new();

        public static Tokens.Token VerifyToken(string tokToVerify)
        {
            foreach (var tok in Tokens.tokens)
                if (tok.token == tokToVerify)
                    return tok;

            throw new Exception("Unauthorized");
        }

        public static void VerifySchema(Tokens.Token token, string schemaPrefix)
        {
            bool addPrefix = true;
            foreach (var schema in token.authorizedSchema)
            {
                if (schema.prefix.ToLower() == schemaPrefix.ToLower())
                {
                    addPrefix = false;
                    if (schema.enabled)
                        return;
                }
            }
            if (addPrefix)
            {
                var schema = new Tokens.Schema();
                schema.prefix = schemaPrefix;
                schema.enabled = false;
                token.authorizedSchema.Add(schema);
                SaveTokens();
            }
            throw new Exception("Unauthorized access to " + schemaPrefix);
        }

        public static void ReadTokens()
        {
            if (!File.Exists(TokensFile))
                throw new Exception("No tokens enabled");

            StreamReader sr = new StreamReader(TokensFile);
            Tokens = JObject.Parse(sr.ReadToEnd()).ToObject<Tokens>()!;
            sr.Close();
        }

        public static void SaveTokens()
        {
            StreamWriter sw = new StreamWriter(TokensFile);
            sw.Write(JObject.FromObject(Tokens).ToString(Newtonsoft.Json.Formatting.Indented));
            sw.Close();
        }
    }
}
