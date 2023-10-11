using Newtonsoft.Json.Linq;

namespace BCRemoteFunctions
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Auth.TokensFile = builder.Environment.ContentRootPath + "/tokens.json";

            var app = builder.Build();
            app.MapPost("/{system}/{function}", Execute);
            app.Run();
        }

        public static async Task Execute(HttpContext httpContext)
        {
            try
            {
                Auth.ReadTokens();

                string system = httpContext.Request.RouteValues["system"]!.ToString()!;
                string function = httpContext.Request.RouteValues["function"]!.ToString()!;

                string bearer = httpContext.Request.Headers["Authorization"]!.ToString()!;
                if (!bearer.StartsWith("Bearer "))
                    throw new Exception("Invalid authentication");
                bearer = bearer.Substring(7);

                var tok = Auth.VerifyToken(bearer);

                var sr = new StreamReader(httpContext.Request.Body);
                JObject request = JObject.Parse(await sr.ReadToEndAsync());
                sr.Close();

                JObject response;

                switch (system)
                {
                    case "generic":
                        response = await Generic.Execute(function, request);
                        break;
                    case "fileSystem":
                        response = await FileSystem.Execute(function, request, tok);
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