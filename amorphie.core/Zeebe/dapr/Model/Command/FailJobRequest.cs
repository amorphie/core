using System.ComponentModel.DataAnnotations;

namespace amorphie.core.Zeebe.dapr;

public record FailJobRequest(
    [Required] long? JobKey,
    [Required] int? Retries,
    string ErrorMessage);