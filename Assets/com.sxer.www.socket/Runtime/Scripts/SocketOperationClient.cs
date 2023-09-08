using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


namespace Sxer.WWW.MySocket
{
    public static class SocketOperationClient
    {
        static UnityAction<byte[]> decodeBuffer = null;
        public static Socket socketClient;
        public static Thread clientConnect;


        //直接close
        //先shutdown再close
        //直接退出


        public static Thread dodododod;

        static IPEndPoint pEndPoint;

        public static string socketTag = string.Empty;
        public static bool ConnectServer(string serverip, int serverPort, UnityAction<byte[]> decodeDo = null)
        {
            try
            {
                IPAddress pAddress = IPAddress.Parse(serverip);
                pEndPoint = new IPEndPoint(pAddress, serverPort);
                socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                decodeBuffer = decodeDo;

                //保持线程
                //  dodododod = new Thread(LLLLLLLLL);
                dodododod = new Thread(KeeponlyConnect);
                dodododod.IsBackground = true;
                dodododod.Start(socketClient);


                //socketClient.Connect(pEndPoint);





                // Debug.Log("连接成功!");
                //创建线程，执行读取服务器消息
                //clientConnect = new Thread(Received);
                //clientConnect.IsBackground = true;
                //clientConnect.Start(socketClient);


                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("连接服务器失败:" + ex.Message);
                //创建线程，一直连服务器
                // clientConnect = new Thread(KeepConnect);
                // clientConnect.Start(socketClient);

                return false;
            }
        }

        static bool keepalive = false;

        static bool TestConnected(object o)
        {
            Socket socket = o as Socket;
            bool connectState = true;
            bool blockingState = socket.Blocking;
            //Debug.LogError("！");
            try
            {
                byte[] tmp = new byte[1];

                socket.Blocking = false;
                socket.Send(tmp, 0, 0);
                //Console.WriteLine("Connected!");
                connectState = true; //若Send错误会跳去执行catch体，而不会执行其try体里其之后的代码
            }
            catch (SocketException e)
            {
                Debug.LogError(e.Message);
                // 10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10035))
                {
                    //Console.WriteLine("Still Connected, but the Send would block");
                    connectState = true;
                }

                else
                {
                    //Console.WriteLine("Disconnected: error code {0}!", e.NativeErrorCode);
                    connectState = false;
                }
            }
            finally
            {
                socket.Blocking = blockingState;
            }


            return connectState;
        }
        private static void LLLLLLLLL(object o)
        {
            #region 过程
            // This is how you can determine whether a socket is still connected.

            Socket socket = o as Socket;


            while (true)
            {

                if (socket.Connected == false)
                {
                    bool connectState = true;
                    bool blockingState = socket.Blocking;
                    Debug.LogError("！");
                    try
                    {
                        byte[] tmp = new byte[1];

                        socket.Blocking = false;
                        socket.Send(tmp, 0, 0);
                        //Console.WriteLine("Connected!");
                        connectState = true; //若Send错误会跳去执行catch体，而不会执行其try体里其之后的代码
                    }
                    catch (SocketException e)
                    {
                        Debug.LogError(e.Message);
                        // 10035 == WSAEWOULDBLOCK
                        if (e.NativeErrorCode.Equals(10035))
                        {
                            //Console.WriteLine("Still Connected, but the Send would block");
                            connectState = true;
                        }

                        else
                        {
                            //Console.WriteLine("Disconnected: error code {0}!", e.NativeErrorCode);
                            connectState = false;
                        }
                    }
                    finally
                    {
                        socket.Blocking = blockingState;
                    }


                    if (connectState && !keepalive)//断线
                    {
                        if (clientConnect != null && clientConnect.IsAlive)
                        {
                            clientConnect.Abort();
                            keepalive = false;
                        }
                        else
                        {
                            Debug.Log("kaishi 连接");
                            clientConnect = new Thread(KeepConnect);
                            clientConnect.Start(socketClient);
                            keepalive = true;
                        }
                    }
                    else
                    {
                        Debug.Log("连接");
                        Thread.Sleep(3000);

                    }


                }

            }


            //Console.WriteLine("Connected: {0}", client.Connected);

            #endregion

        }

        static void KeepConnect(object o)
        {
            bool isconnect = false;
            while (!isconnect)
            {
                //yield return new WaitForSeconds(5);
                Debug.Log("连接1");
                try
                {
                    socketClient.Connect(pEndPoint);

                    Debug.Log("连接成功!");
                    isconnect = true;
                    // clientConnect = new Thread(Received);
                    //clientConnect.IsBackground = true;
                    //clientConnect.Start(socketClient);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("连接失败！");
                    Thread.Sleep(5000);
                }

            }

            Received(o);

        }

        static void KeeponlyConnect(object o)
        {
            while (true)
            {
                try
                {
                    if (socketClient != null && !socketClient.Connected)
                    {
                        socketClient.Connect(pEndPoint);

                        if (clientConnect != null && clientConnect.IsAlive)
                        {
                            clientConnect.Abort();
                        }
                        clientConnect = new Thread(Received);
                        clientConnect.IsBackground = true;
                        clientConnect.Start(socketClient);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("连接失败3s后重连:" + ex.Message);
                    Thread.Sleep(3000);
                }
            }
        }

        public static void SetDecodeBuffer(UnityAction<byte[]> decodedo = null)
        {
            decodeBuffer = decodedo;
        }


        static byte[] buffer;
        private static void Received(object o)
        {
            try
            {
                Socket socketSend = o as Socket;
                while (true)
                {
                    buffer = new byte[2048];
                    int len = socketSend.Receive(buffer);
                    if (len == 0)
                    {//获取到0则说明服务器断开连接

                        socketTag = "服务器断开连接";
                        socketClient.Close();
                        socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        break;
                    }


                    //获取到
                    //if (Encoding.UTF8.GetString(buffer).Contains("关闭发送连接xxx"))
                    //{
                    //    //socketClient.Shutdown(SocketShutdown.Receive);
                    //    socketSend.Shutdown(SocketShutdown.Receive);
                    //    socketSend.Close();
                    //    socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    //    clientConnect.Abort();
                    //    return;
                    //}
                    //  Debug.Log(Encoding.UTF8.GetString(buffer, 0, len));
                    if (decodeBuffer != null)
                        decodeBuffer(buffer);


#if UNITY_EDITOR
                    //   Debug.Log("服务器打印客户端返回消息：" + socketSend.RemoteEndPoint + ":" + str);
#endif
                    // Send("我收到了");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("信息接收错误！" + ex.Message);
            }
        }

        public static void Send(string msg)
        {
            try
            {
                byte[] buffer = new byte[1024];
                buffer = Encoding.UTF8.GetBytes(msg);
                socketClient.Send(buffer);
            }
            catch (System.Exception)
            {
                Debug.Log("未连接");
            }
        }

        public static void Send(byte[] buffer)
        {
            try
            {

                socketClient.Send(buffer);
            }
            catch (System.Exception)
            {
                Debug.Log("未连接");
            }
        }
        /// <summary>
        /// 关闭连接
        /// </summary>
        public static void close()
        {
            try
            {
                dodododod.Abort();

                clientConnect.Abort();
                socketClient.Close();
                //clientConnect.Abort();

                //Debug.Log("关闭客户端连接");
            }
            catch (System.Exception)
            {
                //Debug.Log("未连接");
            }
        }







    }

}
