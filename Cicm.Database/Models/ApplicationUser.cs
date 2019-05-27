using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Cicm.Database.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<MachinePhoto> Photos { get; set; }
    }
}