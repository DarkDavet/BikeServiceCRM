using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BusinessAccountantService.Models
{
    public class RepairRecord: INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string BikeInfo { get; set; }
        public string ProblemDescription { get; set; }
        public string WorksPerformed { get; set; }
        public double TotalCost { get; set; }
        private string _status;
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(); 
                }
            }
        }
        public DateTime DateCreated { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
