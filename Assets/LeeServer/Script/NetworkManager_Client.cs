using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using UnityEngine.UI;
using System.Runtime.InteropServices;//�������� ���� �����
using CommonDataNameSpace;
using static Common_Data;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

public class NetworkManager_Client : MonoBehaviour
{
    //������
    private Thread tcpListenerThread;

    //����
    private TcpClient socketConnection;
    public NetworkStream stream;

    //����
    private bool clientReady;

    //ip, port
    public string ip;
    public int port;

    //�α�
    public Text ClientLog;
    private List<string> logList;
    //���� �޽���
    public GameObject Text_Input;

    //���� ��� UI
    public GameObject ButtonServerOpen;
    public GameObject ButtonServerClose;
    public GameObject ServerFunctionUI;

    //���� ������ �������
    byte[] buffer;
    //���� �����Ͱ� �߸� ��츦 ����Ͽ� �ӽù��ۿ� �����Ͽ� ����
    byte[] tempBuffer;//�ӽù���
    bool isTempByte;//�ӽù��� ����
    int nTempByteSize;//�ӽù����� ũ��

    //������ �޽��� �������
    byte[] sendMessage = new byte[1024];

    [SerializeField] Transform player;
    [SerializeField] GameObject cube;
    [SerializeField] GameObject bullet;
    [SerializeField] GameObject gun;
    public List<Tuple<ushort, Vector3, short, float, ushort>> tuples;
    public ushort strRcvMyName;
    private ushort integer;

    //�߰�
    Move_Object_Manager manager;
    public List<Tuple<ushort, float>> finishTimes;

    //���� ������ ó�� ����
    private Queue<stSendObj> receive_changeInfo_Obj = new Queue<stSendObj>();
    private Queue<stSendCannon> receive_changeInfo_Cannon = new Queue<stSendCannon>();
    private Queue<stSendUseItem> receive_useitem = new Queue<stSendUseItem>();


    private float finishTime;
    private bool isGameStart = false; 
    private bool isGameEnd = false;
    public ushort[] getItems = { 0, 0, 0};
    public Action reset;
    public bool resetbool = false;
    public Goal goal;

    // Start is called before the first frame update
    void Start()
    {
        
        //�α� �ʱ�ȭ
        logList = new List<string>();
        //���� ������ ������� �ʱ�ȭ
        buffer = new byte[1024];
        //�ӽù��� �ʱ�ȭ
        tempBuffer = new byte[1024];
        isTempByte = false;
        nTempByteSize = 0;
        //�߰�
        SceneManager.sceneLoaded += OnSceneStart;
       
        //if (goal != null) goal = GameObject.Find("Goal").GetComponent<Goal>();
    }

    void OnSceneStart(Scene scene , LoadSceneMode mode)
    {
        try
        {
            manager = GameObject.Find("MoveObject").GetComponent<Move_Object_Manager>();
            player = GameObject.Find("Player").GetComponent<Transform>();
            gun = player.transform.GetChild(2).gameObject;
        }
        catch
        {

        }
        
    }
    // Update is called once per frame
    void Update()
    {
        if (receive_useitem.Count > 0)
        {
            stSendUseItem stSendUse = receive_useitem.Dequeue();

            foreach(var i in tuples)
            {
                if(i.Item1 == stSendUse.sendClientName)
                {
                    Vector3 lookVector = new Vector3(stSendUse.rotation[0], stSendUse.rotation[1], 0);
                    var b = Instantiate(bullet, i.Item2 + lookVector * 3 + Vector3.up * 1.5f, Quaternion.Euler(lookVector)).GetComponent<bullet>();
                    b.v = lookVector;
                }
            }
        }
        if(receive_changeInfo_Obj.Count > 0)
        {
            stSendObj CreateObjMsg = receive_changeInfo_Obj.Dequeue();

            manager.Drop(CreateObjMsg.objIndex);
        }

        if (receive_changeInfo_Cannon.Count > 0)
        {
            stSendCannon CreateObjMsg = receive_changeInfo_Cannon.Dequeue();

            manager.usingCannon = CreateObjMsg.useCannon;
            manager.Drop(CreateObjMsg.CannonIndex);
        }

        //�α׸���Ʈ�� �׿��ٸ�
        if (logList.Count > 0)
        {
            //����
            WriteLog(logList[0]);
            logList.RemoveAt(0);
        }

        if (isGameStart)
        {
            finishTime += Time.deltaTime;
        }

        if (integer > 0)
        {
            /*
            ClientCube instant = Instantiate(cube).GetComponent<ClientCube>();
            instant.Client = this;
            instant.ClientName = 0;
            tuples.Add(new Tuple<ushort, Vector3, short, float, ushort>(0, Vector3.zero, 1, 0, 0));
            */
            StartCoroutine(enumerator(integer));
            integer = 0;
        }
        if (resetbool)
        {
            GameRestart();
        }

        //Ŭ���̾�Ʈ ���¿� ���� ���� ��ư Ȱ��ȭ/��Ȱ��ȭ
        if (ButtonServerOpen != null)
        {
            ButtonServerOpen.SetActive(!clientReady);
            ButtonServerClose.SetActive(!clientReady);
            ServerFunctionUI.SetActive(!clientReady);
        }
    }

    IEnumerator enumerator(int i)
    {
        SceneManager.LoadScene("GAME");
        yield return new WaitForSeconds(0.05f);
        Player_Manager manager = GameObject.Find("Player_Manager").GetComponent<Player_Manager>();
        goal = GameObject.Find("Goal").GetComponent<Goal>();
        manager.CreateCube(i);
        goal.isGameStart = true;
    }

    IEnumerator SendPos()
    {
        yield return new WaitForSeconds(0.05f);
        ChangeMyInformation();
        StartCoroutine(SendPos());
    }

    /// <summary>
    /// ���� ����
    /// </summary>
    public void ConnectToTcpServer()
    {
        //ip, port ����
        ip = GameObject.Find("Text_IP").GetComponent<InputField>().text;
        port = int.Parse(GameObject.Find("Text_Port").GetComponent<InputField>().text);

        // TCPŬ���̾�Ʈ ������ ����
        tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingRequeset));
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();
        StartCoroutine(SendPos());
    }

    /// <summary>
    /// TCPŬ���̾�Ʈ ������
    /// </summary>
    private void ListenForIncommingRequeset()
    {
        try
        {
            //����
            socketConnection = new TcpClient(ip, port);
            stream = socketConnection.GetStream();
            clientReady = true;

            //�α� ���
            logList.Add("�ý��� : ���� ����(ip:" + ip + "/port:" + port + ")");

            //������ ���ú� �׽� ���
            while (true)
            {
                //���� ���� ����
                if (!IsConnected(socketConnection))
                {
                    //���� ����
                    DisConnect();
                    break;
                }

                //���� ��
                if (clientReady)
                {
                    //�޽����� ���Դٸ�
                    if (stream.DataAvailable)
                    {
                        //�޽��� ���� ���� �ʱ�ȭ
                        Array.Clear(buffer, 0, buffer.Length);

                        //�޽����� �д´�.
                        int messageLength = stream.Read(buffer, 0, buffer.Length);

                        //���� ó���ϴ� ����
                        byte[] pocessBuffer = new byte[messageLength + nTempByteSize];//���� �о�� �޽����� ���� �޽����� ����� ���ؼ� ó���� ���� ����
                        //���Ҵ� �޽����� �ִٸ�
                        if (isTempByte)
                        {
                            //�� �κп� ���Ҵ� �޽��� ����
                            Array.Copy(tempBuffer, 0, pocessBuffer, 0, nTempByteSize);
                            //���� ���� �޽��� ����
                            Array.Copy(buffer, 0, pocessBuffer, nTempByteSize, messageLength);
                        }
                        else
                        {
                            //���Ҵ� �޽����� ������ ���� �о�� �޽����� ����
                            Array.Copy(buffer, 0, pocessBuffer, 0, messageLength);
                        }

                        //ó���ؾ� �ϴ� �޽����� ���̰� 0�� �ƴ϶��
                        if (nTempByteSize + messageLength > 0)
                        {
                            //���� �޽��� ó��
                            OnIncomingData(pocessBuffer);
                        }
                    }
                    else if (nTempByteSize > 0)
                    {
                        byte[] pocessBuffer = new byte[nTempByteSize];
                        Array.Copy(tempBuffer, 0, pocessBuffer, 0, nTempByteSize);
                        OnIncomingData(pocessBuffer);
                    }
                }
                else//socketReady == false
                {
                    //���� ������
                    break;
                }
            }
        }
        catch (SocketException socketException)
        {
            //�α� ���
            logList.Add("�ý��� : ���� ���� ����(ip:" + ip + "/port:" + port + ")");
            logList.Add(socketException.ToString());

            //Ŭ���̾�Ʈ ���� ����
            clientReady = false;
        }
    }

    /// <summary>
    /// ���� Ȯ��
    /// </summary>
    /// <param name="client"></param>
    /// <returns></returns>
    private bool IsConnected(TcpClient client)
    {
        try
        {
            if (client != null && client.Client != null && client.Client.Connected)
            {
                if (client.Client.Poll(0, SelectMode.SelectRead))
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
    /// <param name="data"></param>
    private void OnIncomingData(byte[] data)
    {

        // �������� ũ�Ⱑ ����� ũ�⺸�ٵ� ������
        if (data.Length < Constants.HEADER_SIZE)
        {
            Array.Copy(data, 0, tempBuffer, nTempByteSize, data.Length);     // ���� ���� ���ۿ� ���� �޽��� ����
            isTempByte = true;
            nTempByteSize += data.Length;
            return;
        }


        //����κ� �߶󳻱�(�����ϱ�)
        byte[] headerDataByte = new byte[Constants.HEADER_SIZE];// ��� ������� 6(ID:ushort + Size:int)
        Array.Copy(data, 0, headerDataByte, 0, headerDataByte.Length);// ��� ������ ��ŭ ������ ����
        //��� ������ ����üȭ(������)
        Common_Data.stHeader headerData = Common_Data.HeaderfromByte(headerDataByte);


        // ����� ������� ���� �޽����� ����� ������
        if (headerData.PacketSize > data.Length)
        {
            Array.Copy(data, 0, tempBuffer, nTempByteSize, data.Length);     // ���� ���� ���ۿ� ���� �޽��� ����
            isTempByte = true;
            nTempByteSize += data.Length;
            return;
        }

        //����� �޽���ũ�⸸ŭ�� �޽��� �����ϱ�
        byte[] msgData = new byte[headerData.PacketSize]; //��Ŷ �и��� ���� ���� ���� ����� ��Ŷ �����ŭ ���� ����
        Array.Copy(data, 0, msgData, 0, headerData.PacketSize); //������ ���ۿ� ��Ŷ ���� ����



        //����� �޽�����
        if (headerData.MsgID == 0)// �� ���� Ȯ�� �޽���
        {
            
            stAllChangeInfoMsg m = AllChangeInfoMsgfromByte(msgData);

            if (tuples.Count > 0)
            {
                tuples[0] = new Tuple<ushort, Vector3, short, float, ushort>
                    (0, new Vector3(m.position0[0], m.position0[1], 0), m.scale0, m.ItemRotationZ0, m.currentAni0);
            }
            if (tuples.Count > 1)
            {
                tuples[1] = new Tuple<ushort, Vector3, short, float, ushort>
                (1, new Vector3(m.position1[0], m.position1[1], 0), m.scale1, m.ItemRotationZ1, m.currentAni1);
            }
            if (tuples.Count > 2)
            {
                tuples[2] = new Tuple<ushort, Vector3, short, float, ushort>
                (2, new Vector3(m.position2[0], m.position2[1], 0), m.scale2, m.ItemRotationZ2, m.currentAni2);
            }
            if (tuples.Count > 3)
            {
                tuples[3] = new Tuple<ushort, Vector3, short, float, ushort>
                (3, new Vector3(m.position3[0], m.position3[1], 0), m.scale3, m.ItemRotationZ3, m.currentAni3);
            }
            
            
        }
        else if (headerData.MsgID == 2)//�޽���
        {
            stSendMsg SendMsgInfo = SendMsgfromByte(msgData);
            //�޽��� �α׿� ���
            logList.Add(headerData.sendClientName + " : " + SendMsgInfo.strSendMsg);
        }
        else if (headerData.MsgID == 3)// Ŭ���̾�Ʈ�� ����
        {
            strRcvMyName = headerData.sendClientName;

            //�޽��� �α׿� ���
            logList.Add(headerData.sendClientName + " : " + "���� �̸��� -> " + headerData.sendClientName);
        }
        else if (headerData.MsgID == 10)
        {
            
            tuples = new List<Tuple<ushort, Vector3, short, float, ushort>>();
            this.integer = SendIntfromByte(data).integer;
            
            
            isGameStart = true;
            isGameEnd = false;
            finishTime = 0;
        }
        else if (headerData.MsgID == 12)
        {
            stSendInt stSend = SendIntfromByte(data);
            getItems[stSend.sendClientName] = stSend.integer;
        }
        else if (headerData.MsgID == 13)
        {
            if (strRcvMyName != headerData.sendClientName)
            {
                stSendUseItem stSend = SendUseItemfromByte(data);
                receive_useitem.Enqueue(stSend);
            }
        }

        else if (headerData.MsgID == 20)
        {

            Debug.Log("ī��Ʈ�ٿ�����");
            if (!isGameEnd)
            {
                isGameEnd = true;
                Debug.Log("ī��Ʈ�ٿ����");
            }
        }
        else if (headerData.MsgID == 21)
        {
            stSendObj SendObjInfo = SendObjfromByte(data);

            receive_changeInfo_Obj.Enqueue(SendObjInfo);
        }
        else if (headerData.MsgID == 22)
        {
            stSendCannon SendCannonInfo = SendCannonfromByte(data);

            receive_changeInfo_Cannon.Enqueue(SendCannonInfo);
        }
        else if (headerData.MsgID == 30)
        {
            stSendRanking sendRanking = SendRankfromByte(data);

            finishTimes = new List<Tuple<ushort, float>>();
            for (int i = 0; i < tuples.Count; i++)
            {
                if (sendRanking.ClientNames[i] == 5) continue;
                finishTimes.Add(new Tuple<ushort, float>(sendRanking.ClientNames[i], sendRanking.time[i]));
            }
            

            //ushort[] str = sendRanking.ClientNames;

            //float[] clientTime = sendRanking.time;

            //for (int i = 0; i < str.Length; i++)
            //{
            //    logList.Add((i+1).ToString() +"�� : "+ str[i].ToString() + " / "+ clientTime[i].ToString());
            //}

            resetbool = true;
        }
        else//�ĺ����� ���� ID
        {

        }

        // ��� �޽����� ó���Ǽ� ���� �޽����� ���� ��� 
        if (data.Length == msgData.Length)
        {
            isTempByte = false;
            nTempByteSize = 0;
        }
        // �޽��� ó�� �� �޽����� �����ִ� ���
        else
        {
            //�ӽ� ���� û��
            Array.Clear(tempBuffer, 0, tempBuffer.Length);

            //������ ���ۿ� ��Ŷ ���� ����
            Array.Copy(data, msgData.Length, tempBuffer, 0, data.Length - (msgData.Length));// �ӽ� ���� ���ۿ� ���� �޽��� ����
            isTempByte = true;
            nTempByteSize += data.Length - (msgData.Length);
        }

    }




    /// <summary>
    /// ���� ����
    /// </summary>
    public void GameRestart()
    {
        resetbool = false;
        reset?.Invoke();

        //loglist
        for (int i = 0; i < tuples.Count; i++)
        {
            tuples[i] = new Tuple<ushort, Vector3, short,float, ushort>(0, Vector3.zero, 0, 0, 0);
        }
        ClientLog.GetComponent<Text>().text = "";

        StopCoroutine(SendPos());
    }

    /// <summary>
    /// �޽��� ����
    /// </summary>
    public void Send()
    {
        //������°� �ƴ� ���
        if (socketConnection == null)
        {
            return;
        }

        // ���� ���� ����ü �ʱ�ȭ
        Common_Data.stSendMsg stSendMsgInfo = new Common_Data.stSendMsg();

        string strSendMsg = Text_Input.GetComponent<InputField>().text;

        stSendMsgInfo.sendClientName = strRcvMyName;
        stSendMsgInfo.MsgID = 2;//�޽��� ID
        stSendMsgInfo.PacketSize = (ushort)Marshal.SizeOf(stSendMsgInfo);//�޽��� ũ��
        stSendMsgInfo.strSendMsg = strSendMsg;

        //����ü ����Ʈȭ �� ����
        SendMsg(Common_Data.GetSendMsgToByte(stSendMsgInfo));
    }

    /// <summary>
    /// �α� ����
    /// </summary>
    /// <param name="message"></param>
    public void WriteLog(/*Time*/string message)
    {
        ClientLog.GetComponent<Text>().text += message + "\n";
    }

    /// <summary>
    /// ���� ����
    /// </summary>
    public void DisConnect()
    {
        //�� �����
        if (socketConnection == null)
        {
            return;
        }

        //�α� ���
        logList.Add("[�ý���] Ŭ���̾�Ʈ ���� ����");

        //���� �ʱ�ȭ
        clientReady = false;

        //stream �ʱ�ȭ
        stream.Close();

        //���� �ʱ�ȭ
        socketConnection.Close();
        socketConnection = null;

        //������ �ʱ�ȭ
        tcpListenerThread.Abort();
        tcpListenerThread = null;
    }

    /// <summary>
    /// ���� �����
    /// </summary>
    private void OnApplicationQuit()
    {
        DisConnect();
    }

    /// <summary>
    /// �� ���� Ȯ��
    /// </summary>
    public void CheckMyInformation()
    {
        //�޽��� �ʱ�ȭ
        sendMessage = new byte[1024];

        // �� ���� Ȯ�� ����ü �ʱ�ȭ
        stHeader stCheckInfoMsgData = new stHeader();

        stCheckInfoMsgData.sendClientName = strRcvMyName;
        stCheckInfoMsgData.MsgID = 0;
        stCheckInfoMsgData.PacketSize = (ushort)Marshal.SizeOf(stCheckInfoMsgData);

        //����ü ����Ʈȭ �� ����
        SendMsg(GetHeaderToByte(stCheckInfoMsgData));
    }

    /// <summary>
    /// ������ ���
    /// </summary>
    public void UseItem(Vector3 vector)
    {
        stSendUseItem i = new stSendUseItem();
        i.sendClientName = strRcvMyName;
        i.PacketSize = (ushort)Marshal.SizeOf(i);
        i.MsgID = 13;
        float[] floatArray = { vector.x, vector.y, 0f};
        i.rotation = floatArray;
        i.useitem = 1;

        byte[] sendmsg = GetSendUseItemToByte(i);

        SendMsg(sendmsg);
    }

    public void GameEnd()
    {
        stSendMsg sendMsg = new stSendMsg();
        sendMsg.sendClientName = strRcvMyName;
        sendMsg.MsgID = 20;
        sendMsg.PacketSize = (ushort)Marshal.SizeOf(sendMsg);
        sendMsg.strSendMsg = finishTime.ToString();

        SendMsg(GetSendMsgToByte(sendMsg));
        isGameStart = false;
        if (!isGameEnd)
        {
            isGameEnd = true;
        }
    }

    /// <summary>
    /// �� ���� ����
    /// </summary>
    public void ChangeMyInformation()
    {
        if (player == null) return;
        //�޽��� �ʱ�ȭ
        sendMessage = new byte[1024];

        // ���� ���� ����ü �ʱ�ȭ
        stChangeInfoMsg stChangeInfoMsgData = new stChangeInfoMsg();

        //������ �� ���� ��������
        float[] positionArray = { player.position.x, player.position.y };

        short sizeX = (short)player.localScale.x;

        //�޽��� �ۼ�
        stChangeInfoMsgData.sendClientName = strRcvMyName;
        stChangeInfoMsgData.MsgID = 1;//�޽��� ID
        stChangeInfoMsgData.PacketSize = (ushort)Marshal.SizeOf(stChangeInfoMsgData);//�޽��� ũ��
        stChangeInfoMsgData.position = positionArray;
        stChangeInfoMsgData.scale = sizeX;
        stChangeInfoMsgData.ItemRotationZ = gun.transform.rotation.eulerAngles.z;
        stChangeInfoMsgData.currentAni = player.GetComponent<JY_Player>().aniNum;

        //����ü ����Ʈȭ �� ����
        SendMsg(GetChangeInfoMsgToByte(stChangeInfoMsgData));
    }

    /// <summary>
    /// �Ű����� �޽��� ������
    /// </summary>
    private void SendMsg(byte[] message)
    {
        //������°� �ƴ� ���
        if (socketConnection == null)
        {
            return;
        }

        //����
        stream.Write(message, 0, message.Length);
        stream.Flush();
    }

    /// <summary>
    /// �߰� ������ ȹ��
    /// </summary>
    public void GainItem()
    {
        //������°� �ƴ� ���
        if (socketConnection == null)
        {
            return;
        }

        stSendInt sendInt = new stSendInt();

        sendInt.sendClientName = strRcvMyName;
        sendInt.integer = (ushort)Random.Range(1, 10);
        sendInt.PacketSize = (ushort)Marshal.SizeOf(sendInt);
        sendInt.MsgID = 12;

        byte[] SendData = GetSendIntToByte(sendInt);

        
        stream.Write(SendData, 0, SendData.Length);
        stream.Flush();
    }

    /// <summary>
    /// �߰� ������Ʈ ����
    /// </summary>
    public void SendClientObjMsg(ushort integer, ushort objIndex)
    {
        stSendObj sendObj = new stSendObj();
        sendObj.MsgID = 21;
        sendObj.PacketSize = (ushort)Marshal.SizeOf(sendObj);
        sendObj.integer = integer;
        sendObj.objIndex = objIndex;

        byte[] sendMsg = GetSendObjToByte(sendObj);
        stream.Write(sendMsg, 0, sendMsg.Length);
        stream.Flush();
    }

    /// <summary>
    /// �߰� Ŭ���̾�Ʈ ���� ����
    /// </summary>
    public void SendClientCannonMsg(bool use, ushort cannonIndex)
    {
        stSendCannon sendObj = new stSendCannon();
        sendObj.MsgID = 22;
        sendObj.PacketSize = (ushort)Marshal.SizeOf(sendObj);
        sendObj.useCannon = use;
        sendObj.CannonIndex = cannonIndex;

        byte[] sendMsg = GetSendCannonToByte(sendObj);
        stream.Write(sendMsg, 0, sendMsg.Length);
        stream.Flush();
    }
}