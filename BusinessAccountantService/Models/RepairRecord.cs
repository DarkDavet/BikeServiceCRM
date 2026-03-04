using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessAccountantService.Models
{
    public class RepairRecord
    {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public string BikeModel { get; set; }
        public string Status { get; set; }
        public double Cost { get; set; }
    }
}
