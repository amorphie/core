using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace amorphie.core.Base
{
    public abstract class Translation : EntityBase
    {
        public string Language { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}
