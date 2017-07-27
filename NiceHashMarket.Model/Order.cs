using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using NiceHashMarket.Logger;
using NiceHashMarket.Model.Enums;
using NiceHashMarket.Model.Interfaces;

namespace NiceHashMarket.Model
{
    [DataContract]
    public class Order : IHaveId, ICloneable, INotifyPropertyChanged
    {
        private int _id;
        private decimal _price;
        private decimal _amount;
        private decimal _speed;
        private int _workers;
        private bool _active;
        private decimal _deltaPrice;
        private int _deltaPercentAmount;
        private int _deltaPercentWorkers;
        private int _deltaPercentSpeed;
        private OrderTypeEnum _type;
        private DateTime? _priceChanged;

        public Order(int id, decimal price, decimal amount, decimal speed, int workers, int type, int active, ServerEnum server = ServerEnum.Unknown)
        {
            Server = server;

            _id = id;
            _price = price;
            _amount = amount;
            _speed = speed;
            _workers = workers;
            _active = active == 1;

            if (type == 0)
                _type = OrderTypeEnum.Standart;
            else if (type == 1)
                _type = OrderTypeEnum.Fixed;

            //MarketLogger.Information("counstructor: {@orderId})", Id);
        }

        [DataMember]
        public int Id
        {
            get => _id;
            set
            {
                if (_id == value) return;

                _id = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public decimal Price
        {
            get => _price;
            set
            {
                //MarketLogger.Information("set price 1: {@orderId} price: {@price} value: {@value})", Id, _price, value);
                if (_price == value) return;

                if (_price != 0)
                    DeltaPrice = value - _price;

                //MarketLogger.Information("set price 2: {@orderId} price: {@price} value: {@value})", Id, _price, value);
                _price = value;
                _priceChanged = DateTime.Now;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public decimal DeltaPrice
        {
            get => _deltaPrice;
            set
            {
                //MarketLogger.Information("set deltaPrice 1: {@orderId} deltaPrice: {@deltaPrice} value: {@value})", Id, _deltaPrice, value);
                if (_deltaPrice == value || value == 0) return;

                //MarketLogger.Information("set deltaPrice 2: {@orderId} deltaPrice: {@deltaPrice} value: {@value})", Id, _deltaPrice, value);
                _deltaPrice = value; 
                OnPropertyChanged();
            }
        }

        [DataMember]
        public DateTime? PriceChanged
        {
            get => _priceChanged;
            set
            {
                if (_priceChanged == value) return;

                _priceChanged = value; 

                OnPropertyChanged();
            }
        }

        [DataMember]
        public decimal Amount
        {
            get => _amount;
            set
            {
                if (_amount == value) return;

                DeltaPercentAmount = (int)Math.Round(_amount == 0 ? value : value * 100 / _amount - 100);

                _amount = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public int DeltaPercentAmount
        {
            get => _deltaPercentAmount;
            set
            {
                if (_deltaPercentAmount == value) return;

                _deltaPercentAmount = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public decimal Speed
        {
            get => _speed;
            set
            {
                if (_speed == value) return;

                DeltaPercentSpeed = (int)Math.Round(_speed == 0 ? value : value * 100 / _speed - 100);

                _speed = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public int DeltaPercentSpeed
        {
            get => _deltaPercentSpeed;
            set
            {
                if (_deltaPercentSpeed == value) return;

                _deltaPercentSpeed = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public int Workers
        {
            get => _workers;
            set
            {
                if (_workers == value) return;

                DeltaPercentWorkers = _workers == 0 ? value : value * 100 / _workers - 100 ;

                _workers = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public int DeltaPercentWorkers
        {
            get => _deltaPercentWorkers;
            set
            {
                if (_deltaPercentWorkers == value) return;

                _deltaPercentWorkers = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
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

        [DataMember]
        public OrderTypeEnum Type
        {
            get => _type;
            set
            {
                if (_type == value) return;

                _type = value; 
                OnPropertyChanged();
            }
        }

        [DataMember]
        public ServerEnum Server { get; set; }


        public override string ToString()
        {
            return $"Id={Id};\tPrice={Price};\tDeltaPrice={DeltaPrice};\tAmount={Amount};\tWorkers={Workers};\tSpeed={Speed};\tActive={Active}";
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
