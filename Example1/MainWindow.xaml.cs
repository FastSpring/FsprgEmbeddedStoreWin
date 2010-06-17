using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FsprgEmbeddedStore;
using FsprgEmbeddedStore.Model;

namespace Example1 {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private StoreParameters _parameters;
        private Controller _controller;
        private Order _receivedOrder;

        public MainWindow() {
            InitializeComponent();

            // set parameters
            _parameters = new StoreParameters();
            _parameters.OrderProcessType = OrderProcessType.Detail;
            _parameters.StoreId = "your_store";
            _parameters.ProductId = "your_product";
            _parameters.Mode = Mode.Test;

            // configure controller and provide it as DataContext to
            // access IsLoading property from XAML
            _controller = new Controller();
            _controller.DidLoadStore += DidLoadStore;
            _controller.DidReceiveOrder += DidReceiveOrder;
            _controller.WebView = webBrowser;
            DataContext = _controller;

            // load store
            urlField.Text = _parameters.ToURL.ToString();
            _controller.LoadWithParameters(_parameters);
        }

        private void DidLoadStore(object sender, EventArgs args) {
            _receivedOrder = null;

            webBrowser.Visibility = Visibility.Visible;
            confirmationLabel1.Visibility = Visibility.Hidden;
            confirmationLabel2.Visibility = Visibility.Hidden;
            confirmationShowLicenseButton.Visibility = Visibility.Hidden;
        }

        private void DidReceiveOrder(object sender, DidReceiveOrderEventArgs args) {
            _receivedOrder = args.Order;

            confirmationLabel1.Content = "Hi " + _receivedOrder.CustomerFirstName;
            webBrowser.Visibility = Visibility.Hidden;
            confirmationLabel1.Visibility = Visibility.Visible;
            confirmationLabel2.Visibility = Visibility.Visible;
            confirmationShowLicenseButton.Visibility = Visibility.Visible;
        }

        private void reloadButton_Click(object sender, RoutedEventArgs e) {
            _controller.LoadWithParameters(_parameters);
        }

        private void confirmationShowLicenseButton_Click(object sender, RoutedEventArgs e) {
            string message = string.Format("Name: {0}\nEmail: {1}", _receivedOrder.CustomerLastName, _receivedOrder.CustomerEmail);
            MessageBox.Show(this, message, "Your License");
        }

        private void openInBrowserButton_MouseDown(object sender, MouseButtonEventArgs e) {
            System.Diagnostics.Process.Start(_parameters.ToURL.ToString());
        }
    }
}
