using System.Text.Json;
using Microsoft.Extensions.Options;
using proyecto.web.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOptions<ApiOptions>().BindConfiguration("Api")
    .Validate(options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var uri)
        && (uri.Scheme == "https" || uri.Scheme == "http"), "Api:BaseUrl debe ser una URL HTTP o HTTPS absoluta.")
    .ValidateOnStart();

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();  
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

// Public client configuration only. Secrets and credentials are never exposed here.
app.MapGet("/app-config.js", (IOptions<ApiOptions> options, HttpContext context) =>
{
    context.Response.Headers.CacheControl = "no-store";
    return Results.Text("window.APP_CONFIG = Object.freeze(" +
        JsonSerializer.Serialize(new { apiUrl = options.Value.BaseUrl.TrimEnd('/') }) + ");",
        "application/javascript");
});

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
