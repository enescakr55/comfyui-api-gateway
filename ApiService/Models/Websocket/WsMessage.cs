namespace ApiService.Models.Websocket
{
  public class WsMessage
  {
    public string Type { get; set; } = "unknown";
    public string? Data { get; set; }
    public string Raw { get; set; } = "";
  }
}
