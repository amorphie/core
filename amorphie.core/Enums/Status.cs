using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace amorphie.core.Enums
{
    public enum Status
    {
        [Description("Tüm başarılı işlemler için kullanılır")]
        Success = 1,
        [Description("Tüm hatalı ve uyarı işlemleri için kullanılır")]
        Error = 2,
    }
}
