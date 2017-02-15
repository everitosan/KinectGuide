﻿using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinnectGuide
{
    class MainWindowViewModel :INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string connectionIdValue;
        public string ConnectionID {
            get
            {
                return connectionIdValue;
            }
            set
            {
                if (this.connectionIdValue != value)
                {
                    this.connectionIdValue = value;
                    this.OnNotifyPropertyChange("ConnectionID");
                }
            }
        }

        public void OnNotifyPropertyChange(string PropertyName)
        {
            if (this.PropertyChanged != null) {
                this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(PropertyName));
            }
        }
    }
}
