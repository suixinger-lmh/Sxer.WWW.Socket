using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Sxer.WWW.MySocket
{
    public class SocketTool : MonoBehaviour
    {
		/// <summary>
		/// 字符串数据 转带消息长度信息头的字节数据
		/// </summary>
		/// <param name="message">字符串数据</param>
		/// <returns></returns>
		public static byte[] PackageData(string message)
		{
			//消息 的 字节数据
			byte[] originalByte = Encoding.UTF8.GetBytes(message);
			//BitConverter.GetBytes(int)//长度信息占四个字节
			//消息长度 的 字节数据
			byte[] sizeByte = BitConverter.GetBytes(originalByte.Length);
			//
			Debug.LogError(originalByte.Length);
			Debug.Log(sizeByte.Length);
			return (sizeByte.Concat(originalByte).ToArray());
		}





    }

}
