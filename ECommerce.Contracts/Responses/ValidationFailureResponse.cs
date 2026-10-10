using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Contracts.Responses
{
    public class ValidationFailureResponse
    {
        public required IEnumerable<ValidationResponse> Errors { get; set; }    

    }

    public class ValidationResponse
    {
        public required string PropertyName { get; set; }
        public required string Message { get; set; }
    }
}
