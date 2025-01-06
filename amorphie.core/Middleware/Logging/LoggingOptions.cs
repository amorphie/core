namespace amorphie.core.Middleware.Logging;
public class LoggingOptions
{
    public const string Logging = "Logging";
    public const string AmorphieLogging = "AmorphieLogging";
    public bool LogResponse { get; set; } = false;
    public bool LogRequest { get; set; } = true;
    public string[]? SanitizeHeaderNames { get; set; }
    public string[]? SanitizeFieldNames { get; set; }
    public string[]? IgnorePaths { get; set; }
    //Response body will be ignored by path
    public string[] IgnoreResponseByPaths { get; set; } = default!;
    //Request and Response body will be ignored by X-Workflow-Name header
    public string[] IgnoreContentByWorkflowName { get; set; } = default!;
}

