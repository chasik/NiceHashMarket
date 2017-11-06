using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using NiceHashMarket.Model.Annotations;

namespace NiceHashMarket.Model
{
    public class BlockInfo : INotifyPropertyChanged, ICloneable
    {
        private double _percent;
        private int _difficulty;
        private DateTime _created;
        private string _id;

        public BlockInfo(string id, string percent, double diffDouble, DateTime created)
        {
            Id = id;
            Created = created;
            Percent = -1;
            Difficulty = (int)diffDouble;

            if (double.TryParse(percent, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double parsePercent))
                Percent = parsePercent;
        }

        public string Id
        {
            get => _id;
            set
            {
                _id = value; 
                OnPropertyChanged();
            }
        }

        public DateTime Created
        {
            get => _created;
            set
            {
                _created = value; 
                OnPropertyChanged();
            }
        }

        public double Percent
        {
            get => _percent;
            set
            {
                _percent = value; 
                OnPropertyChanged();
            }
        }

        public int Difficulty
        {
            get => _difficulty;
            set
            {
                _difficulty = value; 
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
