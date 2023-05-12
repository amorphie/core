namespace amorphie.core.Zeebe.dapr;

public record CreateInstanceResponse(
    long? ProcessDefinitionKey,
    string BpmnProcessId,
    int? Version,
    long? ProcessInstanceKey);