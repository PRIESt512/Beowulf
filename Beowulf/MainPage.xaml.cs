using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using RabbitMQ.Client;
using System.Text;
using Windows.Storage;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.Networking;
using System.Threading.Tasks;
using System.Diagnostics;

// Документацию по шаблону элемента "Пустая страница" см. по адресу http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Beowulf
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        StreamSocketListener tcp;
        public MainPage()
        {
            this.InitializeComponent();

        }

        private async Task<StreamSocketListener> StartTcpServiceAsync()
        {
            var tcpService = new StreamSocketListener();

            tcpService.ConnectionReceived += TcpService_ConnectionReceived;
            await tcpService.BindServiceNameAsync("50000");
            

            return tcpService;
        }

        private async void StartTCP_Click(object sender, RoutedEventArgs e)
        {
            tcp = await StartTcpServiceAsync();
        }

        private async void TcpService_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            using (var client = args.Socket)
            using (var dr = new DataReader(client.InputStream))
            using (var dw = new DataWriter(client.OutputStream))
            {
                await dr.LoadAsync(sizeof(Int32));
                var messageLenght = dr.ReadUInt32();

                await dr.LoadAsync(messageLenght);
                var bytes = new Byte[messageLenght];
                dr.ReadBytes(bytes);

                var request = Encoding.UTF8.GetString(bytes);
                var response = "Успешно получено";

                var body = Encoding.UTF8.GetBytes(response);
                dw.WriteUInt32((UInt32)body.Length);
                dw.WriteBytes(body);
                await dw.StoreAsync();
            }
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            //var openPicker = new Windows.Storage.Pickers.FolderPicker();
            //openPicker.FileTypeFilter.Add("*");
            //var folder = await openPicker.PickSingleFolderAsync();

            //var list = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(folder);


            try
            {
                var file = await StorageFile.GetFileFromPathAsync(@"\\Desktop-7rpb63n\e\distribution.txt");
                var path = file.Path;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
           

            //var factory = new ConnectionFactory() { HostName = "localhost" };
            //using (var connection = factory.CreateConnection())
            //using (var channel = connection.CreateModel())
            //{
            //    channel.QueueDeclare(queue: "hello",
            //                     durable: false,
            //                     exclusive: false,
            //                     autoDelete: false,
            //                     arguments: null);
            //    var message = Message.Text;
            //    var body = Encoding.UTF8.GetBytes(message);

            //    channel.BasicPublish(exchange: "",
            //                         routingKey: "hello",
            //                         basicProperties: null,
            //                         body: body);

            //}
        }

        private async void SendSocket_Click(object sender, RoutedEventArgs e)
        {
            using (var socket = new StreamSocket())
            using (var dw = new DataWriter(socket.OutputStream))
            using (var dr = new DataReader(socket.InputStream))
            {
                var end = new EndpointPair(
                    localHostName: new HostName("localhost"), localServiceName: String.Empty,
                    remoteHostName: new HostName("localhost"), remoteServiceName: "50000");

                //await socket.ConnectAsync(new HostName("localhost"), "8080");
                await socket.ConnectAsync(end);

                var message = Message.Text;
                var body = Encoding.UTF8.GetBytes(message);

                dw.WriteUInt32((UInt32)body.Length);
                dw.WriteBytes(body);
                await dw.StoreAsync();

                await dr.LoadAsync(sizeof(Int32));
                var response = dr.ReadString(100);
            }
        }


    }
}
