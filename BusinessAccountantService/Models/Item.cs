using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessAccountantService.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public double PurchasePrice { get; set; }
        public double RetailPrice { get; set; }
        public string Category {  get; set; }
    }
}
