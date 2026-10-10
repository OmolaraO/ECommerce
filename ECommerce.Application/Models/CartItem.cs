using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Models
{
    public class CartItem
    {
        public required Guid Id { get; set; }
        public required Guid CartId { get; set; }
        public required Guid ProductId { get; set; }
        public required string ProductName { get; set; }
        public required decimal Price { get; set; }
        public required int Quantity { get; set; }
    }
}
