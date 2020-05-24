using System;
using Microsoft.AspNetCore.Identity;

namespace Marechai.Database.Models
{
    public class ApplicationRole : IdentityRole
    {
        public const string ROLE_UBERADMIN = "UberAdmin";
        public const string ROLE_WRITER = "Writer";
        public const string ROLE_PROOFREADER = "Proofreader";
        public const string ROLE_TRANSLATOR = "Translator";
        public const string ROLE_SUPERTRANSLATOR = "SuperTranslator";
        public const string ROLE_COLLABORATOR = "Collaborator";
        public const string ROLE_CURATOR = "Curator";
        public const string ROLE_PHYSICALCURATOR = "PhysicalCurator";
        public const string ROLE_TECHNICIAN = "Technician";
        public const string ROLE_SUPERTECHNICIAN = "SuperTechnician";
        public const string ROLE_ADMIN = "Administrator";
        public const string ROLE_NONE = "NormalUser";

        
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