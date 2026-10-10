using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Models
{
    public class Cart
    {
        public required Guid Id { get; set; }
        public required Guid UserId { get; set; }
        public List<CartItem> Items { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount => Items.Sum(x => x.Price * x.Quantity);
    }
}
