using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Marechai.Database;

namespace Marechai.Areas.Admin.Models
{
    public class CompanyViewModel : BaseViewModel<int>
    {
        public string Name { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true), DataType(DataType.Date)]
        public DateTime? Founded { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}"), DataType(DataType.Date)]
        public DateTime? Sold { get; set; }
        [DisplayName("Sold to")]
        public string SoldTo { get;  set; }
        public string Country { get; set; }
        [Required]
        public CompanyStatus Status { get; set; }

        [DisplayName("Sold"), NotMapped]
        public string SoldView => Status != CompanyStatus.Active && Status != CompanyStatus.Unknown
                                      ? Sold is null
                                            ? "Unknown"
                                            : Sold.Value.ToShortDateString()
                                      : Sold is null
                                          ? SoldTo is null
                                                ? ""
                                                : "Unknown"
                                          : Sold.Value.ToShortDateString();
    }
}