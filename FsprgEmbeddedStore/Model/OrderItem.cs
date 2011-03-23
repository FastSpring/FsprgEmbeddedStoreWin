using System.Collections.Generic;

namespace FsprgEmbeddedStore.Model {

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
        public decimal ItemTotal {
            get { return Raw.GetDecimal("ItemTotal", 0); }
        }
        public decimal ItemTotalUSD {
            get { return Raw.GetDecimal("ItemTotalUSD", 0); }
        }
        public Fulfillment Fulfillment {
            get { return new Fulfillment(Raw.GetDict("Fulfillment")); }
        }
        /// <summary>
        /// Shortcut for Fulfillment["license"].
        /// </summary>
        public License License {
            get { return (License)Fulfillment["license"]; }
        }
        /// <summary>
        /// Shortcut for Fulfillment["download"].
        /// </summary>
        public FileDownload FileDownload {
            get { return (FileDownload)Fulfillment["download"]; }
        }
    }

}
