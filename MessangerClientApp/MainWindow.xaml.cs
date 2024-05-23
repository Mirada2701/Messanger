using System.Collections.ObjectModel;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
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

namespace MessangerClientApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IPEndPoint serverPoint;
        //const string server_address = "127.0.0.1";
        //const short server_port = 4040;
        UdpClient client;
        ObservableCollection<MessageInfo> messages;
        private string Nick;
        public MainWindow()
        {
            InitializeComponent();
            NickWindow window = new NickWindow();
            window.ShowDialog();
            Nick = window.nickTb.Text;
            client = new UdpClient();
            string address = ConfigurationManager.AppSettings["ServerAddress"]!;
            short port = short.Parse(ConfigurationManager.AppSettings["ServerPort"]!);
            serverPoint = new IPEndPoint(IPAddress.Parse(address),port);
            messages = new ObservableCollection<MessageInfo>();
            this.DataContext = messages;
        }

        private void Send_Button_Click(object sender, RoutedEventArgs e)
        {            
            string message = msgTextBox.Text;
            SendMessage(message);
        }
        private void Join_Button_Click(object sender, RoutedEventArgs e)
        {
            string message = "$<join>";
            SendMessage(message);
            Listen();
        }
        private async void SendMessage(string message)
        {
            if(!string.IsNullOrEmpty(message))
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                await client.SendAsync(data, serverPoint);
            }            
        }
        private async void Listen()
        {
            while (true)
            {
                //IPEndPoint remoreIpServer = null;
                var result = await client.ReceiveAsync();
                string message = Encoding.UTF8.GetString(result.Buffer);
                messages.Add(new MessageInfo(Nick,message));
            }
        }

        private void Leave_Button_Click(object sender, RoutedEventArgs e)
        {
            string message = "$<leave>";
            SendMessage(message);
        }
    }
    public class MessageInfo
    {
        public string UserName { get; set; }
        public string Text { get; set; }
        public DateTime Time { get; set; }
        public MessageInfo(string name,string text)
        {
            UserName = name;
            this.Text = text;
            Time = DateTime.Now;
        }
        public string FullInfo => $"{UserName}\n{Text}\t{Time}";
    }
}