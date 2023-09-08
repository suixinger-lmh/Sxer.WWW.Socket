using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sxer.WWW.MySocket;
public class ServerOpen : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

       // SocketTool.PackageData("我我我");


        SocketOperationServer.OpenServer_Tcp("192.168.8.11", 21102, 3);



    }



    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SocketOperationServer.IsListenConnect(false);
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SocketOperationServer.IsListenConnect(true);
        }
    }

    private void OnDestroy()
    {
        SocketOperationServer.CloseServer();
    }

}
