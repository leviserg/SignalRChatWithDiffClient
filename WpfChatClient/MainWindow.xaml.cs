using Common.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace WpfChatClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string messageHandle = nameof(IChatClient.SendClientMessageToChat);
        string addMessageKey = nameof(IChatServer.AddMessageToChat);

        string subscribeKey = nameof(IChatServer.Subscribe);
        string unsubscribeKey = nameof(IChatServer.Unsubscribe);


        string userName = String.Empty;

        HubConnection hubConnection;

        private bool subscribed;

        public MainWindow()
        {
            InitializeComponent();
            string input = Microsoft.VisualBasic.Interaction.InputBox("WpfChatClient","Enter your name", "Anonymous...", -1, -1);
            userNameTxtBox.Text = input;
            userName = input;

            if (string.IsNullOrEmpty(this.userName))
            {
                this.userName = userNameTxtBox.Text;
            }

            hubConnection = new HubConnectionBuilder().
                WithUrl("https://localhost:7065/chat",
                (HttpConnectionOptions options) => options.Headers.Add("username", userName))
                    .WithAutomaticReconnect()
                    //.AddMessagePackProtocol() // added for using MessagePack protocol + using Microsoft.Extensions.DependencyInjection;
                    .Build();

            // install NuGet package Microsoft AspNetCore SignalR Protocols MessagePack for using MessagePack protocol
            // profit - compact message (in comparison with uncompressed json - default) + quicker serialize/deserialize process - limits on data types + item counts



            hubConnection.On<ChatMessage>(messageHandle, message =>
                 AppendTextToTextBox($"{message.CreatedAt.ToString("HH:mm:ss")} : {message.Caller}", message.Text, Brushes.Black));

            // delegates

            hubConnection.Closed += error =>
            {
                MessageBox.Show($"Connection closed. {error?.Message}");
                return Task.CompletedTask;
            };

            hubConnection.Reconnected += id =>
            {
                MessageBox.Show($"Connection reconnected with id: {id}");
                return Task.CompletedTask;
            };

            hubConnection.Reconnecting += error =>
            {
                MessageBox.Show($"Connection reconnecting. {error?.Message}");
                return Task.CompletedTask;
            };
        }

        private async void connectBtn_Click(object sender, RoutedEventArgs e)
        {

            if (hubConnection.State == HubConnectionState.Disconnected)
            {
                try
                {
                    await hubConnection.StartAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else if (hubConnection.State == HubConnectionState.Connected)
            {
                await hubConnection.StopAsync();
            }

            if (hubConnection.State == HubConnectionState.Connected)
            {
                connectionStatus.Content = "Connected";
                connectionStatus.Foreground = Brushes.Green;
            }
            else
            {
                connectionStatus.Content = "Disconnected";
                connectionStatus.Foreground = Brushes.Red;
            }

        }

        private async void sendMessageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (hubConnection.State == HubConnectionState.Connected)
            {

                string message = messageTxtBox.Text;

                try
                {
                    //var mymessage = await hubConnection.InvokeAsync<ChatMessage>(addMessageKey, message);
                    await hubConnection.SendAsync(addMessageKey, message);
                    AppendTextToTextBox($"{DateTime.Now.ToString("HH:mm:ss")} :  Me", message, Brushes.Green);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    messageTxtBox.Clear();
                }
            }
        }

        private async void subscribeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (subscribed)
            {
                try
                {
                    await hubConnection.InvokeAsync(unsubscribeKey);
                    subscribed = false;
                    subscribeBtn.Content = "Subscribe";
                }
                catch (Exception ex)
                {
                    ShowError(ex);
                }
            }
            else
            {
                try
                {
                    await hubConnection.InvokeAsync(subscribeKey);
                    subscribed = true;
                    subscribeBtn.Content = "Unsubscribe";
                }
                catch (Exception ex)
                {
                    ShowError(ex);
                }
            }
        }

        public void AppendTextToTextBox(string sender, string text, Brush brush)
        {
            //BrushConverter bc = new BrushConverter();
            TextRange tr = new TextRange(chatTextBox.Document.ContentEnd, chatTextBox.Document.ContentEnd);
            tr.Text = string.Format("{0} : {1}{2}", sender, text, Environment.NewLine);
            try
            {
                tr.ApplyPropertyValue(TextElement.ForegroundProperty,
                    brush);
                    //bc.ConvertFromString(color));
            }
            catch (FormatException) { }
            finally
            {
                chatTextBox.ScrollToEnd();
            }
        }

        private void ShowError(Exception ex)
        {
            MessageBox.Show(ex?.Message ?? "Error");
        }

        private async void Shutdown(object sender, EventArgs e)
        {
            await DisposeAsync(hubConnection);
        }
        async ValueTask DisposeAsync(HubConnection hubConnection)
        {
            if (this.hubConnection != null)
            {
                await this.hubConnection.DisposeAsync();
            }
        }

    }


}
