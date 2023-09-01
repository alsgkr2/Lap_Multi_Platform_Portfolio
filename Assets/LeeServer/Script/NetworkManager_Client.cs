using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using UnityEngine.UI;
using System.Runtime.InteropServices;//마샬링을 위한 어셈블리
using CommonDataNameSpace;
using static Common_Data;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

public class NetworkManager_Client : MonoBehaviour
{
    //쓰레드
    private Thread tcpListenerThread;

    //소켓
    private TcpClient socketConnection;
    public NetworkStream stream;

    //상태
    private bool clientReady;

    //ip, port
    public string ip;
    public int port;

    //로그
    public Text ClientLog;
    private List<string> logList;
    //전송 메시지
    public GameObject Text_Input;

    //서버 기능 UI
    public GameObject ButtonServerOpen;
    public GameObject ButtonServerClose;
    public GameObject ServerFunctionUI;

    //받은 데이터 저장공간
    byte[] buffer;
    //받은 데이터가 잘릴 경우를 대비하여 임시버퍼에 저장하여 관리
    byte[] tempBuffer;//임시버퍼
    bool isTempByte;//임시버퍼 유무
    int nTempByteSize;//임시버퍼의 크기

    //보내는 메시지 저장공간
    byte[] sendMessage = new byte[1024];

    [SerializeField] Transform player;
    [SerializeField] GameObject cube;
    [SerializeField] GameObject bullet;
    [SerializeField] GameObject gun;
    public List<Tuple<ushort, Vector3, short, float, ushort>> tuples;
    public ushort strRcvMyName;
    private ushort integer;

    //추가
    Move_Object_Manager manager;
    public List<Tuple<ushort, float>> finishTimes;

    //받은 데이터 처리 공간
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
        
        //로그 초기화
        logList = new List<string>();
        //받은 데이터 저장공간 초기화
        buffer = new byte[1024];
        //임시버퍼 초기화
        tempBuffer = new byte[1024];
        isTempByte = false;
        nTempByteSize = 0;
        //추가
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

        //로그리스트에 쌓였다면
        if (logList.Count > 0)
        {
            //배출
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

        //클라이언트 상태에 따라 서버 버튼 활성화/비활성화
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
    /// 서버 연결
    /// </summary>
    public void ConnectToTcpServer()
    {
        //ip, port 설정
        ip = GameObject.Find("Text_IP").GetComponent<InputField>().text;
        port = int.Parse(GameObject.Find("Text_Port").GetComponent<InputField>().text);

        // TCP클라이언트 스레드 시작
        tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingRequeset));
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();
        StartCoroutine(SendPos());
    }

    /// <summary>
    /// TCP클라이언트 쓰레드
    /// </summary>
    private void ListenForIncommingRequeset()
    {
        try
        {
            //연결
            socketConnection = new TcpClient(ip, port);
            stream = socketConnection.GetStream();
            clientReady = true;

            //로그 기록
            logList.Add("시스템 : 서버 연결(ip:" + ip + "/port:" + port + ")");

            //데이터 리시브 항시 대기
            while (true)
            {
                //연결 끊김 감지
                if (!IsConnected(socketConnection))
                {
                    //연결 해제
                    DisConnect();
                    break;
                }

                //연결 중
                if (clientReady)
                {
                    //메시지가 들어왔다면
                    if (stream.DataAvailable)
                    {
                        //메시지 저장 공간 초기화
                        Array.Clear(buffer, 0, buffer.Length);

                        //메시지를 읽는다.
                        int messageLength = stream.Read(buffer, 0, buffer.Length);

                        //실제 처리하는 버퍼
                        byte[] pocessBuffer = new byte[messageLength + nTempByteSize];//지금 읽어온 메시지에 남은 메시지의 사이즈를 더해서 처리할 버퍼 생성
                        //남았던 메시지가 있다면
                        if (isTempByte)
                        {
                            //앞 부분에 남았던 메시지 복사
                            Array.Copy(tempBuffer, 0, pocessBuffer, 0, nTempByteSize);
                            //지금 읽은 메시지 복사
                            Array.Copy(buffer, 0, pocessBuffer, nTempByteSize, messageLength);
                        }
                        else
                        {
                            //남았던 메시지가 없으면 지금 읽어온 메시지를 저장
                            Array.Copy(buffer, 0, pocessBuffer, 0, messageLength);
                        }

                        //처리해야 하는 메시지의 길이가 0이 아니라면
                        if (nTempByteSize + messageLength > 0)
                        {
                            //받은 메시지 처리
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
                    //연결 해제시
                    break;
                }
            }
        }
        catch (SocketException socketException)
        {
            //로그 기록
            logList.Add("시스템 : 서버 연결 실패(ip:" + ip + "/port:" + port + ")");
            logList.Add(socketException.ToString());

            //클라이언트 연결 실패
            clientReady = false;
        }
    }

    /// <summary>
    /// 접속 확인
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
    /// 받은 메시지 처리
    /// </summary>
    /// <param name="data"></param>
    private void OnIncomingData(byte[] data)
    {

        // 데이터의 크기가 헤더의 크기보다도 작으면
        if (data.Length < Constants.HEADER_SIZE)
        {
            Array.Copy(data, 0, tempBuffer, nTempByteSize, data.Length);     // 임지 저장 버퍼에 지금 메시지 저장
            isTempByte = true;
            nTempByteSize += data.Length;
            return;
        }


        //헤더부분 잘라내기(복사하기)
        byte[] headerDataByte = new byte[Constants.HEADER_SIZE];// 헤더 사이즈는 6(ID:ushort + Size:int)
        Array.Copy(data, 0, headerDataByte, 0, headerDataByte.Length);// 헤더 사이즈 만큼 데이터 복사
        //헤더 데이터 구조체화(마샬링)
        Common_Data.stHeader headerData = Common_Data.HeaderfromByte(headerDataByte);


        // 헤더의 사이즈보다 남은 메시지의 사이즈가 작으면
        if (headerData.PacketSize > data.Length)
        {
            Array.Copy(data, 0, tempBuffer, nTempByteSize, data.Length);     // 임지 저장 버퍼에 지금 메시지 저장
            isTempByte = true;
            nTempByteSize += data.Length;
            return;
        }

        //헤더의 메시지크기만큼만 메시지 복사하기
        byte[] msgData = new byte[headerData.PacketSize]; //패킷 분리를 위한 현재 읽은 헤더의 패킷 사이즈만큼 버퍼 생성
        Array.Copy(data, 0, msgData, 0, headerData.PacketSize); //생성된 버퍼에 패킷 정보 복사



        //헤더의 메시지가
        if (headerData.MsgID == 0)// 내 정보 확인 메시지
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
        else if (headerData.MsgID == 2)//메시지
        {
            stSendMsg SendMsgInfo = SendMsgfromByte(msgData);
            //메시지 로그에 기록
            logList.Add(headerData.sendClientName + " : " + SendMsgInfo.strSendMsg);
        }
        else if (headerData.MsgID == 3)// 클라이언트명 정보
        {
            strRcvMyName = headerData.sendClientName;

            //메시지 로그에 기록
            logList.Add(headerData.sendClientName + " : " + "너의 이름은 -> " + headerData.sendClientName);
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

            Debug.Log("카운트다운진입");
            if (!isGameEnd)
            {
                isGameEnd = true;
                Debug.Log("카운트다운시작");
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
            //    logList.Add((i+1).ToString() +"등 : "+ str[i].ToString() + " / "+ clientTime[i].ToString());
            //}

            resetbool = true;
        }
        else//식별되지 않은 ID
        {

        }

        // 모든 메시지가 처리되서 남은 메시지가 없을 경우 
        if (data.Length == msgData.Length)
        {
            isTempByte = false;
            nTempByteSize = 0;
        }
        // 메시지 처리 후 메시지가 남아있는 경우
        else
        {
            //임시 버퍼 청소
            Array.Clear(tempBuffer, 0, tempBuffer.Length);

            //생성된 버퍼에 패킷 정보 복사
            Array.Copy(data, msgData.Length, tempBuffer, 0, data.Length - (msgData.Length));// 임시 저장 버퍼에 남은 메시지 저장
            isTempByte = true;
            nTempByteSize += data.Length - (msgData.Length);
        }

    }




    /// <summary>
    /// 게임 리셋
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
    /// 메시지 전송
    /// </summary>
    public void Send()
    {
        //연결상태가 아닌 경우
        if (socketConnection == null)
        {
            return;
        }

        // 정보 변경 구조체 초기화
        Common_Data.stSendMsg stSendMsgInfo = new Common_Data.stSendMsg();

        string strSendMsg = Text_Input.GetComponent<InputField>().text;

        stSendMsgInfo.sendClientName = strRcvMyName;
        stSendMsgInfo.MsgID = 2;//메시지 ID
        stSendMsgInfo.PacketSize = (ushort)Marshal.SizeOf(stSendMsgInfo);//메시지 크기
        stSendMsgInfo.strSendMsg = strSendMsg;

        //구조체 바이트화 및 전송
        SendMsg(Common_Data.GetSendMsgToByte(stSendMsgInfo));
    }

    /// <summary>
    /// 로그 전시
    /// </summary>
    /// <param name="message"></param>
    public void WriteLog(/*Time*/string message)
    {
        ClientLog.GetComponent<Text>().text += message + "\n";
    }

    /// <summary>
    /// 연결 해제
    /// </summary>
    public void DisConnect()
    {
        //미 연결시
        if (socketConnection == null)
        {
            return;
        }

        //로그 기록
        logList.Add("[시스템] 클라이언트 연결 해제");

        //상태 초기화
        clientReady = false;

        //stream 초기화
        stream.Close();

        //소켓 초기화
        socketConnection.Close();
        socketConnection = null;

        //쓰레드 초기화
        tcpListenerThread.Abort();
        tcpListenerThread = null;
    }

    /// <summary>
    /// 어플 종료시
    /// </summary>
    private void OnApplicationQuit()
    {
        DisConnect();
    }

    /// <summary>
    /// 내 정보 확인
    /// </summary>
    public void CheckMyInformation()
    {
        //메시지 초기화
        sendMessage = new byte[1024];

        // 내 정보 확인 구조체 초기화
        stHeader stCheckInfoMsgData = new stHeader();

        stCheckInfoMsgData.sendClientName = strRcvMyName;
        stCheckInfoMsgData.MsgID = 0;
        stCheckInfoMsgData.PacketSize = (ushort)Marshal.SizeOf(stCheckInfoMsgData);

        //구조체 바이트화 및 전송
        SendMsg(GetHeaderToByte(stCheckInfoMsgData));
    }

    /// <summary>
    /// 아이템 사용
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
    /// 내 정보 변경
    /// </summary>
    public void ChangeMyInformation()
    {
        if (player == null) return;
        //메시지 초기화
        sendMessage = new byte[1024];

        // 정보 변경 구조체 초기화
        stChangeInfoMsg stChangeInfoMsgData = new stChangeInfoMsg();

        //변경할 내 정보 가져오기
        float[] positionArray = { player.position.x, player.position.y };

        short sizeX = (short)player.localScale.x;

        //메시지 작성
        stChangeInfoMsgData.sendClientName = strRcvMyName;
        stChangeInfoMsgData.MsgID = 1;//메시지 ID
        stChangeInfoMsgData.PacketSize = (ushort)Marshal.SizeOf(stChangeInfoMsgData);//메시지 크기
        stChangeInfoMsgData.position = positionArray;
        stChangeInfoMsgData.scale = sizeX;
        stChangeInfoMsgData.ItemRotationZ = gun.transform.rotation.eulerAngles.z;
        stChangeInfoMsgData.currentAni = player.GetComponent<JY_Player>().aniNum;

        //구조체 바이트화 및 전송
        SendMsg(GetChangeInfoMsgToByte(stChangeInfoMsgData));
    }

    /// <summary>
    /// 매개변수 메시지 보내기
    /// </summary>
    private void SendMsg(byte[] message)
    {
        //연결상태가 아닌 경우
        if (socketConnection == null)
        {
            return;
        }

        //전송
        stream.Write(message, 0, message.Length);
        stream.Flush();
    }

    /// <summary>
    /// 추가 아이템 획득
    /// </summary>
    public void GainItem()
    {
        //연결상태가 아닌 경우
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
    /// 추가 오브젝트 대응
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
    /// 추가 클라이언트 대포 대응
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