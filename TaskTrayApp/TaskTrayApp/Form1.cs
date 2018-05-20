using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Threading;
using System.Net.WebSockets;


namespace TaskTrayApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void quitMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;

            Application.Exit();

        }

        private void connectMenuItem_Click(object sender, EventArgs e)
        {
            Connect();
        }

        private void disconnectMenuItem_Click(object sender, EventArgs e)
        {
            Disconnect();
        }

        // TODO 接続状態を持ったモデルを作って、外部化。
        ClientWebSocket _client = null;
        private async void Connect()
        {
            if (_client != null)
            {
                // 既存の接続がある間、接続は行わない。
                return;
            }
            _client = new ClientWebSocket();
            await _client.ConnectAsync(new Uri("ws://localhost:8989/pushEndpoint"), CancellationToken.None);

            // TODO uidの設定
            await _client.SendAsync(new ArraySegment<byte> (new UTF8Encoding().GetBytes("uid:100")), WebSocketMessageType.Text, true, CancellationToken.None);

            var buf = new ArraySegment<byte>(new byte[1024]);
            while (_client.State == WebSocketState.Open)
            {
                var size = await _client.ReceiveAsync(buf, CancellationToken.None);
                var msg = new UTF8Encoding().GetString(buf.Take(size.Count).ToArray());
                this.notifyIcon1.BalloonTipTitle = "メッセージを受信しました";
                this.notifyIcon1.BalloonTipText = msg;
                this.notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
                this.notifyIcon1.ShowBalloonTip(20);

            }

            this.notifyIcon1.BalloonTipTitle = "接続が閉じられました。";
            this.notifyIcon1.BalloonTipText = "再度接続を行ってください。";
            this.notifyIcon1.BalloonTipIcon = ToolTipIcon.Error;
            this.notifyIcon1.ShowBalloonTip(20);

            _client = null;
        }

        private async void Disconnect()
        {
            await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "normal close", CancellationToken.None);
        }

    }
}
