using System.Windows;
using System.Windows.Controls;
using FsprgEmbeddedStore;
using TestApp.Properties;
using System;
using System.Configuration;

namespace TestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Closed += AppClosed;

            confirmationView.Visibility = Visibility.Hidden;
            orderProcessTypeField.ItemsSource = OrderProcessType.GetAll();
            modelField.ItemsSource = Mode.GetAll();

            DataContext = new DataContext();
            DataContext.Controller = new Controller();
            DataContext.Controller.WebView = webBrowser;
            DataContext.Controller.DidReceiveOrder += DidReceiveOrder;
            DataContext.Parameters = new StoreParameters();

            foreach (SettingsProperty property in Settings.Default.Properties) {
                string value = (string)Settings.Default[property.Name];
                if (value.Length > 0) {
                    DataContext.Parameters.Raw.Add(property.Name, value);
                }
            }
        }

        private void DidReceiveOrder(object sender, DidReceiveOrderEventArgs args)
        {
            webBrowser.Visibility = Visibility.Hidden;
            confirmationView.Visibility = Visibility.Visible;

            confirmationView.Content += "Thanks "+args.Order.CustomerFirstName+" for buying a license!\n\n";
            confirmationView.Content += "Your license key is " + args.Order.FirstOrderItem.License.FirstLicenseCode;
        }

        private void reloadButton_Click(object sender, RoutedEventArgs e)
        {
            webBrowser.Visibility = Visibility.Visible;
            confirmationView.Visibility = Visibility.Hidden;
            DataContext.Controller.LoadWithParameters(DataContext.Parameters);
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (TabItem)tabControl.SelectedItem;
            if (selectedItem != null && selectedItem.Name.Equals("preview"))
            {
                reloadButton_Click(sender, null);
            }
        }

        private void AppClosed(object sender, EventArgs args) {
            Settings.Default.Reset();
            foreach (var param in DataContext.Parameters.Raw) {
                Settings.Default[param.Key] = param.Value;
            }
            Settings.Default.Save();
        }

        new DataContext DataContext
        {
            get { return (DataContext)base.DataContext; }
            set { base.DataContext = value; }
        }
    }

    internal class DataContext
    {
        public StoreParameters Parameters { get; set; }
        public Controller Controller { get; set; }
    }

}
