using ApiService.Models.Responses;
using Flurl.Http;
using System.Text.Json;

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
    private string GetHistory(string promptId){
      var host = _configuration.GetSection("ComfyUi:Url").Value;
      var port = _configuration.GetSection("ComfyUi:Port").Value;
      var serviceUrl = host + ":" + port + "/history/"+promptId;
      var stringResult =serviceUrl.GetStringAsync().Result;
      return stringResult;
      
    }
    //Birden çok çıkış dosyası olursa yalnızca ilk dosyayı görüntülüyor.
    //Birden çok çıkış dosyasına ihtiyaç olursa tipi string[] olarak değiştirilebilir.
    public string? GetOutputFileName(string promptId){
      var historyInfo = GetHistory(promptId);
      JsonDocument doc = JsonDocument.Parse(historyInfo);
      var rootEl = doc.RootElement;
      var prompt = rootEl.EnumerateObject().First().Value;
      var outputs = prompt.GetProperty("outputs");
      foreach(var outputNode in outputs.EnumerateObject()){
        var outputVal = outputNode.Value;
        if(outputVal.TryGetProperty("images",out var data)){
          foreach(var dataNode in data.EnumerateArray()){
            var dataType = dataNode.GetProperty("type").GetString();
            var fileName = dataNode.GetProperty("filename").GetString();
            if(dataType == "output"){
              return fileName;
            }
          }
        }
      }
      return null;
    }
    public MemoryStream GetOutputFile(string filename){
      var host = _configuration.GetSection("ComfyUi:Url").Value;
      var port = _configuration.GetSection("ComfyUi:Port").Value;
      var serviceUrl = host + ":" + port + "/api/view?type=output&filename="+filename;
      var httpResult = serviceUrl.GetStreamAsync().Result;
      var ms = new MemoryStream();
      httpResult.CopyTo(ms);
      ms.Position = 0;
      return ms;
    }
  }
}
