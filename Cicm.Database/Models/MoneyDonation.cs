namespace Cicm.Database.Models
{
    public class MoneyDonation
    {
        public int     Id       { get; set; }
        public string  Donator  { get; set; }
        public decimal Quantity { get; set; }
    }
}