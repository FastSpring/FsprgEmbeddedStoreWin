using System.Windows;
using System.Windows.Controls;
using FsprgEmbeddedStore;
using FsprgEmbeddedStore.Model;
using TestApp.Properties;
using System;
using System.Configuration;
using System.IO;
using Microsoft.Win32;

namespace TestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Order _receivedOrder;

        public MainWindow()
        {
            InitializeComponent();

            orderProcessTypeField.ItemsSource = OrderProcessType.GetAll();
            modelField.ItemsSource = Mode.GetAll();

            DataContext = new DataContext();
            DataContext.Controller = new Controller();
            DataContext.Controller.WebView = webBrowser;
            DataContext.Controller.DidLoadStore += DidLoadStore;
            DataContext.Controller.DidReceiveOrder += DidReceiveOrder;
            DataContext.Parameters = new StoreParameters();

            foreach (SettingsProperty property in Settings.Default.Properties) {
                string value = (string)Settings.Default[property.Name];
                if (value.Length > 0) {
                    DataContext.Parameters.Raw.Add(property.Name, value);
                }
            }
        }

        private void DidLoadStore(object sender, EventArgs args) {
            _receivedOrder = null;
            saveAsButton.Visibility = Visibility.Hidden;
        }

        private void DidReceiveOrder(object sender, DidReceiveOrderEventArgs args) {
            _receivedOrder = args.Order;
            saveAsButton.Visibility = Visibility.Visible;
        }

        private void reloadButton_Click(object sender, RoutedEventArgs e) {
            DataContext.Controller.LoadWithParameters(DataContext.Parameters);
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selectedItem = (TabItem)tabControl.SelectedItem;
            if (selectedItem != null && selectedItem.Name.Equals("preview"))
            {
                reloadButton_Click(sender, null);
            }
        }

        private void saveAsButton_Click(object sender, RoutedEventArgs e) {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = Environment.SpecialFolder.Desktop.ToString();
            saveFileDialog1.Filter = "XML files (*.xml)|*.xml";
            saveFileDialog1.FilterIndex = 1;

            if (saveFileDialog1.ShowDialog() == true) {
                StreamWriter writer = new StreamWriter(saveFileDialog1.FileName);
                writer.Write(_receivedOrder.Raw.OriginalDoc.InnerXml);
                writer.Close();
            }
        }

        private void Window_Closed(object sender, EventArgs e) {
            Settings.Default.Reset();
            foreach (var param in DataContext.Parameters.Raw) {
                Settings.Default[param.Key] = param.Value;
            }
            Settings.Default.Save();
        }

        new DataContext DataContext {
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
