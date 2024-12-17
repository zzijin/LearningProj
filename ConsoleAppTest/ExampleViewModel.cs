using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoNotify;
using AutoSetProperty;

namespace ConsoleAppTest
{
    public partial class AutoNotifyViewModel : INotifyPropertyChanged
    {
        [AutoNotify]
        private string _text = "private field text";

        [AutoNotify(PropertyName = "Count")]
        private int _amount = 5;

        public event PropertyChangedEventHandler PropertyChanged;
    }

    [AutoSetProperty]
    public partial class AutoSetPropertyViewModel : INotifyPropertyChanged
    {
        [AutoNotify]
        private string _text = "private field text";

        [AutoNotify(PropertyName = "Count")]
        private int _amount = 5;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
