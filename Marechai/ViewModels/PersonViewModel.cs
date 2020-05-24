using System;

namespace Marechai.ViewModels
{
    public class PersonViewModel : BaseViewModel<int>
    {
        public string    Name           { get; set; }
        public string    Surname        { get; set; }
        public string    CountryOfBirth { get; set; }
        public DateTime  BirthDate      { get; set; }
        public DateTime? DeathDate      { get; set; }
        public string    Webpage        { get; set; }
        public string    Twitter        { get; set; }
        public string    Facebook       { get; set; }
        public Guid      Photo          { get; set; }
        public string    Alias          { get; set; }
        public string    DisplayName    { get; set; }

        public string FullName => DisplayName ?? Alias ?? $"{Name} {Surname}";
    }
}