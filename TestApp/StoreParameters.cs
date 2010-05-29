using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Fsprg
{
    class StoreParameters : INotifyPropertyChanged
    {
        private Dictionary<string, string> _raw = new Dictionary<string,string>();

        public string OrderProcess {
            get { return GetValueOrEmpty("orderProcessType"); }
            set { SetValue("orderProcessType", value); }
        }
        public string StoreId {
            get { return GetValueOrEmpty("storeId"); }
            set { SetValue("storeId", value); }
        }
        public string ProductId {
            get { return GetValueOrEmpty("productId"); }
            set { SetValue("productId", value); }
        }
        public Uri ToURL
        {
            get {
                var uriAsStr = string.Format("http://sites.fastspring.com/{0}/product/{1}?mode=test", StoreId, ProductId); 
                return new Uri(uriAsStr);
            }
        }

        protected string GetValueOrEmpty(string key)
        {
            if (_raw.ContainsKey(key))
            {
                return _raw[key];
            }
            else
            {
                return "";
            }
        }
        protected void SetValue(string key, string value)
        {
            _raw[key] = value;
            ToURLChanged();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void ToURLChanged()
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ToURL"));
            }
        }
    }
}
