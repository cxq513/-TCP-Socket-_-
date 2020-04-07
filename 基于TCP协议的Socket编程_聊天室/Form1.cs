using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 基于TCP协议的Socket编程_聊天室
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                //当点击开始监听的时候，在服务器端口创建一个负责监听的IP地址跟端口号的Socket
                Socket socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ip = IPAddress.Any;
                //创建端口号对象
                IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(txtPort.Text));
                //监听
                socketWatch.Bind(point);
                ShowMsg("监听成功");
                socketWatch.Listen(10);

                Thread th = new Thread(Listen);
                th.IsBackground = true;
                th.Start(socketWatch);
            }
            catch
            { }

        }

        /// <summary>
        /// 等待客户端的连接，并且创建与之通信的Socket
        /// </summary>
        /// 
        Socket socketSend;
        void Listen(object o)
        {
            Socket socketWatch = o as Socket;
            //等待客户端连接 并且创建一个负责通讯的Socket
            while (true)
            {
                try
                {
                    //负责跟客户端通信的Socket
                    socketSend = socketWatch.Accept();
                    //将远程连接的客户端的IP地址和Socket存入集合中
                    dicScoket.Add(socketSend.RemoteEndPoint.ToString(), socketSend);
                    //将远程连接的客户端的IP地址和端口号存入下拉框中
                    cboUsers.Items.Add(socketSend.RemoteEndPoint.ToString());
                    ShowMsg(socketSend.RemoteEndPoint.ToString() + ":" + "连接成功");
                    //开启一个新线程，不停地接受客户端发来的消息
                    Thread th = new Thread(Receive);
                    th.IsBackground = true;
                    th.Start(socketSend);
                }
                catch
                { }
            }
        }

        //将远程连接的客户端的IP地址和Socket存入集合中
        Dictionary<string, Socket> dicScoket = new Dictionary<string, Socket>();

        /// <summary>
        /// 服务器端不停地接受客户端发来的消息
        /// </summary>
        /// <param name="o"></param>
        void Receive(object o)
        {
            Socket socketSend = o as Socket;
            while (true)
            {
                try
                {
                    //客户端连接成功后，服务器应该接受客户端发来的消息
                    byte[] buffer = new byte[1024 * 1024 * 2];
                    //实际接收到的有效字符
                    int r = socketSend.Receive(buffer);
                    if (r == 0)
                    {
                        break;
                    }
                    string str = Encoding.UTF8.GetString(buffer, 0, r);
                    ShowMsg(socketSend.RemoteEndPoint + ":" + str);
                }
                catch
                { }
            }
        }
        void ShowMsg(string str)
        {
            txtLog.AppendText(str + "\r\n");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        /// <summary>
        /// 服务器给客户端发送消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Click(object sender, EventArgs e)
        {
            string str = txtMsg.Text;
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(str);
            //获得用户在下拉框中选择的IP地址
            string ip = cboUsers.SelectedItem.ToString();
            dicScoket[ip].Send(buffer);
            //socketSend.Send(buffer);
        }

        /// <summary>
        /// txtMsg热键：回车键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtMsg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
            {
                btnSend_Click(null, null);
                txtMsg.Clear();
                txtMsg.Focus();
            }
        }
    }
}

