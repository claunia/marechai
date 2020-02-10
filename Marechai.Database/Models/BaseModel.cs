namespace Marechai.Database.Models
{
    public class BaseModel<TKey>
    {
        public TKey Id { get; set; }
    }
}