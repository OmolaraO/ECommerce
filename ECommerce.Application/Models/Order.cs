using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Models
{
    public class Order
    {
        public required Guid Id { get; set; }
        public required Guid UserId { get; set; }
        public required OrderStatus Status { get; set; }
        public required decimal TotalAmount { get; set; }
        public List<OrderItem> Items { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
