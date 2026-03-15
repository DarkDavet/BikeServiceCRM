using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessAccountantService.Models
{
    public class RepairItem : INotifyPropertyChanged
    {
        private int _quantity;
        private decimal _price;

        public int? ProductId { get; set; } 
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal PurchasePrice { get; set; } 

        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged("Total"); OnPropertyChanged("Quantity"); }
        }

        public decimal Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged("Total"); OnPropertyChanged("Price"); }
        }

        public decimal Total => Quantity * Price;

        public string TypeDisplay => ProductId.HasValue ? "Запчасть" : "Работа";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }


}
