using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Models
{
    public class Product
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required decimal Price { get; set; }
        public required int StockQuantity { get; set; }
        public required ProductCategory Category { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
