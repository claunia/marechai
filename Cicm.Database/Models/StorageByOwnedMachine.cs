using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models
{
    public class StorageByOwnedMachine : BaseModel<long>
    {
        public long             OwnedMachineId { get; set; }
        public StorageType      Type           { get; set; }
        public StorageInterface Interface      { get; set; }
        [Range(1, long.MaxValue)]
        public long Capacity { get; set; }

        public virtual OwnedMachine OwnedMachine { get; set; }
    }
}