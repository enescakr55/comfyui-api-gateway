namespace ApiService.Services
{
  public class ComfyClientIdService
  {
    private string? ComfyClientId;
    public static ComfyClientIdService _instance = new();
    public static ComfyClientIdService Instance
    {
      get {
        return _instance ??= new();
      }
    }
    public string GetId(){
      return ComfyClientId ??= Guid.NewGuid().ToString("N");
    }
  }
}
