using System;
using System.ComponentModel;
using System.Windows;

namespace OpcUaClient
{
    /// <summary>
    /// Interaction logic for AutoIncrementTimerDialog.xaml
    /// </summary>
    public partial class AutoIncrementTimerDialog : Window, INotifyPropertyChanged
    {
        private string _incrementInterval = "1";
        private string _incrementTime = "10000";
        private string _incrementWrap = "100";

        public string IncrementInterval
        {
            get { return _incrementInterval; }
            set
            {
                _incrementInterval = value;
                OnPropertyChanged("IncrementInterval");
            }
        }

        public string IncrementTime
        {
            get { return _incrementTime; }
            set
            {
                _incrementTime = value;
                OnPropertyChanged("IncrementTime");
            }
        }

        public string IncrementWrap
        {
            get { return _incrementWrap; }
            set
            {
                _incrementWrap = value;
                OnPropertyChanged("IncrementWrap");
            }
        } 

        public AutoIncrementTimerDialog()
        {
            InitializeComponent();
        }

        private void Ok_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
