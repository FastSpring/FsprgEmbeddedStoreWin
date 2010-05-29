using System.Windows;
using System.Windows.Controls;
using Fsprg;

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
            confirmationView.Visibility = Visibility.Hidden;

            DataContext = new DataContext();
            DataContext.Controller = new EmbeddedStoreController();
            DataContext.Controller.WebView = webBrowser;
            DataContext.Controller.DidReceiveOrder += DidReceiveOrder;
            DataContext.Parameters = new StoreParameters();

            // some defaulting
            DataContext.Parameters.OrderProcess = "detail";
            DataContext.Parameters.StoreId = "spootnik";
            DataContext.Parameters.ProductId = "fsembeddedstore";
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

        new DataContext DataContext
        {
            get { return (DataContext)base.DataContext; }
            set { base.DataContext = value; }
        }
    }

    class DataContext
    {
        public StoreParameters Parameters { get; set; }
        public EmbeddedStoreController Controller { get; set; }
    }

}
