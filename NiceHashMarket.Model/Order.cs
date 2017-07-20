using System.ComponentModel;
using System.Runtime.CompilerServices;
using NiceHashMarket.Core.Interfaces;
using NiceHashMarket.Model.Annotations;

namespace NiceHashMarket.Model
{
    public class Order : IHaveId, INotifyPropertyChanged
    {
        private decimal _price;
        private decimal _amount;
        private decimal _speed;
        private int _workers;
        private bool _active;

        public Order(int id, decimal price, decimal amount, decimal speed, int workers, int type, int active, ServerEnum server = ServerEnum.Unknown)
        {
            Server = server;

            Id = id;
            Price = price;
            Amount = amount;
            Speed = speed;
            Workers = workers;
            Active = active == 1;

            if (type == 0)
                Type = OrderTypeEnum.Standart;
            else if (type == 1)
                Type = OrderTypeEnum.Fixed;
        }


        public int Id { get; set; }

        public decimal Price
        {
            get => _price;
            set
            {
                if (_price == value) return;

                _price = value;
                OnPropertyChanged();
            }
        }

        public decimal Amount
        {
            get => _amount;
            set
            {
                if (_amount == value) return;

                _amount = value;
                OnPropertyChanged();
            }
        }

        public decimal Speed
        {
            get => _speed;
            set
            {
                if (_speed == value) return;

                _speed = value;
                OnPropertyChanged();
            }
        }

        public int Workers
        {
            get => _workers;
            set
            {
                if (_workers == value) return;

                _workers = value;
                OnPropertyChanged();
            }
        }

        public bool Active
        {
            get => _active;
            set
            {
                if (_active == value) return;

                _active = value;
                OnPropertyChanged();
            }
        }

        public ServerEnum Server { get; set; }

        public OrderTypeEnum Type { get; set; }


        public override string ToString()
        {
            return $"Id={Id};\tPrice={Price};\tAmount={Amount};\tWorkers={Workers};\tSpeed={Speed};\tActive={Active}";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
