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
        public int ClientId { get; set; }
        public string BikeInfo { get; set; }
        public string ProblemDescription { get; set; }
        public string WorksPerformed { get; set; }
        public double TotalCost { get; set; }
        public string Status { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
