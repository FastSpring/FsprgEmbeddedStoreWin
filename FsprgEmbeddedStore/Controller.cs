using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Navigation;
using FsprgEmbeddedStore.Model;
using MSHTML;
using System.Windows;
using System.Windows.Media;

namespace FsprgEmbeddedStore
{
    public class Controller : INotifyPropertyChanged
    {
        private bool _isInitialLoad;

        private WebBrowser _webView;
        /// <summary>
        /// Attach <see cref="WebBrowser"/> instance to this controller.
        /// </summary>
        public WebBrowser WebView { 
            set {
                if (_webView != null)
                {
                    _webView.LoadCompleted -= LoadCompleted;
                }
                _webView = value;
                _webView.Navigating += Navigating;
                _webView.LoadCompleted += LoadCompleted;
                _webView.SizeChanged += WebBrowserSizeChanged;
            }
            get { return _webView; }
        }
        /// <summary>
        /// Occurs when the store has been loaded for the first time.
        /// </summary>
        public event EventHandler DidLoadStore;
        /// <summary>
        /// Occurs when the order has been received.
        /// </summary>
        public event DidReceiveOrderEventHandler DidReceiveOrder;

        /// <summary>
        /// Loads the store.
        /// </summary>
        /// <param name="parameters">Parameters to load the store with.</param>
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

        /// <summary>
        /// Loads content from a file with an .xml suffix and shows the order confirmation.
        /// </summary>
        /// <param name="path">Path to the file.</param>
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
        /// <summary>
        /// <code>true</code> if store is loading. Useful to show a progress indicator.
        /// </summary>
        public bool IsLoading { 
            get { return _isLoading; }
            internal set {
                _isLoading = value;
                if (PropertyChanged != null) {
                    PropertyChanged(this, new PropertyChangedEventArgs("IsLoading"));
                }
            }
        }
        /// <summary>
        /// <code>true</code> if communication is secure.
        /// </summary>
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
        
        private void WebBrowserSizeChanged(object sender, SizeChangedEventArgs args) {
            AdjustResizableContent((int)Math.Round(args.NewSize.Height));
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
                AdjustResizableContent((int)Math.Round(_webView.ActualHeight));

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

        private void AdjustResizableContent(int browserWindowHeightPx) {
            if (_webView.Document == null) {
                return;
            }

            IHTMLElement resizableContentE = ((HTMLDocument)_webView.Document).getElementById("FsprgResizableContent");
            if (resizableContentE == null) {
                return;
            }

            IHTMLElement storePageNavigationE = null;
            IHTMLElementCollection divEs = ((HTMLDocument)_webView.Document).getElementsByTagName("div");
            foreach (IHTMLElement divE in divEs) {
                if ("store-page-navigation".Equals(divE.className)) {
                    storePageNavigationE = divE;
                    break;
                }
            }
            if (storePageNavigationE == null) {
                return;
            }

            dynamic resizableContentEStyle = resizableContentE.getAttribute("currentStyle"); // see http://blog.stchur.com/2006/06/21/css-computed-style/
            string paddingTopStr = resizableContentEStyle.paddingTop;
            int paddingTopPx = int.Parse(paddingTopStr.TrimEnd(new char[] { 'p', 'x' }));
            string paddingBottomStr = resizableContentEStyle.paddingBottom;
            int paddingBottomPx = int.Parse(paddingBottomStr.TrimEnd(new char[] { 'p', 'x' }));

            int storePageNavigationHeightPx = storePageNavigationE.getAttribute("clientHeight");

            int newHeight = browserWindowHeightPx - paddingTopPx - paddingBottomPx - storePageNavigationHeightPx;
            if (newHeight < 0) {
                newHeight = 0;
            }
            resizableContentE.style.height = newHeight + "px";
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
