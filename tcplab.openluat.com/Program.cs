using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;


namespace tcplab.openluat.com
{
    class Program
    {
        private static WebSocket ws = new WebSocket("ws://tcplab.openluat.com:12421");
        private static string luat_ip,luat_port;

        static void Main(string[] args)
        {
            if(args.Length == 1)
            {
                ws = new WebSocket("ws://"+ args[0].Replace("ws://",""));
            }

            ws.OnClose += (sender, e) =>
                PrintLog("websocket连接被关闭，请重新打开该工具");
            ws.OnMessage += WSRev;
            ws.Connect();
            ws.Send("{\"function\": 0}");

            Console.Clear();
            Console.WriteLine("tcplab.openluat.com连接保活工具\r\n" +
                "本工具仅用于调试使用，请勿用来恶意占用调试服务器资源\r\n");

            Console.WriteLine("使用说明：\r\n输入数据按回车可以发送数据\r\n");

            while (true)
            {
                string readText = Console.ReadLine();

                WSSend(readText);
            }
        }

        /// <summary>
        /// 打印日志
        /// </summary>
        /// <param name="s">要打印的东西</param>
        private static void PrintLog(string s)
        {
            Console.WriteLine("[" +DateTime.Now.ToString()+"]"+s);
        }

        /// <summary>
        /// 连接TCP服务器，就是用了创建一个保活链接用的
        /// </summary>
        /// <param name="luatip">ip</param>
        /// <param name="luatport">端口</param>
        private static void ConnectTCP(string luatip,string luatport)
        {
            TcpClient client = new TcpClient();
            try
            {
                client.Connect(luatip, int.Parse(luatport));
            }
            catch (Exception ex)
            {
                PrintLog("创建保活连接时发生错误：\r\n" + ex.Message);
                return;
            }
            PrintLog("成功创建保活连接");
        }

        /// <summary>
        /// 发送websocket数据
        /// </summary>
        /// <param name="s">要发送的数据</param>
        private static void WSSend(string s)
        {
            JObject staff = new JObject();
            staff.Add(new JProperty("function", 5));
            staff.Add(new JProperty("data", s));
            ws.Send(staff.ToString());
            PrintLog("发送数据：" + s);
        }

        private static bool firstConnect = true;
        /// <summary>
        /// websocket接受数据处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void WSRev(object sender, MessageEventArgs e)
        {
            //PrintLog("收到websocket消息：" + e.Data);
            try
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(e.Data);
                if((int)jo["function"] == 0)
                {
                    luat_ip = (string)jo["ip"];
                    luat_port = (string)jo["port"];
                    //Console.Clear();
                    PrintLog("已获取服务器信息：");
                    PrintLog("服务器IP：" + luat_ip);
                    PrintLog("服务器端口：" + luat_port);
                    PrintLog("请在程序里使用上述的服务器信息");
                    ConnectTCP(luat_ip, luat_port);
                }
                else if ((int)jo["function"] == 1)
                {
                    if(firstConnect)
                    {
                        firstConnect = false;
                        return;
                    }
                    string address_str = (string)jo["address_str"];
                    PrintLog("已被客户端连接：" + address_str);
                }
                else if ((int)jo["function"] == 2)
                {
                    string address_str = (string)jo["address_str"];
                    PrintLog("客户端连接断开：" + address_str);
                }
                else if ((int)jo["function"] == 3)
                {
                    string address_str = (string)jo["address_str"];
                    string data = (string)jo["data"];
                    PrintLog("收到客户端" + address_str + "消息：" + data);
                    byte[] bt = System.Text.Encoding.ASCII.GetBytes(data);
                    PrintLog("HEX：" + BitConverter.ToString(bt).Replace("-"," "));
                }
                else if ((int)jo["function"] == 7)
                {
                    PrintLog("长时间没有tcp连接导致断开\r\n咦奇怪了，不该啊！");
                }
                else
                {
                    PrintLog("发生错误，解析失败" + e.Data);
                }
            }
            catch
            {
                PrintLog("发生错误，解析失败.");
            }
        }










        /// <summary>
        /// 把 16 进制字符串转换成字节数组
        /// </summary>
        private static byte[] hexStringToByte(String hex)
        {
            hex = hex.Replace(" ", "").ToUpper();
            int len = (hex.Length / 2);
            byte[] result = new byte[len];
            char[] achar = hex.ToCharArray();
            for (int i = 0; i < len; i++)
            {
                int pos = i * 2;
                result[i] = (byte)(toByte(achar[pos]) << 4 | toByte(achar[pos + 1]));
            }
            return result;
        }

        private static int toByte(char c)
        {
            byte b = (byte)"0123456789ABCDEF".IndexOf(c);
            return b;
        }

    }
}
