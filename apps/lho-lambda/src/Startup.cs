using System.Text.Json;
using System.Text.Json.Serialization;
using LhoLambda.services;

namespace LhoLambda;

public class Startup
{
  public Startup(IConfiguration configuration)
  {
    Configuration = configuration;
  }

  public IConfiguration Configuration { get; }

  public void ConfigureServices(IServiceCollection services)
  {
    services.AddControllers().AddJsonOptions(opts =>
    {
      opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
      opts.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
      opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
      opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

    services.AddCors(options =>
    {
      options.AddDefaultPolicy(builder =>
      {
        builder
          .AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader();
      });
    });

    services.AddMemoryCache();
    services.AddHttpClient<ISpotifyService, SpotifyService>();
    services.AddScoped<ISpotifyService, SpotifyService>();
    services.AddLogging();
  }

  public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
  {
    if (env.IsDevelopment())
      app.UseDeveloperExceptionPage();

    app.UseCors();
    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
      endpoints.MapControllers();

      // Handle undefined routes - return 404 with proper JSON response
      endpoints.MapFallback(async context =>
      {
        context.Response.StatusCode = 404;
        context.Response.ContentType = "application/json";
        context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        context.Response.Headers.Append("Access-Control-Allow-Methods", "GET,OPTIONS,POST,PUT,DELETE");
        context.Response.Headers.Append("Cache-Control", "no-cache");

        await context.Response.WriteAsync("{\"message\": \"route not found\"}");
      });
    });
  }
}
