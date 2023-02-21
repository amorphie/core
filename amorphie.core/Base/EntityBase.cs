using amorphie.core.IBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace amorphie.core.Base
{
    public abstract class EntityBase : EntityBaseWithOutId, IHasKey
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}
