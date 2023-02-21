using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace amorphie.core.Enums
{
    [Flags]
    public enum StatusType
    {
        New = 2,
        Active = 4,
        InProgress = 8,
        Passive = 16,
        Completed = 32,
        LockedInFlow = 256,
        All = New | Active | InProgress | Passive
    }
}
