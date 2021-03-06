﻿using JohnBauerPictureViewer.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace JohnBauerPictureViewer.UserControls
{
    public sealed partial class WatchOutDashboard : UserControl
    {
        private Grid root;
        private WatchoutControlProtocol protocol;

        public WatchOutDashboard(Grid root)
        {
            this.InitializeComponent();
            this.root = root;
            root.Children.Add(this);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            root.Children.Remove(this);
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            //var protocol = new WatchoutControlProtocol(HostBox.Text, PortBox.Text, ResultTextBlock, CommandBox.Text);
            
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            protocol.SendCommand(CommandBox.Text);

            //WatchoutControlProtocol.SendCommand(HostBox.Text, PortBox.Text, ResultTextBlock, CommandBox.Text);


            //protocol = new WatchoutControlProtocol(HostBox.Text, PortBox.Text, ResultTextBlock, CommandBox.Text);
        }

        private void RunT1Button_Click(object sender, RoutedEventArgs e)
        {
            protocol.SendCommand("run T1");
        }

        private void KillT1Button_Click(object sender, RoutedEventArgs e)
        {
            protocol.SendCommand("kill T1");
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ResultTextBlock.Text = "";
        }


        private void ShowMessage(string message)
        {
            ResultTextBlock.Text += message + "\n";
        }
        

        private async void StartServer()
        {
            try
            {
                var streamSocketListener = new Windows.Networking.Sockets.StreamSocketListener();

                // The ConnectionReceived event is raised when connections are received.
                streamSocketListener.ConnectionReceived += StreamSocketListener_ConnectionReceived;

                // Start listening for incoming TCP connections on the specified port. You can specify any port that's not currently in use.
                await streamSocketListener.BindServiceNameAsync(PortBox.Text);

                ShowMessage("server is listening...");
            }
            catch (Exception ex)
            {
                Windows.Networking.Sockets.SocketErrorStatus webErrorStatus = Windows.Networking.Sockets.SocketError.GetStatus(ex.GetBaseException().HResult);
                ShowMessage(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
            }
        }

        private async void StreamSocketListener_ConnectionReceived(Windows.Networking.Sockets.StreamSocketListener sender, Windows.Networking.Sockets.StreamSocketListenerConnectionReceivedEventArgs args)
        {
            string request;
            using (var streamReader = new StreamReader(args.Socket.InputStream.AsStreamForRead()))
            {
                request = await streamReader.ReadLineAsync();
            }

            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>

            ShowMessage(string.Format("server received the request: \"{0}\"", request)));

            // Echo the request back as the response.
            using (Stream outputStream = args.Socket.OutputStream.AsStreamForWrite())
            {
                using (var streamWriter = new StreamWriter(outputStream))
                {
                    await streamWriter.WriteLineAsync(request);
                    await streamWriter.FlushAsync();
                }
            }

            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ShowMessage(string.Format("server sent back the response: \"{0}\"", request)));

            sender.Dispose();

            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ShowMessage("server closed its socket"));
        }

        private void StartServerButton_Click(object sender, RoutedEventArgs e)
        {
            StartServer();

        }

        private void Authenticate1Button_Click(object sender, RoutedEventArgs e)
        {
            protocol.SendCommand("authenticate 1");
        }

        private void CreateClassButton_Click(object sender, RoutedEventArgs e)
        {
            if (protocol == null)
            {
                protocol = new WatchoutControlProtocol(HostBox.Text, PortBox.Text, ResultTextBlock);

                CreateClassButton.Content = "Destroy class";
            }
            else
            {
                protocol = null;

                CreateClassButton.Content = "Create class";
            }
        }
    }
}
