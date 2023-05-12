using System.ComponentModel.DataAnnotations;

namespace amorphie.core.Zeebe.dapr;

public record UpdateJobRetriesRequest(
    [Required] long? JobKey,
    int? Retries);