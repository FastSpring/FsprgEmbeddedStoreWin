using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace FsprgEmbeddedStore
{

    public class StoreParameters : INotifyPropertyChanged
    {
        private const string LANGUAGE = "language";
        private const string ORDER_PROCESS_TYPE = @"orderProcessType";
        private const string STORE_ID = @"storeId";
        private const string PRODUCT_ID = @"productId";
        private const string MODE = @"mode";
        private const string CAMPAIGN = @"campaign";
        private const string OPTION = @"option";
        private const string REFERRER = @"referrer";
        private const string SOURCE = @"source";
        private const string CONTACT_FNAME = @"contact_fname";
        private const string CONTACT_LNAME = @"contact_lname";
        private const string CONTACT_EMAIL = @"contact_email";
        private const string CONTACT_COMPANY = @"contact_company";
        private const string CONTACT_PHONE = @"contact_phone";

        private readonly Dictionary<string, string> _raw = new Dictionary<string, string>(15);

        public Dictionary<string, string> Raw {
            get { return _raw; }
        }

        public string Language {
            get { return GetValue(LANGUAGE); }
            set { SetValue(LANGUAGE, value); }
        }
        public OrderProcessType OrderProcessType {
            get { return OrderProcessType.Parse(GetValue(ORDER_PROCESS_TYPE)); }
            set { SetValue(ORDER_PROCESS_TYPE, value.ToString()); }
        }
        public string StoreId {
            get { return GetValue(STORE_ID); }
            set { SetValue(STORE_ID, value); }
        }
        public string ProductId {
            get { return GetValue(PRODUCT_ID); }
            set { SetValue(PRODUCT_ID, value); }
        }
        public Mode Mode {
            get { return Mode.Parse(GetValue(MODE)); }
            set { SetValue(MODE, value.ToString()); }
        }
        public string Campaign {
            get { return GetValue(CAMPAIGN); }
            set { SetValue(CAMPAIGN, value); }
        }
        public string Option {
            get { return GetValue(OPTION); }
            set { SetValue(OPTION, value); }
        }
        public string Referrer {
            get { return GetValue(REFERRER); }
            set { SetValue(REFERRER, value); }
        }
        public string Source {
            get { return GetValue(SOURCE); }
            set { SetValue(SOURCE, value); }
        }

        public bool HasContactDefaults() {
            return Raw.ContainsKey(CONTACT_FNAME) ||
                Raw.ContainsKey(CONTACT_LNAME) ||
                Raw.ContainsKey(CONTACT_EMAIL) ||
                Raw.ContainsKey(CONTACT_COMPANY) ||
                Raw.ContainsKey(CONTACT_PHONE);
        }
        public string ContactFname {
            get { return GetValue(CONTACT_FNAME); }
            set { SetValue(CONTACT_FNAME, value); }
        }
        public string ContactLname {
            get { return GetValue(CONTACT_LNAME); }
            set { SetValue(CONTACT_LNAME, value); }
        }
        public string ContactEmail {
            get { return GetValue(CONTACT_EMAIL); }
            set { SetValue(CONTACT_EMAIL, value); }
        }
        public string ContactCompany {
            get { return GetValue(CONTACT_COMPANY); }
            set { SetValue(CONTACT_COMPANY, value); }
        }
        public string ContactPhone {
            get { return GetValue(CONTACT_PHONE); }
            set { SetValue(CONTACT_PHONE, value); }
        }

        public Uri ToURL {
            get {
                string storeIdEncoded = StoreId;
                if(storeIdEncoded == null) {
                    storeIdEncoded = "";
                } else {
                    storeIdEncoded = Uri.EscapeDataString(storeIdEncoded);
                }
                string productIdEncoded = ProductId;
                if(productIdEncoded == null) {
                    productIdEncoded = "";
                } else {
                    productIdEncoded = Uri.EscapeDataString(productIdEncoded);
                }

                StringBuilder urlAsStr = new StringBuilder(1024);
                OrderProcessType type = OrderProcessType;
                if(type == OrderProcessType.Detail) {
                    string scheme = Uri.UriSchemeHttp;
                    if(HasContactDefaults()) {
                        scheme = Uri.UriSchemeHttps;
                    }
                    urlAsStr.AppendFormat("{0}://sites.fastspring.com/{1}/product/{2}", scheme, storeIdEncoded, productIdEncoded);
                } else if(type == OrderProcessType.Instant) {
                    urlAsStr.AppendFormat("https://sites.fastspring.com/{0}/instant/{1}", storeIdEncoded, productIdEncoded);
                } else {
                    throw new Exception("OrderProcessType '"+type+"' unknown.");
                }

                StringBuilder queryStr = new StringBuilder(256);
                foreach(string key in Raw.Keys) {
                    if (key.Equals(ORDER_PROCESS_TYPE)) continue;
                    if (key.Equals(STORE_ID)) continue;
                    if (key.Equals(PRODUCT_ID)) continue;

                    string value = Raw[key];
                    if (value != null) {
                        queryStr.AppendFormat("&{0}={1}", key, Uri.EscapeDataString(value));
                    }
                }

                if (queryStr.Length > 0) {
                    urlAsStr.Append('?');
                    urlAsStr.Append(queryStr.ToString().Substring(1));
                }

                return new Uri(urlAsStr.ToString());
            }
        }

        protected string GetValue(string key) {
            if (Raw.ContainsKey(key)) {
                return Raw[key];
            } else {
                return null;
            }
        }
        protected void SetValue(string key, string value) {
            if (value == null || value.Length == 0) {
                Raw.Remove(key);
            } else {
                Raw[key] = value;
            }
            ToURLChanged();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void ToURLChanged() {
            if (this.PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs("ToURL"));
            }
        }
    }

    public class OrderProcessType {
        private readonly static Dictionary<string, OrderProcessType> _byValue = new Dictionary<string, OrderProcessType>(2);
        public readonly static OrderProcessType Detail = new OrderProcessType("detail");
        public readonly static OrderProcessType Instant = new OrderProcessType("instant");

        private readonly string _value;

        private OrderProcessType(string value) {
            _value = value;
            _byValue.Add(value, this);
        }

        public static OrderProcessType Parse(string value) {
            return value == null || value.Length == 0 ? null : _byValue[value];
        }

        public static OrderProcessType[] GetAll() {
            return new OrderProcessType[] { Detail, Instant };
        }

        public override string ToString() {
            return _value;
        }
    }

    public class Mode {
        private readonly static Dictionary<string, Mode> _byValue = new Dictionary<string, Mode>(3);
        public readonly static Mode Active = new Mode("active");
        public readonly static Mode ActiveTest = new Mode("active.test");
        public readonly static Mode Test = new Mode("test");

        private readonly string _value;

        private Mode(string value) {
            _value = value;
            _byValue.Add(value, this);
        }

        public static Mode Parse(string value) {
            return value == null || value.Length == 0 ? null : _byValue[value];
        }

        public static Mode[] GetAll() {
            return new Mode[] { Active, ActiveTest, Test };
        }

        public override string ToString() {
            return _value;
        }
    }

}
