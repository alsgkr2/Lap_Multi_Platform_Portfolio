using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move_Object_Manager : MonoBehaviour
{
    JH_MapObject[] mapObjects;
    [SerializeField] NetworkManager_Server server;
    [SerializeField] NetworkManager_Client client;
    public bool isServer;
    public bool IsServer { set { isServer = value; } }

    public bool usingCannon = false;

    private void Start()
    {
        mapObjects = GetComponentsInChildren<JH_MapObject>();
        server = GameObject.Find("NetworkManager_Server").GetComponent<NetworkManager_Server>();
        isServer = server.serverReady;
    }
    /// <summary>
    /// 나는고드름일세
    /// </summary>
    /// <param name="ice"></param>
    public void Send(JH_IceSpike ice)
    {
        if (isServer)
        {
            server.SendServerObjMsg(integer: 1, objIndex: (ushort)Array.IndexOf(mapObjects, ice));
        }
        else if (!isServer)
        {
            client.SendClientObjMsg(integer: 1, objIndex: (ushort)Array.IndexOf(mapObjects, ice));
        }
    }

    /// <summary>
    /// 나는발판일세
    /// </summary>
    /// <param name="ice"></param>
    public void Send(JH_FallFloor Ff)
    {
        if (isServer)
        {
            server.SendServerObjMsg(integer: 0, objIndex: (ushort)Array.IndexOf(mapObjects, Ff));
        }
        else if (!isServer)
        {
            client.SendClientObjMsg(integer: 0, objIndex: (ushort)Array.IndexOf(mapObjects, Ff));
        }
    }

    /// <summary>
    /// 나는 대포일세
    /// </summary>
    /// <param name="can"></param>
    public void Send(JH_Cannon can, bool flag)
    {
        if (isServer)
        {
            server.SendServerCannonMsg(flag, (ushort)Array.IndexOf(mapObjects, can));
        }
        else if (!isServer)
        {
            client.SendClientCannonMsg(flag, (ushort)Array.IndexOf(mapObjects, can));
        }
    }

    public void Drop(int index)
    {
        mapObjects[index].Drop();
    }
}
