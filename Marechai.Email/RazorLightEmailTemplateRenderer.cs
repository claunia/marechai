using RazorLight;

namespace Marechai.Email;

/// <summary>
///     <see cref="IEmailTemplateRenderer" /> implementation backed by RazorLight, with templates loaded as
///     embedded resources from this assembly. Caches a single <see cref="RazorLightEngine" /> for the lifetime of
///     the process.
/// </summary>
public sealed class RazorLightEmailTemplateRenderer : IEmailTemplateRenderer
{
    readonly RazorLightEngine _engine;

    public RazorLightEmailTemplateRenderer() =>
        _engine = new RazorLightEngineBuilder()
                 .UseEmbeddedResourcesProject(typeof(RazorLightEmailTemplateRenderer).Assembly, "Marechai.Email.Templates")
                 .UseMemoryCachingProvider()
                 .Build();

    public Task<string> RenderAsync<TModel>(string templateKey, TModel model) =>
        _engine.CompileRenderAsync(templateKey, model);
}
