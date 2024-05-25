using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
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
        TcpClient client;
        ObservableCollection<MessageInfo> messages;
        private string Nick;
        NetworkStream ns = null;
        StreamReader sr = null;
        StreamWriter sw = null;
        public MainWindow()
        {
            InitializeComponent();
            NickWindow window = new NickWindow();
            window.ShowDialog();
            Nick = window.nickTb.Text;
            client = new TcpClient();
            string address = ConfigurationManager.AppSettings["ServerAddress"]!;
            short port = short.Parse(ConfigurationManager.AppSettings["ServerPort"]!);
            serverPoint = new IPEndPoint(IPAddress.Parse(address),port);
            messages = new ObservableCollection<MessageInfo>();
            this.DataContext = messages;
        }

        private void Send_Button_Click(object sender, RoutedEventArgs e)
        {            
            string message = msgTextBox.Text;
            if (!string.IsNullOrEmpty(message))
            {
                sw.WriteLine(message);
                sw.Flush();
            }
        }
        private void Connect_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                client.Connect(serverPoint);
                ns = client.GetStream();

                sr = new StreamReader(ns);
                sw = new StreamWriter(ns);
                Listen();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }            
        }
        private void Disconnect_Button_Click(object sender, RoutedEventArgs e)
        {
            ns.Close();
            client.Close();
        }
        private async void Listen()
        {
            while (true)
            {
                string? message = await sr.ReadLineAsync();
                messages.Add(new MessageInfo(Nick,message));
            }
        }        
    }
    public class MessageInfo
    {
        public string UserName { get; set; }
        public string Text { get; set; }
        public DateTime Time { get; set; }
        public MessageInfo(string name,string? text)
        {
            UserName = name;
            this.Text = text??"";
            Time = DateTime.Now;
        }
        public string FullInfo => $"{UserName}\n{Text}\t{Time}";
    }
}