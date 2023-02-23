using Dapr.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace amorphie.core.security.Extensions
{
    public static class Vault
    {
        public static async Task AddVaultSecrets(this IConfigurationBuilder builder, string? secretStoreName, string[] secretPaths)
        {
            if(string.IsNullOrWhiteSpace(secretStoreName))
            {
                throw new SecretException("Secret Store Name couldn't be null or empty string. Provide a valid Secret Store Name");
            }
            try
            {
                var daprClient = new DaprClientBuilder().Build();
                foreach(var secretPath in secretPaths)
                {
                    var secret = await daprClient.GetSecretAsync(secretStoreName,secretPath);
                    
                    builder.AddInMemoryCollection(secret); 
                }
            }
            catch (Exception ex)
            {
                throw new SecretException("An Error Occured At Dapr Secret Store. Detail:"+ ex);
            }
        }

        public class SecretException : Exception
        {
            public SecretException(string message) : base(message) { }
        }
    }
}
