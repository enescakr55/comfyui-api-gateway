namespace ApiService.Websocket.Abstract
{
  public interface IComfyUiHubMethods
  {
    public Task ExecutedMessage(string promptId,string data);
    public Task StatusMessage(string data);
    public Task ProgressMessage(string promptId, string data);
    public Task ErrorMessage(string promptId,string data);

  }
}
