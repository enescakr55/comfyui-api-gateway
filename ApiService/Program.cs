
using ApiService.Services;
using ApiService.Websocket;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ApiService
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);
      builder.Services.AddAuthorization();
      builder.Services.AddEndpointsApiExplorer();
      builder.Services.AddSignalR();
      builder.Services.AddSwaggerGen();
      builder.Services.AddCors();
      var contentRootPath = builder.Environment.WebRootPath;
      builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
      .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
      .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
      .AddEnvironmentVariables()
      .Build();
      var Configuration = builder.Configuration;
      builder.Services.AddSingleton<IConfiguration>(Configuration);
      builder.Services.AddSingleton<ComfyUISocketListener>();
      builder.Services.AddTransient<PromptRestService>();
      var app = builder.Build();
      var comfyListener = app.Services.GetRequiredService<ComfyUISocketListener>();
      Task.Run(()=> comfyListener.StartAsync());

      app.UseCors(x => x.SetIsOriginAllowed(_ => true).AllowCredentials().AllowAnyHeader().AllowAnyMethod());
      if (app.Environment.IsDevelopment())
      {
        app.UseSwagger();
        app.UseSwaggerUI();
      }

      app.UseHttpsRedirection();
      app.MapHub<ComfyUiHub>("hubs/comfy-ui");
      app.UseAuthorization();
      //
      var promptsPath = builder.Configuration.GetSection("Paths:Prompts").Value;
      if(promptsPath == null){
        throw new ArgumentNullException(nameof(promptsPath));
      }
      PromptIndexerService.Instance.InitializePrompts(promptsPath);
      //
      ComfyUiEndpoints.BuildEndpoints(app);

      app.Run();
    }
  }
}
