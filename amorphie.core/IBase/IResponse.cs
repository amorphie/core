using amorphie.core.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace amorphie.core.IBase
{
    public interface IResponse
    {
        Result Result { get; set; }
    }
    public interface IResponse<T>
    {
        Result Result { get; set; }
    }
}
