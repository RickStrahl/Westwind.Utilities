using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using System.Linq;
using System.Text;
using Westwind.Utilities;

namespace Westwind.Data.Test.Models
{
    public class Customer
    {
        public int Id { get; set; }

        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public string Company { get; set; }
        
        public string Address { get; set; }

        public string Password { get; set; }

        public DateTime? LastOrder { get; set; }

        public DateTime Entered { get; set; }
        public DateTime Updated { get; set; }

        public List<Order> Orders { get; set; }

        public Customer()
        {
            Id = (int) Math.Abs(DataUtils.GenerateUniqueNumericId());
            Entered = DateTime.Now;
            Updated = DateTime.Now;
            Orders = new List<Order>();
        }

    }

}
