using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sxer.WWW.MySocket;

public class ClientOpen : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SocketOperationClient.ConnectServer("192.168.8.11", 21102);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            for(int i = 0; i < 100; i++)
            {
                SocketOperationClient.Send("Hello Unity!");
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            for (int i = 0; i < 100; i++)
            {
                SocketOperationClient.Send(SocketTool.PackageData("Hello Unity!"));
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            for (int i = 0; i < 100; i++)
            {
                SocketOperationClient.Send("你好！U!#");
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SocketOperationClient.socketClient.Shutdown(System.Net.Sockets.SocketShutdown.Both);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SocketOperationClient.socketClient.Close();
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Application.Quit();
        }

    }
}
