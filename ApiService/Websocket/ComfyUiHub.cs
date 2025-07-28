using ApiService.Websocket.Abstract;
using Microsoft.AspNetCore.SignalR;

namespace ApiService.Websocket
{
  public class ComfyUiHub : Hub<IComfyUiHubMethods>
  {
    public override Task OnConnectedAsync()
    {
      return base.OnConnectedAsync();
    }
    public override Task OnDisconnectedAsync(Exception? exception)
    {
      return base.OnDisconnectedAsync(exception);
    }
    
  }
}
