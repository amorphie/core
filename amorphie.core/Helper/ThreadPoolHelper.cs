using System.Text.Json.Nodes;

namespace amorphie.core;

public static class ThreadPoolHelper
{
    public static void ConfigureThreadPool(string configFilePath = "runtimeconfig.json")
    {
        try
        {
            if (!File.Exists(configFilePath))
                return;
            
            var configContent = File.ReadAllText(configFilePath);
            var jsonNode = JsonNode.Parse(configContent);
            
            var minThreadsNode =
                jsonNode?["runtimeOptions"]?["configProperties"]?["System.Threading.ThreadPool.MinThreads"];
            if (minThreadsNode != null && int.TryParse(minThreadsNode.ToString(), out int minThreads))
            {
                ThreadPool.SetMinThreads(minThreads, minThreads);
            }
        }
        catch
        {
            // No OP
        }
    }
}
