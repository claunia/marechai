using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marechai.Database.Models
{
    public class BaseModel<TKey>
    {
        public TKey Id { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedOn { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedOn { get; set; }
    }
}