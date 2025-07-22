namespace ApiService
{
  public static class ComfyUiEndpoints
  {
    public static WebApplication BuildEndpoints(this WebApplication app)
    {
      app.MapGet("api/v1/comfy-ui/schemes/list", (HttpContext httpContext) =>
      {
        
      });
      app.MapPost("/api/v1/comfy-ui/schemes/{scheme}/images/generate", (HttpContext httpContext,string scheme) =>
      {
        return Results.Ok(scheme);
      });
      return app;
    }
  }
}
