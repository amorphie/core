using System.Collections.Generic;

namespace amorphie.core.Zeebe.dapr;

public record CreateInstanceRequest(
    string BpmnProcessId,
    long? ProcessDefinitionKey,
    int? Version,
    Dictionary<string, string> Variables);