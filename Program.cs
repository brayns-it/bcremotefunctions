using Brayns.Shaper;

var builder = WebApplication.CreateBuilder(args);
builder.InitializeShaper();

var app = builder.Build();
app.MapShaperApi();
app.MapShaperClient();
app.MapShaperDefault();
app.UseWebSockets();
app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/other"
});
app.UseShaperMonitor();

app.Run();