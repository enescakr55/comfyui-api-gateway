using ApiService.Models.Websocket;
using ApiService.Services;
using Microsoft.AspNetCore.SignalR;
using System.Buffers;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace ApiService.Websocket
{
  public class ComfyUISocketListener
  {
    private readonly IConfiguration _configuration;
    private readonly IHubContext<ComfyUiHub> _comfyUiHub;
    private readonly PromptRestService _promptRestService;
    public ComfyUISocketListener(IConfiguration configuration, IHubContext<ComfyUiHub> comfyUiHub, PromptRestService promptRestService)
    {
      _configuration = configuration;
      _comfyUiHub = comfyUiHub;
      _promptRestService = promptRestService;
    }
    public async Task StartAsync(CancellationToken ct = default)
    {
      var wsClient = new ClientWebSocket();
      var wsUrl = _configuration.GetSection("ComfyUi:Websocket").Value;
      if (wsUrl == null)
      {
        return;
      }
      Debug.WriteLine("start'a girdi");
      var wsClientIdParameter = "?clientId=" + ComfyClientIdService.Instance.GetId();
      var wsConnectUri = new Uri(wsUrl + wsClientIdParameter);

      await wsClient.ConnectAsync(wsConnectUri, ct);
      var listenTask = ListenWebsocketAsync(wsClient, ct);
      await listenTask;

    }
    private async Task ListenWebsocketAsync(ClientWebSocket ws, CancellationToken ct)
    {
      var buffer = ArrayPool<byte>.Shared.Rent(64 * 1024);
      try
      {
        Debug.WriteLine("try a girdi");
        while (!ct.IsCancellationRequested && ws.State == WebSocketState.Open)
        {
          var result = await ws.ReceiveAsync(buffer.AsMemory(), ct);
          if (result.MessageType == WebSocketMessageType.Close)
          {
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", ct);
            break;
          }
          Debug.WriteLine("while'a girdi");
          var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
          var parsed = ParseWsMessage(msg);
          var promptId = ParsePromptId(parsed.Data ?? "{}");
          Debug.WriteLine(parsed.Type);
          Debug.WriteLine(parsed.Raw);
          if (parsed.Type == "progress")
          {
            await _comfyUiHub.Clients.All.SendAsync("ProgressMessage", promptId, parsed.Data, ct);
          }else if(parsed.Type == "status"){
            await _comfyUiHub.Clients.All.SendAsync("StatusMessage", parsed.Data, ct);
          }
          else if (parsed.Type == "execution_end" || parsed.Type == "finished" || parsed.Type == "executed")
          {
            Console.Write(parsed);
            Debug.Write(parsed);


            await _comfyUiHub.Clients.All.SendAsync("ExecutedMessage", promptId, parsed.Data, ct);
          }else if(parsed.Type == "execution_interrupted"){
            await _comfyUiHub.Clients.All.SendAsync("Interrupted", promptId, parsed.Data, ct);
          }else if(parsed.Type == "execution_success"){
            if (promptId != null)
            {
              var output = _promptRestService.GetOutputFileName(promptId);
            }
          }
        }
      }
      catch
      {

      }
      finally
      {
        ArrayPool<byte>.Shared.Return(buffer);
        if (ws.State == WebSocketState.Open)
        {
          await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
        }

      }
    }
    private static string? ParsePromptId(string data)
    {
      using var doc = JsonDocument.Parse(data);
      var root = doc.RootElement;

      if (root.TryGetProperty("prompt_id", out var p))
      {
        return p.ToString();
      }
      return null;
    }
    private static WsMessage ParseWsMessage(string raw)
    {
      try
      {
        using var doc = JsonDocument.Parse(raw);
        var root = doc.RootElement;

        string? type = null;
        if (root.TryGetProperty("type", out var t))
        {
          type = t.GetString();
        }

        string? data = null;
        if (root.TryGetProperty("data", out var d))
        {
          data = d.ToString();
        }
        // ComfyUI tarafında farklı alanlar gelebilir: "data", "status", "node_id", vs.
        return new WsMessage
        {
          Type = type ?? "unknown",
          Raw = raw,
          Data = data
        };
      }
      catch
      {
        return new WsMessage { Type = "invalid_json", Raw = raw };
      }
    }
  }
}
