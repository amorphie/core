namespace amorphie.core.Middleware.Logging;
public class LoggingOptions
{
    public const string Logging = "Logging";
    public const string AmorphieLogging = "AmorphieLogging";
    public LoggingRouteOptions Default { get; set; } = default!;
    public List<LoggingRouteOptions> Routes { get; set; } = default!;

    public string[]? IgnorePaths { get; set; }
    //Request and Response body will be ignored by X-Workflow-Name header
    public string[] IgnoreContentByWorkflowName { get; set; } = default!;

}

public class LoggingRouteOptions
{
    public string Regex { get; set; } = default!;
    public bool LogAll { get; set; } = false;
    public bool LogResponse { get; set; } = false;
    public bool LogRequest { get; set; } = true;
    public bool LogHeader { get; set; } = true;
    public string[]? IgnoreFields { get; set; }
    public string[]? LogFields { get; set; }

}

