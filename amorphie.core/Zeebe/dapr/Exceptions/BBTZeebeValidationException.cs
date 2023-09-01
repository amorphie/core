using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace amorphie.core.Zeebe.dapr
{
    public sealed class BBTZeebeValidationException : Exception
    {
        public BBTZeebeValidationException(string exception) : base(exception)
        { }
    }
}
