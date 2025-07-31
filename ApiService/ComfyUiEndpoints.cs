using ApiService.Models.Requests.ImageToImage;
using ApiService.Models.Requests.TextToImage;
using ApiService.Models.Responses;
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
      app.MapGet("api/v1/comfy-ui/image-to-image/schemes", (HttpContext httpContext) =>
      {
        var schemes = PromptIndexerService.Instance.GetImageToImagePrompts();
        var keys = schemes.Keys.ToList();
        return Results.Ok(keys);
      });
      app.MapGet("api/v1/comfy-ui/text-to-video/schemes", (HttpContext httpContext) =>
      {
        var schemes = PromptIndexerService.Instance.GetTextToVideoPrompts();
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
      app.MapPost("/api/v1/comfy-ui/image-to-image/schemes/{scheme}/generate", (HttpContext httpContext, string scheme, HttpRequest request) =>
      {
        var restService = app.Services.GetRequiredService<PromptRestService>();
        var promptContent = PromptIndexerService.Instance.GetImageToImagePrompt(scheme);
        if (promptContent == null)
        {
          return Results.NotFound("Scheme not found");
        }
        var imageProcessService = httpContext.RequestServices.GetRequiredService<ImageProcessService>();
        var originalImg = request.Form.Files["image"];
        var mask = request.Form.Files["mask"];
        if(originalImg == null || mask == null){
          throw new ArgumentNullException("Please check Image or Mask arguments");
        }

        var resizedOriginalImage = imageProcessService.ResizeImageIfLimitsExceed(originalImg.OpenReadStream());
        var resizedMaskImage = imageProcessService.ResizeImageIfLimitsExceed(mask.OpenReadStream());
        var originalImgId = restService.UploadImage(originalImg.FileName, resizedOriginalImage).Name;
        var maskId = restService.UploadImage(mask.FileName, resizedMaskImage).Name;
        promptContent = promptContent.Replace("{positive-prompt}", request.Form["positive"]);
        promptContent = promptContent.Replace("{negative-prompt}", request.Form["negative"]);
        promptContent = promptContent.Replace("{original-image}", originalImgId);
        promptContent = promptContent.Replace("{mask-image}", maskId);
        promptContent = promptContent.Replace("{client-id}", ComfyClientIdService.Instance.GetId());
        promptContent = promptContent.Replace("\"{seed}\"", GenerateSeed());
        var jsonObj = JsonSerializer.Deserialize<object>(promptContent);
        if (jsonObj == null)
        {
          return Results.BadRequest();
        }

        var result = restService.SendPrompt(jsonObj);
        return Results.Ok(result);
      }).Accepts<GenerateImageToImageRequest>("multipart/form-data").Produces(200);
      
      app.MapPost("/api/v1/comfy-ui/image-to-image/image-upload", (HttpContext httpContext) =>
      {

        var restService = app.Services.GetRequiredService<PromptRestService>();
        var imageData = httpContext.Request.Form.Files[0];
        var stream = imageData.OpenReadStream();
        var response = restService.UploadImage(imageData.FileName, stream);
        Results.Ok(response);
      }).Accepts<FormFile>("multipart/form-data").Produces<ImageUploadResponse>();
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
