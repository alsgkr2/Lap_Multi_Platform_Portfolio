using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.UI;
using System.IO;
using System.Runtime.InteropServices;//�������� ���� �����
using UnityEngine.UIElements;
using CommonDataNameSpace;
using static Common_Data;

/// <summary>
/// Ŭ���̾�Ʈ Ŭ����
/// </summary>
public class ServerClient
{
    public TcpClient clientSocket;//Ŭ���̾�Ʈ ����(��� ����)
    public ushort clientname;//Ŭ���̾�Ʈ �̸�
    public Vector3 position;//Ŭ���̾�Ʈ ��ġ
    public Vector3 scale;// Ŭ���̾�Ʈ ������
    public UInt16 animation; //Ŭ���̾�Ʈ �ִϸ��̼�

    public int item; // �߰� Ŭ���̾�Ʈ ������

    public NetworkStream stream; // Ŭ���̾�Ʈ ��� ����

    public UInt16 UIt; //������ ���
    public float[] ultV; //������ ��ġ

    public float gun;

    //���� ������ �������
    public byte[] buffer;
    //���� �����Ͱ� �߸� ��츦 ����Ͽ� �ӽù��ۿ� �����Ͽ� ����
    public byte[] tempBuffer;//�ӽù���
    public bool isTempByte;//�ӽù��� ����
    public int nTempByteSize;//�ӽù����� ũ��

    public ServerClient(TcpClient clientSocket, ushort clientname = 0, Vector3 position = default)
    {
        this.clientSocket = clientSocket;
        this.clientname = clientname;
        this.position = position;
        this.scale = Vector3.one;
        this.stream = clientSocket.GetStream();
        this.animation = 0;
        this.item = 0; // �߰� Ŭ���̾�Ʈ ������

        this.ultV = new float[] { 0f, 0f, 0f};

        this.gun = 0;

        //������ ������� �ʱ�ȭ
        this.buffer = new byte[1024];
        //�ӽù��� �ʱ�ȭ
        this.tempBuffer = new byte[1024];
        this.isTempByte = false;
        this.nTempByteSize = 0;
    }
}

public class NetworkManager_Server : MonoBehaviour
{
    //������
    private Thread tcpListenerThread;
    //������ ����
    private TcpListener tcpListener;
    //Ŭ���̾�Ʈ
    //private ServerClient client;

    // Ŭ���̾�Ʈ ���
    public Dictionary<ushort,ServerClient> ConnectedClients; // ����� Ŭ���̾�Ʈ ���
    private List<ServerClient> disconnectedClients;  // ���� ������ Ŭ���̾�Ʈ ���

    //������, ��Ʈ
    public string ip;
    public int port;

    //���� ����
    public bool serverReady;
    //��� �޽��� �а� ���� ����
    //private NetworkStream stream;

    //�α�
    public Text ServerLog;//ui
    private List<string> logList;//data

    //���� �޽���
    public InputField Text_Input;

    //Ŭ���̾�Ʈ ��� UI
    public GameObject ButtonConnect;
    public GameObject ButtonDisConnect;
    public GameObject ClientFunctionUI;

    //�÷��̾� ������
    [SerializeField] private GameObject cube;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject gun;
    // ��¼� ���, ���� ����/�� ����
    public float finishTime;
    public List<Tuple<ushort, float>> finishTimes;
    private bool isGameStart = false;
    private bool isGameEnd = false;

    //���� ������ ó�� ����
    private Queue<stChangeInfoMsg> receive_changeInfo_MSG = new Queue<stChangeInfoMsg>();
    private Queue<stSendObj> receive_changeInfo_Obj = new Queue<stSendObj>(); // �����̴� ������Ʈ ������ ó�� ����
    private Queue<stSendCannon> receive_changeInfo_Cannon = new Queue<stSendCannon>(); // ���� ���� ������ ó�� ����
    private Queue<stSendUseItem> receive_useitem = new Queue<stSendUseItem>();


    //������ �޽��� �������
    public Action reset; // �ν��Ͻ� ����
    int serverItem; // ���� ���� ������ ����
    public Goal goal; 

    //�߰�
    Move_Object_Manager manager; // �����̴� ������Ʈ, �������� ��ũ��Ʈ

    // Start is called before the first frame update
    void Start()
    {

        //�α� �ʱ�ȭ
        logList = new List<string>();


        //Ŭ���̾�Ʈ ��� �ʱ�ȭ
        ConnectedClients = new Dictionary<ushort, ServerClient>();
        disconnectedClients = new List<ServerClient>();

        serverItem = 0; // ���� ������ �ʱ�ȭ

        // ��ũ��Ʈ ������Ʈ ��������
        manager = GameObject.Find("MoveObject").GetComponent<Move_Object_Manager>();
        //if (manager != null) ;
    }

    // Update is called once per frame
    void Update()
    {
        if (receive_useitem.Count > 0)
        {
            stSendUseItem stSendUse = receive_useitem.Dequeue();

            foreach (var i in ConnectedClients)
            {
                if (i.Key == stSendUse.sendClientName)
                {
                    Vector3 lookVector = new Vector3(stSendUse.rotation[0], stSendUse.rotation[1], 0);
                    var b = Instantiate(bullet, i.Value.position + lookVector * 3 + Vector3.up * 1.5f, Quaternion.Euler(lookVector)).GetComponent<bullet>();
                    b.v = lookVector;
                }
            }
        }
        // �����̴� ������Ʈ ������ ó��
        if (receive_changeInfo_Obj.Count > 0)
        {
            stSendObj CreateObjMsg = receive_changeInfo_Obj.Dequeue();

            manager.Drop(CreateObjMsg.objIndex);
        }
        // ���� ������ ó��
        if (receive_changeInfo_Cannon.Count > 0)
        {
            stSendCannon CreateObjMsg = receive_changeInfo_Cannon.Dequeue();

            manager.usingCannon = CreateObjMsg.useCannon;
            manager.Drop(CreateObjMsg.CannonIndex);
        }

        //���� �����Ͱ� �ִ°��(�� ���� Ȯ��)
        if (receive_changeInfo_MSG.Count > 0)
        {
            //���ʴ�� �̾Ƴ���.
            stChangeInfoMsg CreateObjMsg = receive_changeInfo_MSG.Dequeue();

            if(ConnectedClients.ContainsKey(CreateObjMsg.sendClientName))
            {
                //�����͸� �ִ´�.
                ConnectedClients[CreateObjMsg.sendClientName].position.x = CreateObjMsg.position[0];
                ConnectedClients[CreateObjMsg.sendClientName].position.y = CreateObjMsg.position[1];
                ConnectedClients[CreateObjMsg.sendClientName].gun = CreateObjMsg.ItemRotationZ;
                ConnectedClients[CreateObjMsg.sendClientName].scale.x = CreateObjMsg.scale;
                ConnectedClients[CreateObjMsg.sendClientName].animation = CreateObjMsg.currentAni;
            }
        }

        // �ð� ����
        if (isGameStart)
        {
            finishTime += Time.deltaTime;
        }

        //�α׸���Ʈ�� �׿��ٸ�
        if (logList.Count > 0)
        {
            //�α� ���
            WriteLog(logList[0]);
            logList.RemoveAt(0);
        }

        /*
        if(serverItem != 0)
        {
            stSendGunPosMsg str = new stSendGunPosMsg();
            str.SendClientName = "Server";
            str.MsgID = 14;
            str.PacketSize = (ushort)Marshal.SizeOf(str);
            str.RotationZ = jyPlayer.gun.transform.rotation.z;
            str.Scale = new float[]{ jyPlayer.gun.transform.localScale.x , jyPlayer.gun.transform.localScale.y, jyPlayer.gun.transform.localScale.z};

            byte[] SendMsg = GetSendGunPosMsgToByte(str);
            BroadCastByte(SendMsg);
        }
        */

        //���� ���¿� ���� Ŭ���̾�Ʈ ��ư Ȱ��ȭ/��Ȱ��ȭ
        if (ButtonConnect != null)
        {
            ButtonConnect.SetActive(!serverReady);
            ButtonDisConnect.SetActive(!serverReady);
            ClientFunctionUI.SetActive(!serverReady);
        }
           
    }

    /// <summary>
    /// ���� ���� ��ư
    /// </summary>
    public void ServerCreate()
    {
        //ip, port ����
        port = int.Parse(GameObject.Find("Text_Port").GetComponent<InputField>().text);

        // TCP���� ��� ������ ����
        tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingRequeset));
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();
    }

    /// <summary>
    /// ���� ������ ����
    /// </summary>
    private void ListenForIncommingRequeset()
    {
        try
        {
            // ���� ����
            tcpListener = new TcpListener(IPAddress.Any/*������ ���� ������ IP*/, port);
            tcpListener.Start();

            // ���� ���� ON
            serverReady = true;

            // �α� ���
            logList.Add("[�ý���] ���� ����(port:" + port + ")");

            // ������ ���ú� �׽� ���(Update)
            while (true)
            {
                // ������ ������ ���ٸ�
                if(!serverReady)
                    break;

                //���� �õ����� Ŭ���̾�Ʈ Ȯ��
                if(tcpListener != null && tcpListener.Pending())
                {
                    // ����� Ŭ���̾�Ʈ ��Ͽ� ����
                    ushort clientName = (ushort)(ConnectedClients.Count + 1);

                    ConnectedClients.Add(clientName, new ServerClient(tcpListener.AcceptTcpClient(),clientName));


                    BroadCast(clientName + " ����!");

                    stHeader stHeaderTmp = new stHeader();

                    stHeaderTmp.MsgID = 3;
                    stHeaderTmp.sendClientName = clientName;
                    stHeaderTmp.PacketSize = (ushort)Marshal.SizeOf(stHeaderTmp);//�޽��� ũ��

                    byte[] SendData = GetHeaderToByte(stHeaderTmp);

                    ConnectedClients[clientName].stream.Write(SendData, 0, SendData.Length);
                    ConnectedClients[clientName].stream.Flush();
                }

                //���ӵ� Ŭ���̾�Ʈ ����� ��ȣ�ۿ� ó��
                foreach(var DicClient in ConnectedClients)
                {
                    ServerClient client = DicClient.Value;

                    if (client != null)
                    {
                        //Ŭ���̾�Ʈ ���� �����
                        if (!IsConnected(client.clientSocket))
                        {
                            
                            // �̰����� �ٷ� Ŭ���̾�Ʈ�� �����ϸ� �����尣�� ������ ���̷� ������ �߻������� ���������� Ŭ���̾�Ʈ ������� ����
                            //logList.Add("[�ý���] Ŭ���̾�Ʈ ���� ����");
                            
                            // ���������� Ŭ���̾�Ʈ ��Ͽ� �߰�
                            disconnectedClients.Add(client);

                            continue;
                        }
                        //Ŭ���̾�Ʈ �޽��� ó��
                        else
                        {
                            //�޽����� ���Դٸ�
                            if (client.stream.DataAvailable)
                            {
                                //�޽��� ���� ���� �ʱ�ȭ
                                Array.Clear(client.buffer, 0, client.buffer.Length);

                                //�޽����� �д´�.
                                int messageLength = client.stream.Read(client.buffer, 0, client.buffer.Length);

                                //���� ó���ϴ� ����
                                byte[] pocessBuffer = new byte[messageLength + client.nTempByteSize];//���� �о�� �޽����� ���� �޽����� ����� ���ؼ� ó���� ���� ����
                                                                                              //���Ҵ� �޽����� �ִٸ�
                                if (client.isTempByte)
                                {
                                    //�� �κп� ���Ҵ� �޽��� ����
                                    Array.Copy(client.tempBuffer, 0, pocessBuffer, 0, client.nTempByteSize);
                                    //���� ���� �޽��� ����
                                    Array.Copy(client.buffer, 0, pocessBuffer, client.nTempByteSize, messageLength);
                                }
                                else
                                {
                                    //���Ҵ� �޽����� ������ ���� �о�� �޽����� ����
                                    Array.Copy(client.buffer, 0, pocessBuffer, 0, messageLength);
                                }

                                //ó���ؾ� �ϴ� �޽����� ���̰� 0�� �ƴ϶��
                                if (client.nTempByteSize + messageLength > 0)
                                {
                                    //���� �޽��� ó��
                                    OnIncomingData(client, pocessBuffer);
                                }
                            }
                            else if (client.nTempByteSize > 0)
                            {
                                byte[] pocessBuffer = new byte[client.nTempByteSize];
                                Array.Copy(client.tempBuffer, 0, pocessBuffer, 0, client.nTempByteSize);
                                OnIncomingData(client, pocessBuffer);
                            }
                        }
                    }
                }
                
                //���� ������ Ŭ���̾�Ʈ ��� ó��
                for(int i = disconnectedClients.Count-1; i >= 0; i--)
                {
                    //�αױ��
                    logList.Add("[�ý���]Ŭ���̾�Ʈ ���� ����");
                    //���ӵ� Ŭ���̾�Ʈ ��Ͽ��� ����
                    ConnectedClients.Remove(disconnectedClients[i].clientname);
                    // ó���� ���������� Ŭ���̾�Ʈ ��Ͽ��� ����
                    disconnectedClients.Remove(disconnectedClients[i]);
                }
                
                //����� Ŭ���̾�Ʈ ���(connectedClients)�� �߰��� �Ǿ� foreach���� Ÿ�� ������ ������ �ȵ��� client�� null�� �Ǵ� ������ �߻��Ͽ� �����̸� �ش�
                Thread.Sleep(10);
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("SocketException " + socketException.ToString());
        }
    }

    /// <summary>
    /// Ŭ���̾�Ʈ ���� Ȯ��
    /// </summary>
    /// <param name="client"></param>
    /// <returns></returns>
    private bool IsConnected(TcpClient client)
    {
        try
        {
            if(client != null && client.Client != null && client.Client.Connected)
            {
                if(client.Client.Poll(0, SelectMode.SelectRead))
                {
                    return !(client.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// ���� �޽��� ó��
    /// </summary>
    /// <param name="client"></param>
    /// <param name="data"></param>
    private void OnIncomingData(ServerClient client, byte[] data)
    {

        //BroadCastByte(data);
        //return;

        // �������� ũ�Ⱑ ����� ũ�⺸�ٵ� ������
        if (data.Length < Constants.HEADER_SIZE)
        {
            Array.Copy(data, 0, client.tempBuffer, client.nTempByteSize, data.Length);     // ���� ���� ���ۿ� ���� �޽��� ����
            client.isTempByte = true;
            client.nTempByteSize += data.Length;
            return;
        }

        //����κ� �߶󳻱�(�����ϱ�)
        byte[] headerDataByte = new byte[Constants.HEADER_SIZE];
        Array.Copy(data, 0, headerDataByte, 0, headerDataByte.Length); //��� ������ ��ŭ ������ ����
        //��� ������ ����üȭ(������)
        stHeader headerData = HeaderfromByte(headerDataByte);

        // ����� ������� ���� �޽����� ����� ������
        if (headerData.PacketSize > data.Length)
        {
            Array.Copy(data, 0, client.tempBuffer, client.nTempByteSize, data.Length);     // ���� ���� ���ۿ� ���� �޽��� ����
            client.isTempByte = true;
            client.nTempByteSize += data.Length;
            return;
        }

        //����� �޽���ũ�⸸ŭ�� �޽��� �����ϱ�
        byte[] msgData = new byte[headerData.PacketSize]; //��Ŷ �и��� ���� ���� ���� ����� ��Ŷ �����ŭ ���� ����
        Array.Copy(data, 0, msgData, 0, headerData.PacketSize); //������ ���ۿ� ��Ŷ ���� ����

        //����� �޽�����
        if (headerData.MsgID == 0)//�� ���� Ȯ��
        {
            //Ŭ���̾�Ʈ�� ������ Ŭ���̾�Ʈ���� ������.
            if (ConnectedClients.ContainsKey(headerData.sendClientName))
            {
                ServerClient clientInfo = ConnectedClients[headerData.sendClientName];

                stChangeInfoMsg stChangeInfoMsgData = new stChangeInfoMsg();

                float[] positionArray = { clientInfo.position.x, clientInfo.position.y, clientInfo.position.z };
                short scaleArray = (short)clientInfo.scale.x;

                //�޽��� �ۼ�
                stChangeInfoMsgData.sendClientName = clientInfo.clientname;
                stChangeInfoMsgData.MsgID = 0;//�޽��� ID
                stChangeInfoMsgData.PacketSize = (ushort)Marshal.SizeOf(stChangeInfoMsgData);//�޽��� ũ��
                stChangeInfoMsgData.position = positionArray;
                stChangeInfoMsgData.scale = scaleArray;

                byte[] SendData = GetChangeInfoMsgToByte(stChangeInfoMsgData);

                clientInfo.stream.Write(SendData, 0, SendData.Length);
                clientInfo.stream.Flush();
            }
        }
        else if (headerData.MsgID == 1)//�� ���� ����
        {
            stChangeInfoMsg stChangeInfoMsg1 = ChangeInfoMsgfromByte(msgData);
            receive_changeInfo_MSG.Enqueue(stChangeInfoMsg1);

            Debug.Log(client.clientname + " : �� ���� ���� �޽��� ����");
        }
        else if (headerData.MsgID == 2)//�޽���
        {
            stSendMsg SendMsgInfo = SendMsgfromByte(msgData);

            BroadCastByte(msgData);
            //�޽��� �α׿� ���
            logList.Add(client.clientname + " : " + SendMsgInfo.strSendMsg);
        }
        else if (headerData.MsgID == 12)// �߰� ������ ȹ��
        {
            stSendInt SendItmeInfo = SendIntfromByte(msgData);
            ConnectedClients[SendItmeInfo.sendClientName].item = SendItmeInfo.integer;
            BroadCastByte(msgData);
            Debug.Log(ConnectedClients[SendItmeInfo.sendClientName].item);
        }
        else if (headerData.MsgID == 13)
        {

            //������ ��� ó��
            stSendUseItem stSend = SendUseItemfromByte(data);
            receive_useitem.Enqueue(stSend); 

            BroadCastByte(data);

        }
        else if (headerData.MsgID == 20)
        {
            stSendMsg sendMsg = SendMsgfromByte(data);
            finishTimes.Add(new Tuple<ushort, float>(sendMsg.sendClientName, float.Parse(sendMsg.strSendMsg)));
            
            if (!isGameEnd)
            {
                Debug.Log("server�� Ŭ���̾�Ʈ ī��Ʈ �ٿ� ����");
                isGameEnd = true;
                stHeader header = new stHeader();
                header.sendClientName = sendMsg.sendClientName;
                header.MsgID = 20;
                header.PacketSize = (ushort)Marshal.SizeOf(header);

                BroadCastByte(GetHeaderToByte(header));
            }
        }
        else if (headerData.MsgID == 21)// �߰� �����̴� ������Ʈ ����
        {
            stSendObj SendItmeInfo = SendObjfromByte(msgData);
            receive_changeInfo_Obj.Enqueue(SendItmeInfo);
            BroadCastByte(msgData);
        }
        else if (headerData.MsgID == 22)// �߰� ���� ����
        {
            stSendCannon SendItmeInfo = SendCannonfromByte(msgData);
            receive_changeInfo_Cannon.Enqueue(SendItmeInfo);
            BroadCastByte(msgData);
        }
        else//�ĺ����� ���� ID
        {

        }

        // ��� �޽����� ó���Ǽ� ���� �޽����� ���� ��� 
        if (data.Length == msgData.Length)
        {
            client.isTempByte = false;
            client.nTempByteSize = 0;
        }
        // �޽��� ó�� �� �޽����� �����ִ� ���
        else
        {
            //�ӽ� ���� û��
            Array.Clear(client.tempBuffer, 0, client.tempBuffer.Length);

            //������ ���ۿ� ��Ŷ ���� ����
            Array.Copy(data, msgData.Length, client.tempBuffer, 0, data.Length - (msgData.Length));// �ӽ� ���� ���ۿ� ���� �޽��� ����
            client.isTempByte = true;
            client.nTempByteSize += data.Length - (msgData.Length);
        }
    }

    public void BroadCastByte(byte[] data)
    {
        foreach(var client in ConnectedClients)
        {
            client.Value.stream.Write(data,0, data.Length);
            client.Value.stream.Flush();
        }
    }


    /// <summary>
    /// �α� ����
    /// </summary>
    /// <param name="message"></param>
    public void WriteLog(/*Time*/string message)
    {
        ServerLog.GetComponent<Text>().text += message + "\n";
    }

    public void GameStart()
    {
        
        Player_Manager manager = GameObject.Find("Player_Manager").GetComponent<Player_Manager>();
        if (manager != null)
        manager.CreateCube();
        foreach (var i in ConnectedClients)
        {
            //Instantiate(cube).GetComponent<Cube>().Init(this, i.Value.clientname);

            stSendInt sendInt = new stSendInt();

            sendInt.sendClientName = 0;
            sendInt.integer = (ushort)ConnectedClients.Count;
            sendInt.PacketSize = (ushort)Marshal.SizeOf(sendInt);
            sendInt.MsgID = 10;

            byte[] SendData = GetSendIntToByte(sendInt);
            i.Value.stream.Write(SendData, 0, SendData.Length);
            i.Value.stream.Flush();            
        }
        
        isGameStart = true;
        goal = GameObject.Find("Goal").GetComponent<Goal>();
        player = GameObject.Find("Player");
        gun = player.transform.GetChild(2).gameObject;
        goal.isGameStart = true;
        finishTimes = new List<Tuple<ushort, float>>();
        StartCoroutine(PosSynchronization());
    }

    ushort ani => player.GetComponent<JY_Player>().aniNum;
    private IEnumerator PosSynchronization()
    {
        yield return new WaitForSeconds(0.05f);

        stAllChangeInfoMsg allChangeInfoMsg = new stAllChangeInfoMsg();   
        allChangeInfoMsg.MsgID = 0;
        allChangeInfoMsg.PacketSize = (ushort)Marshal.SizeOf(allChangeInfoMsg);

        float[] pos = { player.transform.position.x, player.transform.position.y };
        allChangeInfoMsg.position0 = pos;
        allChangeInfoMsg.currentAni0 = ani;
        allChangeInfoMsg.ItemRotationZ0 = gun.transform.rotation.eulerAngles.z;
        allChangeInfoMsg.scale0 = (short)player.transform.localScale.x;

        ServerClient client = ConnectedClients[1];
        pos = new float[]{ client.position.x, client.position.y };
        allChangeInfoMsg.position1 = pos;
        allChangeInfoMsg.currentAni1 = client.animation;
        allChangeInfoMsg.ItemRotationZ1 = client.gun;
        allChangeInfoMsg.scale1 = (short)client.scale.x;

        if (ConnectedClients.Count > 1)
        {
            client = ConnectedClients[2];
            pos = new float[] { client.position.x, client.position.y };
            allChangeInfoMsg.position2 = pos;
            allChangeInfoMsg.currentAni2 = client.animation;
            allChangeInfoMsg.ItemRotationZ2 = client.gun;
            allChangeInfoMsg.scale2 = (short)client.scale.x;
        }
        if (ConnectedClients.Count > 2) {
            client = ConnectedClients[3];
            pos = new float[] { client.position.x, client.position.y };
            allChangeInfoMsg.position3 = pos;
            allChangeInfoMsg.currentAni3 = client.animation;
            allChangeInfoMsg.ItemRotationZ3 = client.gun;
            allChangeInfoMsg.scale3 = (short)client.scale.x;
        }

        byte[] SendMsg = GetAllChangeInfoMsgToByte(allChangeInfoMsg);
        BroadCastByte(SendMsg);

        StartCoroutine(PosSynchronization());
    }

    public void useItem(Vector3 vector)
    {
        stSendUseItem i = new stSendUseItem();
        i.PacketSize = (ushort)Marshal.SizeOf(i);
        i.MsgID = 13;
        float[] floatArray = { vector.x, vector.y, 0f };
        i.rotation = floatArray;
        i.useitem = 1;
        i.sendClientName = 0;

        BroadCastByte(GetSendUseItemToByte(i));
    }

    public void GameRestart()
    {
        //loglist
        foreach (var i in ConnectedClients)
        {
            i.Value.position = default;
            i.Value.scale = Vector3.one;
        }
        //reset?.Invoke();
        ServerLog.GetComponent<Text>().text = "";

    }
    public void Rank()
    {
        finishTimes.Sort((a, b) => a.Item2.CompareTo(b.Item2));

        stSendRanking sendRanking = new stSendRanking();
        ushort[] clientNames = new ushort[4];
        float[] times = new float[finishTimes.Count];
        for (int i = 0; i < finishTimes.Count; i++)
        {
            times[i] = finishTimes[i].Item2;
            clientNames[i] = finishTimes[i].Item1;
        }
        for (int i = 3; i >= finishTimes.Count; i--)
        {
            clientNames[i] = 5; 
        }

        //RankLog rankLog = GameObject.Find("RankLog").GetComponent<RankLog>();
        
        sendRanking.time = times;
        sendRanking.ClientNames = clientNames;
        sendRanking.MsgID = 30;
        sendRanking.PacketSize = (ushort)Marshal.SizeOf(sendRanking);

        //rankLog.RankLogPrint(sendRanking);
        BroadCastByte(GetSendRankToByte(sendRanking));
        
        //GameRestart();
    }
    

    public void SendMsg()
    {
        // ���� ���� ����ü �ʱ�ȭ
        stSendMsg stSendMsgInfo = new stSendMsg();

        string strSendMsg = Text_Input.text;

        //�޽��� �ۼ�
        stSendMsgInfo.sendClientName = 0;
        stSendMsgInfo.MsgID = 2;//�޽��� ID
        stSendMsgInfo.PacketSize = (ushort)Marshal.SizeOf(stSendMsgInfo);//�޽��� ũ��
        stSendMsgInfo.strSendMsg = strSendMsg;

        //����ü ����Ʈȭ �� ����
        byte[] SendData = GetSendMsgToByte(stSendMsgInfo);

        bool bCheckSend = false;
        foreach (var client in ConnectedClients)
        {
            client.Value.stream.Write(SendData, 0, SendData.Length);
            client.Value.stream.Flush();
            bCheckSend = true;
        }
        //�α� ���
        if(bCheckSend)
            logList.Add("���� : " + strSendMsg);
    }
    public void GameEnd()
    {
        stHeader sendMsg = new stHeader();
        sendMsg.sendClientName = 0;
        sendMsg.MsgID = 20;
        sendMsg.PacketSize = (ushort)Marshal.SizeOf(sendMsg);

        
        BroadCastByte(GetHeaderToByte(sendMsg));
        isGameStart = false;
        if (!isGameEnd)
        {
            isGameEnd = true;
            
        }
        finishTimes.Add(new Tuple<ushort, float>(0, finishTime));
    }

    /// <summary>
    /// �޽��� ����
    /// </summary>
    public void Send(ServerClient client, string message = "")
    {
        //������ �����°� �ƴ϶��
        if (!serverReady)
            return;

        //������ �ƴѰ�� �Է��� �ؽ�Ʈ ����
        if(message == "")
        {
            // ���� ���� ����ü �ʱ�ȭ
            stSendMsg stSendMsgInfo = new stSendMsg();

            string strSendMsg = Text_Input.text;

            //�޽��� �ۼ�
            stSendMsgInfo.sendClientName = 0;
            stSendMsgInfo.MsgID = 2;//�޽��� ID
            stSendMsgInfo.PacketSize = (ushort)Marshal.SizeOf(stSendMsgInfo);//�޽��� ũ��
            stSendMsgInfo.strSendMsg = strSendMsg;

            //����ü ����Ʈȭ �� ����
            byte[] SendData = GetSendMsgToByte(stSendMsgInfo);

            foreach (var i in ConnectedClients)
            {
                i.Value.stream.Write(SendData, 0, SendData.Length);
                i.Value.stream.Flush();
            }

            //�α� ���
            logList.Add("���� : " + strSendMsg);
        }
        else
        {
            try
            {
                stSendMsg stSendMsgInfo = new stSendMsg();

                stSendMsgInfo.sendClientName = 0;
                stSendMsgInfo.MsgID = 2;//�޽��� ID
                stSendMsgInfo.PacketSize = (ushort)Marshal.SizeOf(stSendMsgInfo);//�޽��� ũ��
                stSendMsgInfo.strSendMsg = message;

                //����ü ����Ʈȭ �� ����
                byte[] sendMessageByte = GetSendMsgToByte(stSendMsgInfo);

                //����
                client.stream.Write(sendMessageByte, 0, sendMessageByte.Length);
                client.stream.Flush();

                //�α� ���
                logList.Add("���� : " + message);
            }
            catch (Exception e)
            {
                Debug.Log("SendException " + e.ToString());
            }
        }

        
    }   

    /// <summary>
    /// ���� �ݱ�
    /// </summary>
    public void CloseSocket()
    {
        //������ ������ ���ٸ�
        if (!serverReady)
        {
            return;
        }
        else//�ʱ�ȭ
        {
            //Ŭ���̾�Ʈ���� ���� ���� ����
            BroadCast("���� ����!");

            
            //���� ���� �� �ʱ�ȭ
            if (tcpListener != null) { tcpListener.Stop(); tcpListener = null; }

            //���� �ʱ�ȭ
            serverReady = false;

            //������ �ʱ�ȭ
            tcpListenerThread.Abort();
            tcpListenerThread = null;

            //����� Ŭ���̾�Ʈ �ʱ�ȭ
            foreach (var client in ConnectedClients)
            {
                client.Value.stream = null;
                client.Value.clientSocket.Close();
            }
            ConnectedClients.Clear();
        }
    }

    /// <summary>
    /// �߰� ���������� ȹ��
    /// </summary>
    public void GainServerItem()
    {
        //������°� �ƴ� ���
        if (!serverReady)
        {
            return;
        }

        stSendInt stSendint = new stSendInt();

        stSendint.MsgID = 12;
        stSendint.sendClientName = 0;
        stSendint.PacketSize = (ushort)Marshal.SizeOf(stSendint);
        stSendint.integer = 1;
        serverItem = stSendint.integer;

        byte[] sendMsg = GetSendIntToByte(stSendint);

        BroadCastByte(sendMsg);
    }

    public void BroadCast(string message)
    {
        foreach (var client in ConnectedClients)
        {
            Send(client.Value, message);
        }
    }

    /// <summary>
    /// �߰� ���� ������Ʈ ����
    /// </summary>
    public void SendServerObjMsg(ushort integer, ushort objIndex)
    {
        stSendObj sendObj = new stSendObj();
        sendObj.MsgID = 21;
        sendObj.PacketSize = (ushort)Marshal.SizeOf(sendObj);
        sendObj.integer = integer;
        sendObj.objIndex = objIndex;
        
        byte[] sendMsg = GetSendObjToByte(sendObj);
        BroadCastByte(sendMsg);
    }

    /// <summary>
    /// �߰� ���� ���� ����
    /// </summary>
    public void SendServerCannonMsg(bool use, ushort cannonIndex)
    {
        stSendCannon sendObj = new stSendCannon();
        sendObj.MsgID = 22;
        sendObj.PacketSize = (ushort)Marshal.SizeOf(sendObj);
        sendObj.useCannon = use;
        sendObj.CannonIndex = cannonIndex;

        byte[] sendMsg = GetSendCannonToByte(sendObj);
        BroadCastByte(sendMsg);
    }

    /// <summary>
    /// ���� �����
    /// </summary>
    private void OnApplicationQuit()
    {
        CloseSocket();
    }
}
