using Dapr.Client;

namespace amorphie.core;

public static class DaprHelper
{
    public static async Task StartUpCheckAsync(DaprClient client)
    {
        using var tokenSource = new CancellationTokenSource(20000);
        try
        {
            await client.WaitForSidecarAsync(tokenSource.Token);
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("Dapr Sidecar Doesn't Respond. Exception: ", ex);
        }
    }
}
