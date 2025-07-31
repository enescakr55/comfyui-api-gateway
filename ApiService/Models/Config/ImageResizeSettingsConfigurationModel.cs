namespace ApiService.Models.Config
{
  public class ImageResizeSettingsConfigurationModel
  {
    public bool Enabled { get; set; }
    public uint MaxWidth { get; set; }
    public uint MaxHeight { get; set; }
  }
}
