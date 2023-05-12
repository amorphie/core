using System.ComponentModel.DataAnnotations;

namespace amorphie.core.Zeebe.dapr;

public record ThrowErrorRequest(
    [Required] long? JobKey,
    [Required] string ErrorCode,
    string ErrorMessage);