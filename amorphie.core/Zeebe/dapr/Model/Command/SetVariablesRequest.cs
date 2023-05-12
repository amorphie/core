using System.ComponentModel.DataAnnotations;

namespace amorphie.core.Zeebe.dapr;

public record SetVariablesRequest(
    [Required] long? ElementInstanceKey,
    [Required] object Variables,
    bool? Local);