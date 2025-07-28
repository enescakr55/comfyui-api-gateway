using ApiService.Models.Requests.TextToImage;
using ApiService.Services;
using System.Text.Json;

namespace ApiService
{
  public static class ComfyUiEndpoints
  {
    public static WebApplication BuildEndpoints(this WebApplication app)
    {
      app.MapGet("api/v1/comfy-ui/text-to-image/schemes", (HttpContext httpContext) =>
      {
        var schemes = PromptIndexerService.Instance.GetTextToImagePrompts();
        var keys = schemes.Keys.ToList();
        return Results.Ok(keys);
      });
      app.MapPost("/api/v1/comfy-ui/text-to-image/schemes/{scheme}/generate", (HttpContext httpContext, string scheme, GenerateTextToImageRequest request) =>
      {
        var promptContent = PromptIndexerService.Instance.GetTextToImagePrompt(scheme);
        if (promptContent == null)
        {
          return Results.NotFound("Scheme not found");
        }
        promptContent = promptContent.Replace("{positive-prompt}", request.Positive);
        promptContent = promptContent.Replace("{negative-prompt}", request.Negative);
        promptContent = promptContent.Replace("{client-id}", ComfyClientIdService.Instance.GetId());
        promptContent = promptContent.Replace("\"{seed}\"", GenerateSeed());
        var jsonObj = JsonSerializer.Deserialize<object>(promptContent);
        if (jsonObj == null)
        {
          return Results.BadRequest();
        }
        var restService = app.Services.GetRequiredService<PromptRestService>();
        var result = restService.SendPrompt(jsonObj);
        return Results.Ok(result);
      });
      return app;
    }
    private static string GenerateSeed()
    {
      var seed = "";

      var rand = Random.Shared.Next(100000000, 999999999);
      seed = rand.ToString();

      return seed;


    }
  }
}
