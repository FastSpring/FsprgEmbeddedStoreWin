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

        /// <summary>
        /// Information about the fulfillment.
        /// </summary>
        /// <param name="key">key type of fulfillment (e.g. license, download)</param>
        /// <returns>Specific fulfillment information (FsprgLicense, FsprgFileDownload).</returns>
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
