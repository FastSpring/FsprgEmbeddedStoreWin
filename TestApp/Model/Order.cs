using System.Collections.Generic;

namespace Fsprg.Model
{
    public class Order {
        private readonly PlistDict _raw;

        private Order(PlistDict dict) {
            _raw = dict;
        }

        public static Order Parse(string plistXml) {
            return new Order(Plist.Parse(plistXml).Dict);
        }

        public PlistDict Raw {
            get { return _raw; }
        }

        public bool OrderIsTest {
            get { return Raw.GetBool("OrderIsTest", false); }
        }
        public string OrderReference {
            get { return Raw.GetString("OrderReference", ""); }
        }
        public string OrderLanguage {
            get { return Raw.GetString("OrderLanguage", ""); }
        }
        public string OrderCurrency {
            get { return Raw.GetString("OrderCurrency", ""); }
        }
        public decimal OrderTotal {
            get { return Raw.GetDecimal("OrderTotal", 0); }
        }
        public string CustomerFirstName {
            get { return Raw.GetString("CustomerFirstName", ""); }
        }
        public string CustomerLastName {
            get { return Raw.GetString("CustomerLastName", ""); }
        }
        public string CustomerCompany {
            get { return Raw.GetString("CustomerCompany", ""); }
        }
        public string CustomerEmail {
            get { return Raw.GetString("CustomerEmail", ""); }
        }

        public OrderItem FirstOrderItem {
            get {
                var item = Raw.GetArray("OrderItems")[0];
                return new OrderItem((PlistDict)item);
            }
        }
        public OrderItem[] OrderItems {
            get {
                object[] items = Raw.GetArray("OrderItems");
                OrderItem[] orderItems = new OrderItem[items.Length];
                for (int i = 0; i < orderItems.Length; i++) {
                    orderItems[i] = new OrderItem((PlistDict)items[i]);
                }
                return orderItems;
            }
        }
    }

}
