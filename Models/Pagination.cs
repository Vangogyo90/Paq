namespace Paq.Models
{
    public partial class Pagination
    {
        public int? TotalItems { get; set; }
        public int? TotalPages { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
        public List<QuantityColor>? QuantityColor { get; set; }
        public List<Discount>? Discounts { get; set; }
    }
}
