using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Navigation;
using FsprgEmbeddedStore.Model;
using MSHTML;
using System.Windows;
using System.Text;
using Utilities.WebBrowser;

namespace FsprgEmbeddedStore
{
    public class Controller : INotifyPropertyChanged {
        private string _storeHost;
        private bool _isLoading;

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
        public event DidLoadStoreHandler DidLoadStore;
        /// <summary>
        /// Gets called on subsequent page loads.
        /// </summary>
        public event DidLoadPageEventHandler DidLoadPage;
        /// <summary>
        /// Occurs when the order has been received.
        /// </summary>
        public event DidReceiveOrderEventHandler DidReceiveOrder;
        /// <summary>
        /// Delivers a PropertyChangedEvent for <code>IsLoading</code> and <code>IsSecure</code>.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Loads the store.
        /// </summary>
        /// <param name="parameters">Parameters to load the store with.</param>
        public void LoadWithParameters(StoreParameters parameters) {
            StoreHost = null;
            IsLoading = true;

            ExpandUserAgent("FSEmbeddedStore/1.0");
            ResetCookies();
            WebBrowserHelper.ClearCache();

            WebView.Navigate(parameters.ToURL);
        }

        /// <summary>
        /// Loads content from a file with an .xml suffix and shows the order confirmation.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        public void LoadWithContentsOfFile(string path) {
            StoreHost = null;
            IsLoading = true;

            StreamReader reader = new StreamReader(path);
            string plistXml = reader.ReadToEnd();
            reader.Close();

            Uri url = new Uri("file://" + path);
            WebView.Navigate(url);
        }

        /// <summary>
        /// <code>true</code> if store is loading. Useful to show a progress indicator.
        /// </summary>
        public bool IsLoading { 
            get { return _isLoading; }
            internal set {
                _isLoading = value;
                if (PropertyChanged != null) {
                    PropertyChanged(this, new PropertyChangedEventArgs("IsLoading"));
                    PropertyChanged(this, new PropertyChangedEventArgs("IsSecure"));
                }
            }
        }
        /// <summary>
        /// <code>true</code> if communication is secure.
        /// </summary>
        public bool IsSecure {
            get {
                if (IsLoading || _webView == null || _webView.Source == null) {
                    return false;
                } else {
                    return "https".Equals(_webView.Source.Scheme, StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        public string StoreHost {
            get { return _storeHost; }
            internal set {
                _storeHost = value;
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

            AdjustResizableContent((int)Math.Round(_webView.ActualHeight));

            Uri newUri = args.Uri;
            string newStoreHost;
            if ("file".Equals(newUri.Scheme, StringComparison.CurrentCultureIgnoreCase)) {
                newStoreHost = "file";
            } else {
                newStoreHost = newUri.Host;
            }

            if (StoreHost == null) {
                StoreHost = newStoreHost;
                DidLoadStore(this, new DidLoadStoreEventArgs(newUri));
            } else {
                PageType newPageType;
                if (newStoreHost.Equals(StoreHost, StringComparison.CurrentCultureIgnoreCase)) {
                    newPageType = PageType.FS;
                } else if (newStoreHost.EndsWith("paypal.com")) {
                    newPageType = PageType.PayPal;
                } else {
                    newPageType = PageType.Unknown;
                }
                DidLoadPage(this, new DidLoadPageEventArgs(newUri, newPageType));
            }

            string aMimetype;
            try {
                aMimetype = ((HTMLDocument)_webView.Document).mimeType;
            } catch (Exception e) {
                // The document has no valid mime type, so it's likely a PayPal page. No problem.
                aMimetype = "Undefined";
            }

            if (aMimetype.ToLower().IndexOf("xml") > -1) {
                string data = ((HTMLDocument)_webView.Document).documentElement.innerText;

                data = data.Replace("<!DOCTYPE plist (View Source for full doctype...)>", "");
                data = data.Replace("\r\n-", "");
                data = data.Substring(data.IndexOf("<?xml version="));

                Order order = Order.Parse(data);
                DidReceiveOrder(this, new DidReceiveOrderEventArgs(order));
            }
        }

        [DllImport("urlmon.dll", CharSet = CharSet.Ansi)]
        private static extern int UrlMkGetSessionOption(int dwOption, StringBuilder pBuffer, int dwBufferLength, ref int pdwBufferLength, int dwReserved);
        [DllImport("urlmon.dll", CharSet = CharSet.Ansi)]
        private static extern int UrlMkSetSessionOption(int dwOption, string pBuffer, int dwBufferLength, int dwReserved);
        const int URLMON_OPTION_USERAGENT = 0x10000001;
        private void ExpandUserAgent(string newSuffix)
        {
            StringBuilder userAgent = new StringBuilder(255);
            int returnLength = 0;
            UrlMkGetSessionOption(URLMON_OPTION_USERAGENT, userAgent, userAgent.Capacity, ref returnLength, 0);

            string newUserAgent = userAgent.ToString() + " " + newSuffix;
            UrlMkSetSessionOption(URLMON_OPTION_USERAGENT, newUserAgent, newUserAgent.Length, 0);
        }

        private void ResetCookies() {
            HTMLDocument document = (HTMLDocument)WebView.Document;
            if (document == null) {
                return;
            }
            if (document.cookie == null) {
                return;
            }

            string[] cookies = document.cookie.Split(new string[] { "; " }, StringSplitOptions.None);
            foreach (string cookie in cookies) {
                string domain = document.location.host;

                while (domain.Length > 0) {
                    string path = document.location.pathname;
                    while (path.Length > 0) {
                        document.cookie = string.Format("{0}; domain={1}; path={2}; expires=Thu, 2 Aug 2001 20:47:11 UTC", cookie, domain, path);

                        int slashIdx = path.LastIndexOf('/');
                        path = slashIdx == -1 ? "" : path.Substring(0, slashIdx);
                    }

                    int dothIdx = domain.IndexOf('.');
                    domain = dothIdx == -1 ? "" : domain.Substring(dothIdx + 1);
                }
            }
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

    public delegate void DidLoadStoreHandler(object sender, DidLoadStoreEventArgs e);
    public class DidLoadStoreEventArgs : EventArgs {
        public DidLoadStoreEventArgs(Uri uri)
        {
            Uri = uri;
        }
        public Uri Uri { get; internal set; }
    }

    public enum PageType {
        FS,
        PayPal,
        Unknown
    }
    public delegate void DidLoadPageEventHandler(object sender, DidLoadPageEventArgs e);
    public class DidLoadPageEventArgs : EventArgs
    {
        public DidLoadPageEventArgs(Uri uri, PageType pageType)
        {
            Uri = uri;
            PageType = pageType;
        }
        public Uri Uri { get; internal set; }
        public PageType PageType { get; internal set; }
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
