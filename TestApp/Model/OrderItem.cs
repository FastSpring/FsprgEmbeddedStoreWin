using System.Collections.Generic;

namespace Fsprg.Model {

    public class OrderItem {
        private readonly PlistDict _raw;

        public OrderItem(PlistDict dict) {
            _raw = dict;
        }

        public PlistDict Raw {
            get { return _raw; }
        }

        public string ProductName {
            get { return Raw.GetString("ProductName", ""); }
        }
        public string ProductDisplay {
            get { return Raw.GetString("ProductDisplay", ""); }
        }
        public decimal Quantity {
            get { return Raw.GetDecimal("Quantity", 0); }
        }
        public Fulfillment Fulfillment {
            get { return new Fulfillment(Raw.GetDict("Fulfillment")); }
        }
        public License License {
            get { return (License)Fulfillment["license"]; }
        }
        public FileDownload FileDownload {
            get { return (FileDownload)Fulfillment["download"]; }
        }
    }

}
