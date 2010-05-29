using System;
using System.Collections.Generic;

namespace FsprgEmbeddedStore.Model {

    public class License {
        private readonly PlistDict _raw;

        public License(PlistDict dict) {
            _raw = dict;
        }

        public PlistDict Raw {
            get { return _raw; }
        }

        public string LicenseName {
            get { return Raw.GetString("LicenseName", ""); }
        }
        public string LicenseEmail {
            get { return Raw.GetString("LicenseEmail", ""); }
        }
        public string LicenseCompany {
            get { return Raw.GetString("LicenseCompany", ""); }
        }
        public string FirstLicenseCode {
            get {
                object[] licenseCodes = Raw.GetArray("LicenseCodes");
                return (string)licenseCodes[0];
            }
        }
        public string[] LicenseCodes {
            get {
                object[] objarray = Raw.GetArray("LicenseCodes");
                string[] licenseCodes = new string[objarray.Length];
                objarray.CopyTo(licenseCodes, 0);

                return licenseCodes;
            }
        }
        public PlistDict LicensePropertyList {
            get { return Raw.GetDict("LicensePropertyList"); }
        }
        public Uri LicenseURL {
            get { return new Uri(Raw.GetString("LicenseURL", "")); }
        }
    }

}
