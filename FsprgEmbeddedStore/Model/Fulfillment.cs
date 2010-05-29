using System.Collections.Generic;

namespace FsprgEmbeddedStore.Model {

    public class Fulfillment {
        private readonly PlistDict _raw;

        public Fulfillment(PlistDict dict) {
            _raw = dict;
        }

        public PlistDict Raw {
            get { return _raw; }
        }

        public object this[string key] {
            get {
                PlistDict item = Raw.GetDict(key);

                string type = item.GetString("FulfillmentType", "");
                if (type.Equals("License")) {
                    return new License(item);
                }
                if (type.Equals("File")) {
                    return new FileDownload(item);
                }

                return item;
            }
        }
    }

}
