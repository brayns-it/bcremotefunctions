using Newtonsoft.Json.Linq;

namespace BCRemoteFunctions
{
    public class Program
    {
        static string tokensFile = "";
        public static Tokens Tokens { get; set; } = new();

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            tokensFile = builder.Environment.ContentRootPath + "/tokens.json";

            var app = builder.Build();
            app.MapPost("/{system}/{function}", Execute);
            app.Run();
        }

        public static async Task Execute(HttpContext httpContext)
        {
            try
            {
                if (!File.Exists(tokensFile))
                    throw new Exception("No tokens enabled");

                StreamReader sr = new StreamReader(tokensFile);
                Tokens = JObject.Parse(sr.ReadToEnd()).ToObject<Tokens>()!;
                sr.Close();

                string system = httpContext.Request.RouteValues["system"]!.ToString()!;
                string function = httpContext.Request.RouteValues["function"]!.ToString()!;

                string auth = httpContext.Request.Headers["Authorization"]!.ToString()!;
                if (!auth.StartsWith("Bearer "))
                    throw new Exception("Invalid authentication");
                auth = auth.Substring(7);

                bool authOk = false;
                foreach (var tok in Tokens.tokens)
                {
                    if (tok.token == auth)
                    {
                        authOk = true;
                        break;
                    }
                }

                if (!authOk)
                    throw new Exception("Unauthorized");

                sr = new StreamReader(httpContext.Request.Body);
                JObject request = JObject.Parse(await sr.ReadToEndAsync());
                sr.Close();

                JObject response;

                switch (system)
                {
                    case "generic":
                        response = await Generic.Execute(function, request);
                        break;
                    default:
                        throw new Exception("Invalid system");
                }

                await httpContext.Response.WriteAsync(response.ToString(Newtonsoft.Json.Formatting.Indented));
            }
            catch (Exception ex)
            {
                httpContext.Response.ContentType = "application/json";

                var res = new ErrorResponse(ex);
                await httpContext.Response.WriteAsync(JObject.FromObject(res).ToString(Newtonsoft.Json.Formatting.Indented));
            }
        }
    }
}