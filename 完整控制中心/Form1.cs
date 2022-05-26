using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;//导入线程类
using System.IO.Ports;//导入端口类

namespace 完整控制中心
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            
            InitializeComponent();
        }

    //控制中心 窗口初始化函数
    private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;//允许在线程中改变控件
        }

        //PLC串口设置-->点击打开串口按钮
        private void button_open_sPort1_Click(object sender, EventArgs e)
        {
            String ckh, btl;
            ckh = comboBox_ckh.Text;
            btl = comboBox_btl.Text;
            if (ckh.Equals("") && btl.Equals(""))
            {
                MessageBox.Show("PLC串口号，波特率不能为空");
                return;
            }

            if (radioButton_open_sPort1.Checked == false)
            {
                //如果串口没有打开
                sPort1.PortName = ckh;//串口号设置
                sPort1.BaudRate = int.Parse(btl);//波特率设置
                try
                {
                    sPort1.Open();
                    radioButton_open_sPort1.Checked = true;
                    button_open_sPort1.Text = "关闭串口";
                    sPort1.DataReceived += new SerialDataReceivedEventHandler(ReceiveSport1);
                }
                catch (Exception ex)
                {
                    
                    MessageBox.Show(ex.Message);
                    throw;
                }
            }
            else
            {
                //如果串口已经打开，那么关闭
                sPort1.Close();
                radioButton_open_sPort1.Checked = false;
                button_open_sPort1.Text = "打开串口";
            }
            
        }

        private void ReceiveSport1(object sender, SerialDataReceivedEventArgs e)
        {
            //开辟接收缓冲区
            byte[] data = new byte[sPort1.BytesToRead];
            //从串口读取数据
            sPort1.Read(data, 0, data.Length);
            //实现数据的解码与显示
            foreach (byte b in data)
            {
                if (checkBox1.Checked == false)
                {
                    Char ctmp = (char)b;
                    ShowSport1(ctmp + "");
                }
                else
                {
                    String s = b.ToString("X2");
                    ShowSport1(s);
                }
            }
        }

        void ShowSport1(string msg)
        {
            textBox_res.AppendText(msg);
        }

        // 服务器端-->启动服务
        private void btnListen_Click(object sender, EventArgs e)
        {
            //ip地址
            IPAddress ip = IPAddress.Parse(txtIP.Text);
            // IPAddress ip = IPAddress.Any;
            //端口,包括ip地址和端口号码
            IPEndPoint point = new IPEndPoint(ip, int.Parse(txtPort.Text));
            //创建监听用的Socket
            /*
             * AddressFamily.InterNetWork：使用 IP4地址。
SocketType.Stream：支持可靠、双向、基于连接的字节流，而不重复数据。此类型的 Socket 与单个对方主机进行通信，并且在通信开始之前需要远程主机连接。Stream 使用传输控制协议 (Tcp) ProtocolType 和 InterNetworkAddressFamily。
ProtocolType.Tcp：使用传输控制协议。
             */
            //使用IPv4地址，流式socket方式，tcp协议传递数据
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //创建好socket后，必须告诉socket绑定的IP地址和端口号。
            //让socket监听point
            try
            {
                //socket监听哪个端口
                socket.Bind(point);
                //同一个时间点过来10个客户端，排队
                socket.Listen(10);
                ShowMsg("开始监听客户端传来的信息!");
                Thread thread = new Thread(AcceptInfo);
                thread.IsBackground = true;
                thread.Start(socket);
            }
            catch (Exception ex)
            {
                ShowMsg(ex.Message);
            }
        }

        //记录通信用的Socket
        Dictionary<string, Socket> dic = new Dictionary<string, Socket>();
        void AcceptInfo(object o)
        {
            Socket socket = o as Socket;
            while (true)
            {
                //通信用socket
                try
                {
                    //创建通信用的Socket
                    Socket tSocket = socket.Accept();
                    string point = tSocket.RemoteEndPoint.ToString();
                    //IPEndPoint endPoint = (IPEndPoint)client.RemoteEndPoint;
                    //string me = Dns.GetHostName();//得到本机名称
                    //MessageBox.Show(me);
                    ShowMsg("客户端连接成功");
                    cboIpPort.Items.Add(point);
                    dic.Add(point, tSocket);
                    //接收消息
                    Thread th = new Thread(ReceiveMsg);
                    th.IsBackground = true;
                    th.Start(tSocket);
                }
                catch (Exception ex)
                {
                    ShowMsg(ex.Message);
                    break;
                }
            }
        }

        byte[] rdata=new byte[1024 * 1024];
        //接收消息
        void ReceiveMsg(object o)
        {
            Socket client = o as Socket;
            while (true)
            {
                //接收客户端发送过来的数据
                try
                {
                    //定义byte数组存放从客户端接收过来的数据
                    //将接收过来的数据放到buffer中，并返回实际接受数据的长度
                    int n = client.Receive(rdata);
                    for (int i = 0; i < n; i++) Console.Write("{0}\n", rdata[i]);
                    //接收到16进制数
                    //接收处理
                    ResRun();
                    //                  sPort1.Write(buffer, 0, n);
                    //将字节转换成字符串
                    //                    string words = Encoding.UTF8.GetString(buffer, 0, n);
                    //                    ShowMsg(client.RemoteEndPoint.ToString() + ":" + words);
                }
                catch (Exception ex)
                {
                    ShowMsg(ex.Message);
                    break;
                }
            }
        }

        int chesu = 0;

        void ResRun()
        {
            //包头判断
            if (rdata[0]==0xcc && rdata[1]==0xee)
            {
                //车速或者轨道灯
                if (rdata[3] == 0xff)
                {
                    //车速
                    if (rdata[6] >= 0 && rdata[6] <= 2)
                    {
                        chesu = rdata[6] * 100;
                        label_chesu.Text = chesu + " km/h";
                    }
                }
                //信号机
                else
                {
                    PictureBox deng=null;
                    switch (rdata[3])
                    {
                        default:break;
                        case 1: deng = deng1;break;
                        case 2: deng = deng2; break;
                        case 3: deng = deng3; break;
                        case 4: deng = deng4; break;
                        case 5: deng = deng5; break;
                        case 6: deng = deng6; break;
                        case 7: deng = deng7; break;
                        case 8: deng = deng8; break;
                    }
                    string filePath=null;
                    switch(rdata[5])
                    {
                        default: break;
                        case 1:filePath = Application.StartupPath + "\\res\\" + "红灯" + ".png";break;
                        case 2:filePath = Application.StartupPath + "\\res\\" + "黄灯" + ".png"; break;
                        case 3:filePath = Application.StartupPath + "\\res\\" + "绿灯" + ".png"; break;
                    }
                    if(deng!=null&&filePath!=null)
                    {
                        deng.Load(filePath);
                    }
                }
            }
            else if(rdata[0] == 0xee && rdata[1] == 0xcc)
            {
                //车位置
                if (rdata[6] >= 1 && rdata[6] <= 8)
                {
                    int xzb = (rdata[6] - 1) * 100 + 73;
                    pictureBox_car.Location = new Point(xzb, 297);
                }
            }
        }

        void ShowMsg(string msg)
        {
            txtLog.AppendText(msg + "\r\n");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            groupBox_sj.Visible = true;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            groupBox_kz.Visible = true;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            groupBox_gprs.Visible = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox_res.Text = "";
        }

        private void button9_Click(object sender, EventArgs e)
        {

        }
    }
}
