using System.Text.Json.Serialization;

namespace ApiService.Models.Responses
{
  public class PromptResponse
  {
    [JsonPropertyName("prompt_id")]
    public string PromptId { get; set; }
    public int Number { get; set; }
    [JsonPropertyName("node_errors")]
    public object? NodeErrors { get; set; }
  }
}
