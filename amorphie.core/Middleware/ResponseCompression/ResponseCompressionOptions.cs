namespace amorphie.core.Middleware.ResponseCompression;

public class ResponseCompressionOptions
{
    public bool Enable { get; set; } = false;
    public List<string> Providers { get; set; } = ["gzip"];
    public bool EnableForHttps { get; set; }
    public List<string> MimeTypes { get; set; } = new();
    public List<string> ExcludedMimeTypes { get; set; } = new();
}