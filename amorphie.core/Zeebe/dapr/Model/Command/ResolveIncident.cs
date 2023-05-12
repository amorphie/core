using System.ComponentModel.DataAnnotations;

namespace amorphie.core.Zeebe.dapr;

public record ResolveIncident([Required] long IncidentKey);