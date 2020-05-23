using System;
using Microsoft.AspNetCore.Identity;

namespace Marechai.Database.Models
{
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() => Created = DateTime.UtcNow;

        public ApplicationRole(string name) : base(name)
        {
            Description = name;
            Created     = DateTime.UtcNow;
        }

        public ApplicationRole(string name, string description) : base(name)
        {
            Description = description;
            Created     = DateTime.UtcNow;
        }

        public string   Description { get; set; }
        public DateTime Created     { get; set; }
    }
}