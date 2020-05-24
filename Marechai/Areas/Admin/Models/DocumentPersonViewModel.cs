﻿using System.ComponentModel;

namespace Marechai.Areas.Admin.Models
{
    public class DocumentPersonViewModel : BaseViewModel<int>
    {
        public string Name { get; set; }
        [DisplayName("Linked person")]
        public string Person { get; set; }
        public int? PersonId { get; set; }
    }
}