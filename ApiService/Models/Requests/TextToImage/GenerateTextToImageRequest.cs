namespace ApiService.Models.Requests.TextToImage
{
  public class GenerateTextToImageRequest
  {
    
    public string Positive { get; set; } = string.Empty;
    public string Negative { get; set; } = string.Empty;
  }
}
