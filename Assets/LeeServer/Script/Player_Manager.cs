using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player_Manager : MonoBehaviour
{
    [SerializeField] private GameObject cube;
    [SerializeField] private GameObject client_Cube;
    ushort myname;
    public ClientCube[] clientCubes = new ClientCube[4];
    public Cube[] cubes;

    private NetworkManager_Client client;
    public NetworkManager_Client Client { set { client = value; } }

    private NetworkManager_Server server;
    public NetworkManager_Server Server { set { server = value; }  get { return server; } }

    public bool isServer;
    public bool IsServer { set { isServer = value; } }

    // Start is called before the first frame update
    void Start()
    {
        Client = GameObject.Find("NetworkManager_Client").GetComponent<NetworkManager_Client>();
        Server = GameObject.Find("NetworkManager_Server").GetComponent<NetworkManager_Server>();
        isServer = Server.serverReady;
    }

    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {
            foreach (var i in server.ConnectedClients)
            {
                
                ServerClient client = i.Value;
                for (int j = 0; j < cubes.Length; j++)
                {
                    if (client.clientname == cubes[j].clientName)
                    {
                        cubes[j].transform.position = client.position;
                        cubes[j].transform.localScale = client.scale;
                        cubes[j].AnimChange(client.animation);
                        cubes[j].gun.transform.rotation = Quaternion.Euler(new Vector3(0, 0, client.gun));
                        cubes[j].gun.transform.localScale = client.scale;
                        if (cubes[j].itemInteger != client.item)
                        {
                            cubes[j].itemInteger = client.item;
                            if (cubes[j].itemInteger != 0)
                            {
                                cubes[j].GetItem();
                            }
                        }
                    }
                }
            }
        }
        
        else if(!isServer) 
        {
            if (client.tuples != null)
            {
                foreach (var i in client.tuples)
                {
                    if (myname == i.Item1)
                        continue;

                    ushort j = i.Item1;
                    clientCubes[j].AnimChange(i.Item5);
                    clientCubes[j].transform.position = i.Item2;
                    //gun.transform.localScale = new Vector3(i.Item3, 1, 1);
                    clientCubes[j].transform.localScale = new Vector3(i.Item3, 1, 1);
                    clientCubes[j].gun.transform.rotation = Quaternion.Euler(new Vector3(0, 0, i.Item4));
                    clientCubes[j].gun.transform.localScale = new Vector3(i.Item3, 1, 1);

                    if (client.getItems[clientCubes[j].clientName] != 0)
                    {
                        clientCubes[j].GetItem();
                        client.getItems[clientCubes[j].clientName] = 0;
                    }
                }
            }
        }
        
    }

    public void CreateCube()
    {
        
        if (isServer)
        {
            
            foreach(var i in server.ConnectedClients)
            {
               Instantiate(cube, this.gameObject.transform).GetComponent<Cube>().Init(server, i.Value.clientname);
            }
            cubes = GetComponentsInChildren<Cube>();
        }
        //else if(!isServer)
        //{
        //    myname = client.strRcvMyName;
        //    Instantiate(client_Cube, this.gameObject.transform);
        //    clientCubes = GetComponentsInChildren<ClientCube>();

        //    clientCubes[clientCubes.Length-1].Client = GameObject.Find("NetworkManager_Client").GetComponent<NetworkManager_Client>();
        //    clientCubes[clientCubes.Length - 1].ClientName = (ushort)(clientCubes.Length - 1);
        //    client.tuples.Add(new Tuple<ushort, Vector3, short, float, ushort>((ushort)(clientCubes.Length - 1), Vector3.zero, 1, 0, 0));
        //}
    }

    public void CreateCube(int i)
    {
        myname = client.strRcvMyName;

        for (int j = 0; j < i+1; j++)
        {
            client.tuples.Add(new Tuple<ushort, Vector3, short, float, ushort>((ushort)(j), Vector3.zero, 1, 0, 0));
            if (myname == j)
                continue;
            var k = Instantiate(client_Cube, this.gameObject.transform).GetComponent<ClientCube>();
            k.Client = client;
            k.ClientName = (ushort)j;
            clientCubes[j] = k;
        }
        
    }
}
