using Dapr.Client;
using Dapr.Client.Autogen.Grpc.v1;

namespace amorphie.core.Service
{
    public class BBTDapperService
    {
        protected readonly DaprClient _daprClient;
        public BBTDapperService(DaprClient daprClient) {
            _daprClient = daprClient;
        }
        public async ValueTask<TResponse> Get<TResponse,TRequest>(TRequest request, string appId, string methodName) {
            Dictionary<string, string> keyParameters = new Dictionary<string, string>();
            var test = _daprClient.CreateInvokeMethodRequest<TRequest>(HttpMethod.Get, appId, methodName, keyParameters, request);
            return await _daprClient.InvokeMethodAsync<TResponse>(test);
        }
        public async ValueTask<TResponse> Get<TResponse>(string appId, string methodName)
        {
            var test = _daprClient.CreateInvokeMethodRequest(HttpMethod.Get, appId, methodName);
            return await _daprClient.InvokeMethodAsync<TResponse>(test);
        }
        public async ValueTask<TResponse> Post<TResponse, TRequest>(TRequest request, string appId, string methodName)
        {
            Dictionary<string, string> keyParameters = new Dictionary<string, string>();
            var test = _daprClient.CreateInvokeMethodRequest<TRequest>(HttpMethod.Post, appId, methodName, keyParameters, request);
            return await _daprClient.InvokeMethodAsync<TResponse>(test);
        }
        public async ValueTask<TResponse> Post<TResponse>(string appId, string methodName)
        {
            var test = _daprClient.CreateInvokeMethodRequest(HttpMethod.Post, appId, methodName);
            return await _daprClient.InvokeMethodAsync<TResponse>(test);
        }


        
    }
}
