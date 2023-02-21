using amorphie.core.IBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace amorphie.core.Base
{
    public abstract class EntiDtoEntityBaseLogtyBaseLog : IHasKey
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime CreatedAt { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

        public Guid CreatedBy { get; set; }
        public Guid? CreatedByBehalfOf { get; set; }
    }
}
