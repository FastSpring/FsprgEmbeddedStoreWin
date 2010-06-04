﻿using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Navigation;
using FsprgEmbeddedStore.Model;
using MSHTML;

namespace FsprgEmbeddedStore
{
    public class Controller : INotifyPropertyChanged
    {
        private bool _isInitialLoad;

        private WebBrowser _webView;
        public WebBrowser WebView { 
            set {
                if (_webView != null)
                {
                    _webView.LoadCompleted -= LoadCompleted;
                }
                _webView = value;
                _webView.Navigating += Navigating;
                _webView.LoadCompleted += LoadCompleted;
            }
            get { return _webView; }
        }
        public event EventHandler DidLoadStore;
        public event DidReceiveOrderEventHandler DidReceiveOrder;

        public void LoadWithParameters(StoreParameters parameters) {
            _isInitialLoad = true;
            IsLoading = true;
            ChangeUserAgent("FSEmbeddedStore/1.0");

            /*
            StringBuilder resetCookiesJs = new StringBuilder();
            resetCookiesJs.Append("javascript:void((function(){");
            resetCookiesJs.Append("  alert('test');var a,b,c,e,f;");
            resetCookiesJs.Append("  f=0;alert(document.cookie)");
            resetCookiesJs.Append("  a=document.cookie.split('; ');");
            resetCookiesJs.Append("  for(e=0;e<a.length&&a[e];e++){");
            resetCookiesJs.Append("    f++;");
            resetCookiesJs.Append("    for(b='.'+location.host;b;b=b.replace(/^(?:%5C.|[^%5C.]+)/,'')){");
            resetCookiesJs.Append("      for(c=location.pathname;c;c=c.replace(/.$/,'')){");
            resetCookiesJs.Append("        document.cookie=(a[e]+'; domain='+b+'; path='+c+'; expires='+new Date((new Date()).getTime()-1e11).toGMTString());");
            resetCookiesJs.Append("      }");
            resetCookiesJs.Append("    }");
            resetCookiesJs.Append("  }");
            resetCookiesJs.Append("})())");
            WebView.Navigate(resetCookiesJs.ToString());
            */

            WebView.Navigate(parameters.ToURL);
        }

        /**
         * loads content from a file with an .xml suffix.
         */
        public void LoadWithContentsOfFile(string path) {
            _isInitialLoad = true;
            IsLoading = true;

            StreamReader reader = new StreamReader(path);
            string plistXml = reader.ReadToEnd();
            reader.Close();

            Uri url = new Uri("file://" + path);
            WebView.Navigate(url);
        }

        private bool _isLoading;
        public bool IsLoading { 
            get { return _isLoading; }
            internal set {
                _isLoading = value;
                if (PropertyChanged != null) {
                    PropertyChanged(this, new PropertyChangedEventArgs("IsLoading"));
                }
            }
        }
        public bool IsSecure {
            get {
                return "Https".Equals(_webView.Source.Scheme);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void IsSecureChanged()
        {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs("IsSecure"));
            }
        }

        private void Navigating(object sender, NavigatingCancelEventArgs args)
        {
            IsLoading = true;
        }

        private void LoadCompleted(object sender, NavigationEventArgs args)
        {
            IsLoading = false;
            IsSecureChanged();

            if (_isInitialLoad) {
                _isInitialLoad = false;
                DidLoadStore(this, EventArgs.Empty);
            }

            try
            {
                var aMimetype = ((HTMLDocument)_webView.Document).mimeType;
                if (aMimetype.ToLower().IndexOf("xml") > -1)
                {
                    string data = ((HTMLDocument)_webView.Document).documentElement.innerText;

                    data = data.Replace("<!DOCTYPE plist (View Source for full doctype...)>", "");
                    data = data.Replace("\r\n-", "");
                    data = data.Substring(data.IndexOf("<?xml version="));

                    Order order = Order.Parse(data);
                    DidReceiveOrder(this, new DidReceiveOrderEventArgs(order));
                }
            }
            catch (Exception e)
            {
                var errorWriter = new StreamWriter("c:/temp/error.txt");
                errorWriter.WriteLine(e.Message);
                errorWriter.Write(e.StackTrace);
                errorWriter.Close();
            }
        }

        [DllImport("urlmon.dll", CharSet = CharSet.Ansi)]
        private static extern int UrlMkSetSessionOption(int dwOption, string pBuffer, int dwBufferLength, int dwReserved);
        const int URLMON_OPTION_USERAGENT = 0x10000001;
        private void ChangeUserAgent(string Agent)
        {
            UrlMkSetSessionOption(URLMON_OPTION_USERAGENT, Agent, Agent.Length, 0);
        }
    }

    public delegate void DidReceiveOrderEventHandler(object sender, DidReceiveOrderEventArgs e);
    public class DidReceiveOrderEventArgs : EventArgs
    {
        public DidReceiveOrderEventArgs(Order order)
        {
            Order = order;
        }
        public Order Order { get; internal set; }
    }

}
