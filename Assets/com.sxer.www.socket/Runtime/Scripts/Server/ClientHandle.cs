using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Sxer.WWW.MySocket
{
    public class ClientHandle
    {
        static int bufferLength = 1024;
        //客户端连接套接字
        Socket client;
        //接收消息线程
        Thread th_receive;

        bool isReceive = false;

        //缓存区
        byte[] buffer;
        //缓存区当前数据位置指针
        int startIndex = 0;


        public void Init(Socket c)
        {
            client = c;
        }


      
      
        //客户端断开三种
        //直接close
        //先shutdown再close
        //直接退出



        public void StartReceive()
        {
            isReceive = true;
            buffer = new byte[bufferLength];
            th_receive = new Thread(Received);
            th_receive.IsBackground = true;
            th_receive.Start();
        }
        
        public void CloseReceive()
        {
            isReceive = false;
        }

        //receive()往数据缓存区存储字节数据
        //数据解析方法，从数据缓存区提取 满足条件的(例如:包头包体形式,结束符形式,固定长度形式等)
        //将数据缓存区数据向前迁移
        //【注意需要满足在缓存区大小内，能够取出数据，否则缓存区满，receive无法再获取数据】
        
        void Received()
        {
            try
            {
                while (isReceive)
                {
                    //消息处理
                    //ParsingData_FixCount(1);//固定长度提取
                    //ParsingData_PackageData();//包头包体提取
                    ParsingData_TagEnd("#");//结束符提取

                    //缓冲区检测
                    //需要判断，数据解析方法是否在数据已满时都没有提取出来，否则得到len=0
                    if (buffer.Length - startIndex <= 0)
                    {
                        Debug.LogError("数据缓存区已满，未取到正确数据！(请调整缓存区大小，或修改信息提取方法)");
                    }

                    //
                    Debug.LogError(string.Format("当前缓存区长度：{0}\t缓存区剩余空间：{1}\t偏移位置：{2}", buffer.Length, buffer.Length - startIndex, startIndex));
                    //数据添加到缓存区
                    int len = client.Receive(buffer, startIndex,buffer.Length-startIndex, SocketFlags.None);//阻塞
                    //缓存区指针偏移
                    startIndex += len;
                    Debug.LogError(string.Format("获得数据长度：{0}\t偏移位置：{1}", len, startIndex));
                    if (len == 0)
                    {
                        Debug.LogError("客户端断开连接");
                        break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.Log("接收消息线程异常：" + ex.Message);
            }
        }


        public void ParsingData_FixCount(int count)
        {
            while (true)
            {
                //提取条件
                if (startIndex < count) break;
               
                //取到的数据
                string str = Encoding.UTF8.GetString(buffer, 0, count);
                Debug.LogError("得到数据:"+str);


                MoveData(count);
            }
        }

        //这里默认包头用4字节int    byte[] sizeByte = BitConverter.GetBytes(originalByte.Length);
        void ParsingData_PackageData()
        {
            while (true)
            {
                //data内容没有四个字节(标记的长度)
                if (startIndex <= 4) break;

                // 具体消息的内容长度
                int count = BitConverter.ToInt32(buffer, 0);
                //data内容包含一次发送的内容
                if (startIndex - 4 >= count)
                {
                    //输出消息内容
                    string str = Encoding.UTF8.GetString(buffer, 4, count);
                    Debug.LogError("得到数据:" + str);
                    //把未输出的内容前移，计算startIndex
                    MoveData(4+count);
                    //Array.Copy(data, 4 + count, data, 0, startIndex - count);
                    //startIndex = startIndex - count - 4;
                }
                else//data内容不及一次发送的内容
                {
                    break;
                }
            }
        }



        public void ParsingData_TagEnd(string tag)
        {
            while (true)
            {
                string str = Encoding.UTF8.GetString(buffer, 0, startIndex);
                //不存在结束符，继续获取
                if (!str.Contains(tag))
                    break;
                else//存在结束符
                {
                    //获取第一个结束符位置
                    int tagIndex = str.IndexOf(tag);
                    //获取开头到index的字符串 的 字节长度
                    //获取数据
                    string subStr = str.Substring(0, tagIndex);
                    Debug.LogError("得到数据:" + subStr);
                    //转字节
                    int subBufferCount = Encoding.UTF8.GetBytes(subStr+tag).Length;
                    Debug.LogError("迁移长度:" +subBufferCount);

                    MoveData(subBufferCount);
                }
            }
        }















        /// <summary>
        /// 数据提取迁移
        /// 把数据缓存区未输出的内容前移，计算startIndex
        /// </summary>
        /// <param name="moveLength">移动长度</param>
        void MoveData(int moveLength)
        {
            Array.Copy(buffer, moveLength, buffer, 0, startIndex - moveLength);
            startIndex = startIndex - moveLength;
        }






        public void Send_str(string msg)
        {
            if (client != null && client.Connected)
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    buffer = Encoding.UTF8.GetBytes(msg);
                    client.Send(buffer);
                }
                catch (System.Exception ex)
                {
                    Debug.Log("【发送数据】异常" + ex.Message);
                }
            }

        }
        public void Send_package(string msg)
        {
            if (client != null && client.Connected)
            {
                try
                {
                    byte[] buffer = SocketTool.PackageData(msg);
                    client.Send(buffer);
                }
                catch (System.Exception ex)
                {
                    Debug.Log("【发送数据】异常"+ex.Message);
                }
            }

        }

    }

}
