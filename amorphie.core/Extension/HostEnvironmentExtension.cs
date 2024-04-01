namespace amorphie.core.Extension;
public static class HostEnvironmentExtension
{
    private const string Prod = "Prod";
    public static bool IsProd(this IHostEnvironment hostEnvironment)
    {
        if (hostEnvironment == null)
            throw new ArgumentNullException(nameof(hostEnvironment));
        return hostEnvironment.IsEnvironment(Prod);
    }
}

