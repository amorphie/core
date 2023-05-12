using System.ComponentModel.DataAnnotations;

namespace amorphie.core.Zeebe.dapr;

public record CompleteJobRequest(
    [Required] long? JobKey,
    object Variables);