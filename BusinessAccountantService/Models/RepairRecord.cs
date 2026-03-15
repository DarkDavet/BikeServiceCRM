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
    public class RepairRecord : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string BikeInfo { get; set; }
        public string ProblemDescription { get; set; }
        public string WorksPerformed { get; set; }

        private decimal _partsCost;
        public decimal PartsCost
        {
            get => _partsCost;
            set { _partsCost = value; OnPropertyChanged(); OnPropertyChanged(nameof(Profit)); }
        }

        private decimal _totalCost;
        public decimal TotalCost
        {
            get => _totalCost;
            set { _totalCost = value; OnPropertyChanged(); OnPropertyChanged(nameof(Profit)); }
        }

        // Это свойство теперь будет само уведомлять таблицу, 
        // если изменились TotalCost или PartsCost
        public decimal Profit => TotalCost - _partsCost;

        private string _status;
        public string Status
        {
            get => _status;
            set { if (_status != value) { _status = value; OnPropertyChanged(); } }
        }

        public DateTime DateCreated { get; set; }
        public string DateFormatted => DateCreated.ToString("dd.MM.yyyy HH:mm");
        public DateTime? DateClosed { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

}
