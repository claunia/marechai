using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Marechai.Database.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<MachinePhoto>      Photos             { get; set; }
        public virtual ICollection<OwnedMachinePhoto> OwnedMachinePhotos { get; set; }
        public virtual ICollection<OwnedMachine>      OwnedMachines      { get; set; }
    }
}