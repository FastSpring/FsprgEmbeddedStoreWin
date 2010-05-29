using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fsprg.Model {

    public class PlistDict {
        private readonly Dictionary<string, object> _dict;

        public PlistDict() {
            _dict = new Dictionary<string, object>();
        }

        public PlistDict(Dictionary<string, object> dict) {
            _dict = dict;
        }

        public string GetString(string key, string defaultStr) {
            if (_dict.ContainsKey(key)) {
                return (string)_dict[key];
            } else {
                return defaultStr;
            }
        }

        public decimal GetDecimal(string key, decimal defaultDecimal) {
            if (_dict.ContainsKey(key)) {
                return (decimal)_dict[key];
            } else {
                return defaultDecimal;
            }
        }

        public bool GetBool(string key, bool defaultBool) {
            if (_dict.ContainsKey(key)) {
                return (bool)_dict[key];
            } else {
                return defaultBool;
            }
        }

        public object[] GetArray(string key) {
            if (_dict.ContainsKey(key)) {
                return (object[])_dict[key];
            } else {
                return new object[0];
            }
        }

        public PlistDict GetDict(string key) {
            if (_dict.ContainsKey(key)) {
                return (PlistDict)_dict[key];
            } else {
                return new PlistDict();
            }
        }
    }

}
