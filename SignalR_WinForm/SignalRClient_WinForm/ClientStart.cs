using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SignalRClient_WinForm
{
    public partial class ClientStart : Form
    {
        private String UserName { get; set; }
        private IHubProxy HubProxy { get; set; }
        const string ServerURI = "http://localhost:8080/signalr";
        private HubConnection Connection { get; set; }
        public ClientStart()
        {
            InitializeComponent();
        }

        private void bt_Connect_Click(object sender, EventArgs e)
        {
            UserName = tb_Name.Text;
            if (!String.IsNullOrEmpty(UserName))
            {
                lb_Status.Visible = true;
                lb_Status.Text = "Connecting to server...";
                ConnectAsync();
            }
        }

        private void bt_Send_Click(object sender, EventArgs e)
        {
            HubProxy.Invoke("Send", UserName, tb_Message.Text);
            tb_Message.Text = String.Empty;
            tb_Message.Focus();
        }

        private async void ConnectAsync()
        {
            Connection = new HubConnection(ServerURI);
            Connection.Closed += Connection_Closed;
            HubProxy = Connection.CreateHubProxy("MyChatHub");
            //Handle incoming event from server: use Invoke to write to console from SignalR's thread
            HubProxy.On<string, string>("AddMessage", (name, message) =>
                this.Invoke((Action)(() =>
                    richTextBox1.AppendText(String.Format("{0}: {1}" + Environment.NewLine, name, message))
                ))
            );
            try
            {
                await Connection.Start();
            }
            catch (HttpRequestException)
            {
                lb_Status.Text = "Unable to connect to server: Start server before connecting clients.";
                //No connection: Don't enable Send button or show chat UI
                return;
            }

            //Activate UI
            bt_Send.Enabled = true;
            tb_Message.Enabled = true;
            tb_Message.Focus();
            tb_Name.Enabled = false;
            bt_Connect.Enabled = false;
            richTextBox1.AppendText("Connected to server at " + ServerURI + Environment.NewLine);
        }

        private void Connection_Closed()
        {
            //Deactivate chat UI; show login UI. 
            this.Invoke((Action)(() => bt_Send.Enabled = false));
            this.Invoke((Action)(() => lb_Status.Text = "You have been disconnected."));
        }

    }
}
