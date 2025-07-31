using ApiService.Models.Config;
using ImageMagick;

namespace ApiService.Services
{
  public class ImageProcessService
  {
    private readonly IConfiguration _configuration;

    public ImageProcessService(IConfiguration configuration)
    {
      _configuration = configuration;
    }

    public Stream ResizeImageIfLimitsExceed(Stream input){
      var config = new ImageResizeSettingsConfigurationModel();
      _configuration.GetSection("ImageResizeSettings").Bind(config);
      
      var outputStream = new MemoryStream();
      using(var image = new MagickImage(input)){
        if (config.Enabled)
        {
          if (image.Width > config.MaxWidth || image.Height > config.MaxHeight)
          {
            var size = new MagickGeometry(config.MaxWidth,config.MaxHeight)
            {
              IgnoreAspectRatio = false
            };
            image.Resize(size);
          }
        }
        image.Strip();
        image.Write(outputStream);
        outputStream.Position = 0;
        return outputStream;
      }

      
    }
  }
}
