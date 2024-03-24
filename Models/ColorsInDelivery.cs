using System.ComponentModel.DataAnnotations.Schema;

namespace Paq.Models
{
    public class ColorsInDelivery
    {
        public int Quantity { get; set; }
        public QuantityColor? Color { get; set; }
        public Discount? Discount { get; set; }
    }
}
