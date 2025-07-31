using System.Text.Json;

namespace ApiService.Services
{
  public class PromptIndexerService
  {
    private readonly string TEXT_TO_IMAGE = "text-to-image";
    private readonly string IMAGE_TO_IMAGE = "image-to-image";
    private readonly string TEXT_TO_VIDEO = "text-to-video";

    private static PromptIndexerService _instance;
    private static readonly object _lock = new();
    private Dictionary<string, string> _text_to_image_prompts = new();
    private Dictionary<string, string> _image_to_image_prompts = new();
    private Dictionary<string, string> _text_to_video_prompts = new();
    public static PromptIndexerService Instance
    {
      get
      {
        lock (_lock)
        {
          return _instance ??= new PromptIndexerService();
        }
      }
    }
    private PromptIndexerService() { }
    public void InitializePrompts(string promptPath)
    {
      if (Directory.Exists(promptPath))
      {
        var textToImagePath = Path.Combine(promptPath, TEXT_TO_IMAGE);
        var imageToImagePath = Path.Combine(promptPath, IMAGE_TO_IMAGE);
        var textToVideoPath = Path.Combine(promptPath, TEXT_TO_VIDEO);
        SetPrompts(_text_to_video_prompts, textToVideoPath);
        SetPrompts(_image_to_image_prompts, imageToImagePath);
        SetPrompts(_text_to_image_prompts, textToImagePath);
      }
    }
    private void SetPrompts(Dictionary<string, string> target, string path)
    {
      if (Directory.Exists(path))
      {
        var files = Directory.GetFiles(path).Where(x => IsJson(x)).ToList();
        foreach (var file in files)
        {
          var fileName = Path.GetFileNameWithoutExtension(file);
          var strContent = File.ReadAllText(file,System.Text.UTF8Encoding.UTF8).Replace(Environment.NewLine,"");
          var jsonObject = JsonSerializer.Deserialize<object>(strContent);
          var content = JsonSerializer.Serialize(jsonObject, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
          target.Add(fileName, content);
        }
      }
    }
    private bool IsJson(string filePath)
    {
      var ext = Path.GetExtension(filePath).ToLower();
      if (ext == ".json")
      {
        return true;
      }
      return false;
    }
    public IDictionary<string, string> GetTextToImagePrompts()
    {
      return _text_to_image_prompts;
    }
    public IDictionary<string, string> GetImageToImagePrompts()
    {
      return _image_to_image_prompts;
    }
    public IDictionary<string, string> GetTextToVideoPrompts()
    {
      return _text_to_video_prompts;
    }
    public string? GetTextToImagePrompt(string promptName){
      var isAvailabe = _text_to_image_prompts.TryGetValue(promptName, out var content);
      return content;
    }   
    public string? GetImageToImagePrompt(string promptName){
      var isAvailabe = _image_to_image_prompts.TryGetValue(promptName, out var content);
      return content;
    }
    public string? GetTextToVideoPrompt(string promptName){
      var isAvailabe = _text_to_video_prompts.TryGetValue(promptName, out var content);
      return content;
    }

  }
}
