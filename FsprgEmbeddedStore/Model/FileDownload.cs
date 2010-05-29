using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FsprgEmbeddedStore.Model {

    public class FileDownload  {
        private readonly PlistDict _raw;

        public FileDownload(PlistDict dict) {
            _raw = dict;
        }

        public PlistDict Raw {
            get { return _raw; }
        }

        public Uri FileURL {
            get { return new Uri(Raw.GetString("FileURL", "")); }
        }
    }

}
