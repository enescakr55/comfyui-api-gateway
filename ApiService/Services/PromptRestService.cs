using ApiService.Models.Responses;
using Flurl.Http;

namespace ApiService.Services
{
  public class PromptRestService
  {
  private readonly IConfiguration _configuration;

    public PromptRestService(IConfiguration configuration)
    {
      _configuration = configuration;
    }
    public PromptResponse SendPrompt(object prompt) {
      var host = _configuration.GetSection("ComfyUi:Url").Value;
      var port = _configuration.GetSection("ComfyUi:Port").Value;
      var serviceUrl = host+":"+port+"/prompt";
      var response = serviceUrl. PostJsonAsync(prompt).Result;
      if(response.StatusCode == 200){
        var promptResponse = response.GetJsonAsync<PromptResponse>().GetAwaiter().GetResult();
        return promptResponse;
      }
      throw new Exception("An error occurred");
      
    }
    public ImageUploadResponse UploadImage(string fileName,Stream image){
      var host = _configuration.GetSection("ComfyUi:Url").Value;
      var port = _configuration.GetSection("ComfyUi:Port").Value;
      var serviceUrl = host + ":" + port + "/api/upload/image";

      var fileExt = Path.GetExtension(fileName).ToLowerInvariant();

      var guidName = Guid.NewGuid().ToString("N")+fileExt;
      var response = serviceUrl.PostMultipartAsync((a) => a.AddFile("image", image, guidName)).Result;
      if (response.StatusCode == 200)
      {
        var promptResponse = response.GetJsonAsync<ImageUploadResponse>().GetAwaiter().GetResult();
        return promptResponse;
      }
      throw new Exception("Image not uploaded");
    }
  }
}
