using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;


namespace Sxer.WWW.MySocket
{
    //数据处理方式：
    //一：对字节数据进行管理，避免粘包拆包问题(固定字节长度，包头包体信息，结束符号判定)
    //二：对字符串数据进行管理，丢弃粘包拆包数据(数据丢失)



    //服务器监听连接线程：等待客户端连接

    //每连接一个客户端，开启一个监听消息线程



    //自定义缓存区，套接字的接收数据加入该缓存区；
    //对自定义的缓存区进行




    public static class SocketOperationServer
    {
        //开启服务端

        //服务端socket
        static Socket _MyServer;

        //客户端
        static List<Socket> socketClients;

        static List<ClientHandle> clients;

        //一个线程监听连接
        //连接成功新线程接收数据

        static Thread _th_KeepListen;//等待连接

        //是否监听
        static bool isListen = false;
        static bool isListenThreadOpen = false;

        public static string socketTag = string.Empty;

        public static void OpenServer_Tcp(string ipaddress, int port,int maxListenCount = 1)
        {
            try
            {
                IPAddress pAddress = IPAddress.Parse(ipaddress);
                IPEndPoint pEndPoint = new IPEndPoint(pAddress, port);
                _MyServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _MyServer.Bind(pEndPoint);//绑定ip和端口

                //设置同时接受到的最大连接数(并不是只能监听这么多，只是避免高并发)
                //当服务端接收到一个socket时，会自动-1，则又可以多一个socket加入等待连接
                _MyServer.Listen(maxListenCount);


                Debug.Log(_MyServer.LocalEndPoint);//打印当前服务ip和端口

                //数据部分
                //
                socketClients = new List<Socket>();
                clients = new List<ClientHandle>();

                //行为部分
                //创建新线程执行监听，否则进程会阻塞
                StartListenConnectThread();

    
                //开启线程检测当前状态
                // CheckNowSocketState();
            }
            catch (System.Exception ex)
            {
                Debug.LogError("启动socket服务端失败" + ex.Message);
            }
        }

        //static void CheckNowSocketState()
        //{
        //    nowStateThread = new Thread(() =>
        //    {
        //        while (true)
        //        {
        //            Debug.Log("服务器:"+SK_server.Connected);
        //            if (socketSend != null)
        //            {
        //                socketSend.
        //            }
        //                Debug.Log("连接socket:" + socketSend.Connected);
        //        }
        //    });
        //    nowStateThread.IsBackground = true;
        //    nowStateThread.Start();
        //}



        static void StartListenConnectThread()
        {
            isListen = true;
            if (!isListenThreadOpen)
            {
                isListenThreadOpen = true;
                _th_KeepListen = new Thread(ServerListen);
                _th_KeepListen.IsBackground = true;
                _th_KeepListen.Start(_MyServer);
            }
        }
        //static void StartReceiveThread()
        //{
        //    if (serverReadData != null)
        //    {
        //        serverReadData.Abort();
        //    }
        //    serverReadData = new Thread(Received);
        //    serverReadData.IsBackground = true;
        //    serverReadData.Start(socketSend);
        //}

        //线程中阻塞的监听
        //三种关闭监听方式:
        //1.直接关闭socket  socket.close();    accept()抛出异常，终止监听；【socket会被关闭】
        //2.直接终止线程  th.Abort();    Abort()抛出ThreadAbortException异常，终止监听；【不知道内部Accept的情况】
        //3.线程信号false  逻辑上的关闭和开启，实际一直存在监听线程   【关闭只在socket.close()时关闭】
        //对3的说明：逻辑上讲，false时会一直accept阻塞，不排除有客户端连接，此时会结束监听线程；
        public static void IsListenConnect(bool islisten)
        {
            if (islisten)
            {
                StartListenConnectThread();
            }
            else
            {
                isListen = false;
            }
          
        }

        private static void ServerListen(object o)
        {
            try
            {
                Socket socketWatch = o as Socket;
                while (isListen)
                {
                    Socket tempClinet = socketWatch.Accept();//accept阻塞在这里，有连接才往后执行
                    //逻辑上的关闭启动，关闭时不添加连接到的socket
                    if (isListen)
                    {
                        socketClients.Add(tempClinet);
                        ClientHandle handle = new ClientHandle();
                        handle.Init(tempClinet);
                        handle.StartReceive();
                        clients.Add(handle);

                        //远程端口
                        Debug.Log(tempClinet.RemoteEndPoint.ToString() + ":" + "连接成功");
                        Debug.Log("当前连接数量：" + socketClients.Count);
                    }
                    else
                    {
                        Debug.Log(tempClinet.RemoteEndPoint.ToString() + ":" + "请求连接被拒绝");
                        tempClinet.Close();
                       
                        //线程结束标识
                        isListenThreadOpen = false;
                        Debug.LogError("【服务器监听连接线程】结束");
                    }
                    //socketTag = "建立新的连接";
                    //StartReceiveThread();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("【服务器监听连接线程】发生异常:" + ex.Message);
            }
        }

        static byte[] buffer;
        public static string str;
        private static void Received(object o)
        {
            try
            {
                Socket socketSend = o as Socket;

                while (true)
                {
                    //Debug.Log(socketSend.Connected);
                    buffer = new byte[1024];
                    int len = socketSend.Receive(buffer);//阻塞
                    if (len == 0) break;
                    str = Encoding.UTF8.GetString(buffer, 0, len);
                }
            }
            catch (System.Exception ex)
            {
                Debug.Log("接收消息线程异常：" + ex.Message);
            }
        }





        public static void CloseServer()
        {
            //侦听线程可能还在，需要关闭

            //关闭 客户端连接监听 线程
            if (_th_KeepListen != null)
            {
                _th_KeepListen.Abort();
            }

          
          
            //断开 服务器
            if(_MyServer!=null)
                _MyServer.Close();
        }






        public static void CloseServer_()
        {
            //关闭通讯
            for(int i = 0; i < socketClients.Count; i++)
            {
                socketClients[i].Shutdown(SocketShutdown.Both);
                socketClients[i].Close();
            }
            socketClients.Clear();





        }






    }

}
