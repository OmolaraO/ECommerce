using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Models
{
    public class OrderItem
    {
        public required Guid Id { get; set; }
        public required Guid OrderId { get; set; }
        public required Guid ProductId { get; set; }
        public required string ProductName { get; set; }
        public required int Quantity { get; set; }
        public required decimal PriceAtPurchase { get; set; }
    }
}
