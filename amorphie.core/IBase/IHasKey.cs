using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace amorphie.core.IBase
{
    public interface IHasKey
    {
        Guid Id { get; set; }
    }
}
