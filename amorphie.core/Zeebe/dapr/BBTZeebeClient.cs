using Dapr.Client;

namespace amorphie.core.Zeebe.dapr
{
    public class BBTZeebeClient
    {
        private readonly DaprClient _daprClient;

        public BBTZeebeClient(DaprClient daprClient)
        {
            _daprClient = daprClient;
        }

        public async ValueTask<TopologyResponse> Topology(string bindingName)
        {
            return await _daprClient.InvokeBindingAsync<object, TopologyResponse>(bindingName, Commands.Topology, new { });
        }

        public async ValueTask<CreateInstanceResponse> CreateInstance(string bindingName,CreateInstanceRequest request)
        {
            if (request.BpmnProcessId == null && request.ProcessDefinitionKey == null)
            {
                throw new BBTZeebeValidationException("Either a bpmnProcessId or a processDefinitionKey must be given");
            }

            var result = await _daprClient.InvokeBindingAsync<CreateInstanceRequest, CreateInstanceResponse>(
                bindingName, Commands.CreateInstance, request);

            return result;
        }

        public async ValueTask CancelInstance(string bindingName,CancelInstanceRequest request)
        {
            await _daprClient.InvokeBindingAsync(bindingName, Commands.CancelInstance, request);
        }

        public async ValueTask<SetVariablesResponse> SetVariables(string bindingName, SetVariablesRequest request)
        {
            return await _daprClient.InvokeBindingAsync<SetVariablesRequest, SetVariablesResponse>(
                bindingName, Commands.SetVariables, request);
        }


        public async ValueTask ResolveIncident(string bindingName, ResolveIncident request)
        {
            await _daprClient.InvokeBindingAsync(bindingName, Commands.ResolveIncident, request);
        }

        public async ValueTask<PublishMessageResponse> PublishMessage(string bindingName, PublishMessageRequest request)
        {
            return await _daprClient.InvokeBindingAsync<PublishMessageRequest, PublishMessageResponse>(
               bindingName, Commands.PublishMessage, request);
        }


        public async ValueTask<IList<ActivatedJob>> ActivateJobs(string bindingName, ActivateJobsRequest request)
        {
            return await _daprClient.InvokeBindingAsync<ActivateJobsRequest, IList<ActivatedJob>>(
               bindingName, Commands.ActivateJobs, request);
        }


        public async ValueTask CompleteJob(string bindingName, CompleteJobRequest request)
        {
            await _daprClient.InvokeBindingAsync(bindingName, Commands.CompleteJob, request);
        }

        public async ValueTask FailJob(string bindingName, FailJobRequest request)
        {
            await _daprClient.InvokeBindingAsync(bindingName, Commands.FailJob, request);
        }

        public async ValueTask UpdateJobRetries(string bindingName, UpdateJobRetriesRequest request)
        {
            await _daprClient.InvokeBindingAsync(bindingName, Commands.UpdateJobRetries, request);
        }

        public async ValueTask ThrowError(string bindingName, ThrowErrorRequest request)
        {
            await _daprClient.InvokeBindingAsync(bindingName, Commands.ThrowError, request);
        }
    }
}
