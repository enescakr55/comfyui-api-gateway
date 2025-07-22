
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
      builder.Services.AddSwaggerGen();

      var app = builder.Build();


      if (app.Environment.IsDevelopment())
      {
        app.UseSwagger();
        app.UseSwaggerUI();
      }

      app.UseHttpsRedirection();

      app.UseAuthorization();

      ComfyUiEndpoints.BuildEndpoints(app);

      app.Run();
    }
  }
}
