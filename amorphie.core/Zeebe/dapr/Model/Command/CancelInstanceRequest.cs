using System.ComponentModel.DataAnnotations;

namespace amorphie.core.Zeebe.dapr;

public record CancelInstanceRequest([Required] long? ProcessInstanceKey);