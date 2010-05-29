using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Navigation;
using Fsprg.Model;
using MSHTML;

namespace Fsprg
{
    class EmbeddedStoreController : INotifyPropertyChanged
    {

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
            IsLoading = true;
            ChangeUserAgent("FSEmbeddedStore/1.0");
            WebView.Navigate(parameters.ToURL);
        }

        /**
        public void LoadWithContentsOfFile(string path)
        {
        TODO
        }
        */

        private bool _isLoading;
        public bool IsLoading { 
            get { return _isLoading; }
            internal set {
                _isLoading = value;
                PropertyChanged(this, new PropertyChangedEventArgs("IsLoading"));
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
            if (this.PropertyChanged != null)
            {
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

            try
            {
                var aMimetype = ((HTMLDocument)_webView.Document).mimeType;
                if (aMimetype.ToLower().IndexOf("xml") > -1)
                {
                    string data = ((HTMLDocument)_webView.Document).documentElement.innerText;

                    var beforeWriter = new StreamWriter("c:/temp/before.xml");
                    beforeWriter.Write(data);
                    beforeWriter.Close();

                    data = data.Replace("<!DOCTYPE plist (View Source for full doctype...)>", "");
                    data = data.Replace("\r\n-", "");
                    data = data.Substring(data.IndexOf("<?xml version="));

                    var afterWriter = new StreamWriter("c:/temp/after.xml");
                    afterWriter.Write(data);
                    afterWriter.Close();

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
