using Microsoft.AspNetCore.Mvc;

namespace ApiService.Models.Requests.ImageToImage
{

  public class GenerateImageToImageRequest
  {
    [FromForm]
    public IFormFile? Image { get; set; }
    [FromForm]
    public IFormFile? Mask { get; set; }
    [FromForm]
    public string Positive { get; set; } = string.Empty;
    [FromForm]
    public string Negative { get; set; } = string.Empty;
  }
}
