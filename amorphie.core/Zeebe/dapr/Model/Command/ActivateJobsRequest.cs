using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace amorphie.core.Zeebe.dapr;

public record ActivateJobsRequest(
    [Required] string JobType,
    [Required] int? MaxJobsToActivate,
    string Timeout,
    string WorkerName,
    IList<string> FetchVariables);