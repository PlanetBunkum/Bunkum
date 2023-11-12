using Bunkum.Core.Services;
using Cottle;
using NotEnoughLogs;

namespace Bunkum.Cottle.Services;

public class CottleService : Service
{
    private readonly DocumentConfiguration _configuration;

    public IDocument CompiledDocument;

    internal CottleService(Logger logger, DocumentConfiguration configuration) : base(logger)
    {
        this._configuration = configuration;
        this.CompiledDocument = this.CompileTemplate("test");
    }

    private IDocument CompileTemplate(string template)
    {
        return Document.CreateDefault(template, this._configuration).DocumentOrThrow;
    }
}