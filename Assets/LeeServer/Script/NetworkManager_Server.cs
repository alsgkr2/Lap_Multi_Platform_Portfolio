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
using System.Runtime.InteropServices;//마샬링을 위한 어셈블리
using UnityEngine.UIElements;
using CommonDataNameSpace;
using static Common_Data;

/// <summary>
/// 클라이언트 클래스
/// </summary>
public class ServerClient
{
    public TcpClient clientSocket;//클라이언트 소켓(통신 도구)
    public ushort clientname;//클라이언트 이름
    public Vector3 position;//클라이언트 위치
    public Vector3 scale;// 클라이언트 스케일
    public UInt16 animation; //클라이언트 애니메이션

    public int item; // 추가 클라이언트 아이템

    public NetworkStream stream; // 클라이언트 통신 도구

    public UInt16 UIt; //아이템 사용
    public float[] ultV; //아이템 위치

    public float gun;

    //받은 데이터 저장공간
    public byte[] buffer;
    //받은 데이터가 잘릴 경우를 대비하여 임시버퍼에 저장하여 관리
    public byte[] tempBuffer;//임시버퍼
    public bool isTempByte;//임시버퍼 유무
    public int nTempByteSize;//임시버퍼의 크기

    public ServerClient(TcpClient clientSocket, ushort clientname = 0, Vector3 position = default)
    {
        this.clientSocket = clientSocket;
        this.clientname = clientname;
        this.position = position;
        this.scale = Vector3.one;
        this.stream = clientSocket.GetStream();
        this.animation = 0;
        this.item = 0; // 추가 클라이언트 아이템

        this.ultV = new float[] { 0f, 0f, 0f};

        this.gun = 0;

        //데이터 저장공간 초기화
        this.buffer = new byte[1024];
        //임시버퍼 초기화
        this.tempBuffer = new byte[1024];
        this.isTempByte = false;
        this.nTempByteSize = 0;
    }
}

public class NetworkManager_Server : MonoBehaviour
{
    //쓰레드
    private Thread tcpListenerThread;
    //서버의 소켓
    private TcpListener tcpListener;
    //클라이언트
    //private ServerClient client;

    // 클라이언트 목록
    public Dictionary<ushort,ServerClient> ConnectedClients; // 연결된 클라이언트 목록
    private List<ServerClient> disconnectedClients;  // 연결 해제된 클라이언트 목록

    //아이피, 포트
    public string ip;
    public int port;

    //서버 상태
    public bool serverReady;
    //통신 메시지 읽고 쓰기 도구
    //private NetworkStream stream;

    //로그
    public Text ServerLog;//ui
    private List<string> logList;//data

    //전송 메시지
    public InputField Text_Input;

    //클라이언트 기능 UI
    public GameObject ButtonConnect;
    public GameObject ButtonDisConnect;
    public GameObject ClientFunctionUI;

    //플레이어 프리팹
    [SerializeField] private GameObject cube;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject gun;
    // 결승선 기록, 게임 시작/끝 여부
    public float finishTime;
    public List<Tuple<ushort, float>> finishTimes;
    private bool isGameStart = false;
    private bool isGameEnd = false;

    //받은 데이터 처리 공간
    private Queue<stChangeInfoMsg> receive_changeInfo_MSG = new Queue<stChangeInfoMsg>();
    private Queue<stSendObj> receive_changeInfo_Obj = new Queue<stSendObj>(); // 움직이는 오브젝트 데이터 처리 공간
    private Queue<stSendCannon> receive_changeInfo_Cannon = new Queue<stSendCannon>(); // 대포 전용 데이터 처리 공간
    private Queue<stSendUseItem> receive_useitem = new Queue<stSendUseItem>();


    //보내는 메시지 저장공간
    public Action reset; // 인스턴스 리셋
    int serverItem; // 서버 소지 아이템 변수
    public Goal goal; 

    //추가
    Move_Object_Manager manager; // 움직이는 오브젝트, 대포관리 스크립트

    // Start is called before the first frame update
    void Start()
    {

        //로그 초기화
        logList = new List<string>();


        //클라이언트 목록 초기화
        ConnectedClients = new Dictionary<ushort, ServerClient>();
        disconnectedClients = new List<ServerClient>();

        serverItem = 0; // 서버 아이템 초기화

        // 스크립트 컴포넌트 가져오기
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
        // 움직이는 오브젝트 데이터 처리
        if (receive_changeInfo_Obj.Count > 0)
        {
            stSendObj CreateObjMsg = receive_changeInfo_Obj.Dequeue();

            manager.Drop(CreateObjMsg.objIndex);
        }
        // 대포 데이터 처리
        if (receive_changeInfo_Cannon.Count > 0)
        {
            stSendCannon CreateObjMsg = receive_changeInfo_Cannon.Dequeue();

            manager.usingCannon = CreateObjMsg.useCannon;
            manager.Drop(CreateObjMsg.CannonIndex);
        }

        //받은 데이터가 있는경우(내 정보 확인)
        if (receive_changeInfo_MSG.Count > 0)
        {
            //차례대로 뽑아낸다.
            stChangeInfoMsg CreateObjMsg = receive_changeInfo_MSG.Dequeue();

            if(ConnectedClients.ContainsKey(CreateObjMsg.sendClientName))
            {
                //데이터를 넣는다.
                ConnectedClients[CreateObjMsg.sendClientName].position.x = CreateObjMsg.position[0];
                ConnectedClients[CreateObjMsg.sendClientName].position.y = CreateObjMsg.position[1];
                ConnectedClients[CreateObjMsg.sendClientName].gun = CreateObjMsg.ItemRotationZ;
                ConnectedClients[CreateObjMsg.sendClientName].scale.x = CreateObjMsg.scale;
                ConnectedClients[CreateObjMsg.sendClientName].animation = CreateObjMsg.currentAni;
            }
        }

        // 시간 측정
        if (isGameStart)
        {
            finishTime += Time.deltaTime;
        }

        //로그리스트에 쌓였다면
        if (logList.Count > 0)
        {
            //로그 출력
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

        //서버 상태에 따라 클라이언트 버튼 활성화/비활성화
        if (ButtonConnect != null)
        {
            ButtonConnect.SetActive(!serverReady);
            ButtonDisConnect.SetActive(!serverReady);
            ClientFunctionUI.SetActive(!serverReady);
        }
           
    }

    /// <summary>
    /// 서버 생성 버튼
    /// </summary>
    public void ServerCreate()
    {
        //ip, port 설정
        port = int.Parse(GameObject.Find("Text_Port").GetComponent<InputField>().text);

        // TCP서버 배경 스레드 시작
        tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingRequeset));
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();
    }

    /// <summary>
    /// 서버 쓰레드 시작
    /// </summary>
    private void ListenForIncommingRequeset()
    {
        try
        {
            // 소켓 생성
            tcpListener = new TcpListener(IPAddress.Any/*서버에 접속 가능한 IP*/, port);
            tcpListener.Start();

            // 서버 상태 ON
            serverReady = true;

            // 로그 기록
            logList.Add("[시스템] 서버 생성(port:" + port + ")");

            // 데이터 리시브 항시 대기(Update)
            while (true)
            {
                // 서버를 연적이 없다면
                if(!serverReady)
                    break;

                //연결 시도중인 클라이언트 확인
                if(tcpListener != null && tcpListener.Pending())
                {
                    // 연결된 클라이언트 목록에 저장
                    ushort clientName = (ushort)(ConnectedClients.Count + 1);

                    ConnectedClients.Add(clientName, new ServerClient(tcpListener.AcceptTcpClient(),clientName));


                    BroadCast(clientName + " 접속!");

                    stHeader stHeaderTmp = new stHeader();

                    stHeaderTmp.MsgID = 3;
                    stHeaderTmp.sendClientName = clientName;
                    stHeaderTmp.PacketSize = (ushort)Marshal.SizeOf(stHeaderTmp);//메시지 크기

                    byte[] SendData = GetHeaderToByte(stHeaderTmp);

                    ConnectedClients[clientName].stream.Write(SendData, 0, SendData.Length);
                    ConnectedClients[clientName].stream.Flush();
                }

                //접속된 클라이언트 존재시 상호작용 처리
                foreach(var DicClient in ConnectedClients)
                {
                    ServerClient client = DicClient.Value;

                    if (client != null)
                    {
                        //클라이언트 접속 종료시
                        if (!IsConnected(client.clientSocket))
                        {
                            
                            // 이곳에서 바로 클라이언트를 삭제하면 쓰레드간의 딜레이 차이로 에러가 발생함으로 연결해제된 클라이언트 목록으로 관리
                            //logList.Add("[시스템] 클라이언트 접속 해제");
                            
                            // 연결해제된 클라이언트 목록에 추가
                            disconnectedClients.Add(client);

                            continue;
                        }
                        //클라이언트 메시지 처리
                        else
                        {
                            //메시지가 들어왔다면
                            if (client.stream.DataAvailable)
                            {
                                //메시지 저장 공간 초기화
                                Array.Clear(client.buffer, 0, client.buffer.Length);

                                //메시지를 읽는다.
                                int messageLength = client.stream.Read(client.buffer, 0, client.buffer.Length);

                                //실제 처리하는 버퍼
                                byte[] pocessBuffer = new byte[messageLength + client.nTempByteSize];//지금 읽어온 메시지에 남은 메시지의 사이즈를 더해서 처리할 버퍼 생성
                                                                                              //남았던 메시지가 있다면
                                if (client.isTempByte)
                                {
                                    //앞 부분에 남았던 메시지 복사
                                    Array.Copy(client.tempBuffer, 0, pocessBuffer, 0, client.nTempByteSize);
                                    //지금 읽은 메시지 복사
                                    Array.Copy(client.buffer, 0, pocessBuffer, client.nTempByteSize, messageLength);
                                }
                                else
                                {
                                    //남았던 메시지가 없으면 지금 읽어온 메시지를 저장
                                    Array.Copy(client.buffer, 0, pocessBuffer, 0, messageLength);
                                }

                                //처리해야 하는 메시지의 길이가 0이 아니라면
                                if (client.nTempByteSize + messageLength > 0)
                                {
                                    //받은 메시지 처리
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
                
                //접속 해제된 클라이언트 목록 처리
                for(int i = disconnectedClients.Count-1; i >= 0; i--)
                {
                    //로그기록
                    logList.Add("[시스템]클라이언트 접속 해제");
                    //접속된 클라이언트 목록에서 삭제
                    ConnectedClients.Remove(disconnectedClients[i].clientname);
                    // 처리후 접속해제된 클라이언트 목록에서 삭제
                    disconnectedClients.Remove(disconnectedClients[i]);
                }
                
                //연결된 클라이언트 목록(connectedClients)에 추가가 되어 foreach문을 타게 되지만 내용은 안들어가서 client가 null이 되는 현상이 발생하여 딜레이를 준다
                Thread.Sleep(10);
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("SocketException " + socketException.ToString());
        }
    }

    /// <summary>
    /// 클라이언트 접속 확인
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
    /// 받은 메시지 처리
    /// </summary>
    /// <param name="client"></param>
    /// <param name="data"></param>
    private void OnIncomingData(ServerClient client, byte[] data)
    {

        //BroadCastByte(data);
        //return;

        // 데이터의 크기가 헤더의 크기보다도 작으면
        if (data.Length < Constants.HEADER_SIZE)
        {
            Array.Copy(data, 0, client.tempBuffer, client.nTempByteSize, data.Length);     // 임지 저장 버퍼에 지금 메시지 저장
            client.isTempByte = true;
            client.nTempByteSize += data.Length;
            return;
        }

        //헤더부분 잘라내기(복사하기)
        byte[] headerDataByte = new byte[Constants.HEADER_SIZE];
        Array.Copy(data, 0, headerDataByte, 0, headerDataByte.Length); //헤더 사이즈 만큼 데이터 복사
        //헤더 데이터 구조체화(마샬링)
        stHeader headerData = HeaderfromByte(headerDataByte);

        // 헤더의 사이즈보다 남은 메시지의 사이즈가 작으면
        if (headerData.PacketSize > data.Length)
        {
            Array.Copy(data, 0, client.tempBuffer, client.nTempByteSize, data.Length);     // 임지 저장 버퍼에 지금 메시지 저장
            client.isTempByte = true;
            client.nTempByteSize += data.Length;
            return;
        }

        //헤더의 메시지크기만큼만 메시지 복사하기
        byte[] msgData = new byte[headerData.PacketSize]; //패킷 분리를 위한 현재 읽은 헤더의 패킷 사이즈만큼 버퍼 생성
        Array.Copy(data, 0, msgData, 0, headerData.PacketSize); //생성된 버퍼에 패킷 정보 복사

        //헤더의 메시지가
        if (headerData.MsgID == 0)//내 정보 확인
        {
            //클라이언트의 정보를 클라이언트에게 보낸다.
            if (ConnectedClients.ContainsKey(headerData.sendClientName))
            {
                ServerClient clientInfo = ConnectedClients[headerData.sendClientName];

                stChangeInfoMsg stChangeInfoMsgData = new stChangeInfoMsg();

                float[] positionArray = { clientInfo.position.x, clientInfo.position.y, clientInfo.position.z };
                short scaleArray = (short)clientInfo.scale.x;

                //메시지 작성
                stChangeInfoMsgData.sendClientName = clientInfo.clientname;
                stChangeInfoMsgData.MsgID = 0;//메시지 ID
                stChangeInfoMsgData.PacketSize = (ushort)Marshal.SizeOf(stChangeInfoMsgData);//메시지 크기
                stChangeInfoMsgData.position = positionArray;
                stChangeInfoMsgData.scale = scaleArray;

                byte[] SendData = GetChangeInfoMsgToByte(stChangeInfoMsgData);

                clientInfo.stream.Write(SendData, 0, SendData.Length);
                clientInfo.stream.Flush();
            }
        }
        else if (headerData.MsgID == 1)//내 정보 변경
        {
            stChangeInfoMsg stChangeInfoMsg1 = ChangeInfoMsgfromByte(msgData);
            receive_changeInfo_MSG.Enqueue(stChangeInfoMsg1);

            Debug.Log(client.clientname + " : 내 정보 변경 메시지 수신");
        }
        else if (headerData.MsgID == 2)//메시지
        {
            stSendMsg SendMsgInfo = SendMsgfromByte(msgData);

            BroadCastByte(msgData);
            //메시지 로그에 기록
            logList.Add(client.clientname + " : " + SendMsgInfo.strSendMsg);
        }
        else if (headerData.MsgID == 12)// 추가 아이템 획득
        {
            stSendInt SendItmeInfo = SendIntfromByte(msgData);
            ConnectedClients[SendItmeInfo.sendClientName].item = SendItmeInfo.integer;
            BroadCastByte(msgData);
            Debug.Log(ConnectedClients[SendItmeInfo.sendClientName].item);
        }
        else if (headerData.MsgID == 13)
        {

            //아이템 사용 처리
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
                Debug.Log("server쪽 클라이언트 카운트 다운 시작");
                isGameEnd = true;
                stHeader header = new stHeader();
                header.sendClientName = sendMsg.sendClientName;
                header.MsgID = 20;
                header.PacketSize = (ushort)Marshal.SizeOf(header);

                BroadCastByte(GetHeaderToByte(header));
            }
        }
        else if (headerData.MsgID == 21)// 추가 움직이는 오브젝트 대응
        {
            stSendObj SendItmeInfo = SendObjfromByte(msgData);
            receive_changeInfo_Obj.Enqueue(SendItmeInfo);
            BroadCastByte(msgData);
        }
        else if (headerData.MsgID == 22)// 추가 대포 대응
        {
            stSendCannon SendItmeInfo = SendCannonfromByte(msgData);
            receive_changeInfo_Cannon.Enqueue(SendItmeInfo);
            BroadCastByte(msgData);
        }
        else//식별되지 않은 ID
        {

        }

        // 모든 메시지가 처리되서 남은 메시지가 없을 경우 
        if (data.Length == msgData.Length)
        {
            client.isTempByte = false;
            client.nTempByteSize = 0;
        }
        // 메시지 처리 후 메시지가 남아있는 경우
        else
        {
            //임시 버퍼 청소
            Array.Clear(client.tempBuffer, 0, client.tempBuffer.Length);

            //생성된 버퍼에 패킷 정보 복사
            Array.Copy(data, msgData.Length, client.tempBuffer, 0, data.Length - (msgData.Length));// 임시 저장 버퍼에 남은 메시지 저장
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
    /// 로그 전시
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
        // 정보 변경 구조체 초기화
        stSendMsg stSendMsgInfo = new stSendMsg();

        string strSendMsg = Text_Input.text;

        //메시지 작성
        stSendMsgInfo.sendClientName = 0;
        stSendMsgInfo.MsgID = 2;//메시지 ID
        stSendMsgInfo.PacketSize = (ushort)Marshal.SizeOf(stSendMsgInfo);//메시지 크기
        stSendMsgInfo.strSendMsg = strSendMsg;

        //구조체 바이트화 및 전송
        byte[] SendData = GetSendMsgToByte(stSendMsgInfo);

        bool bCheckSend = false;
        foreach (var client in ConnectedClients)
        {
            client.Value.stream.Write(SendData, 0, SendData.Length);
            client.Value.stream.Flush();
            bCheckSend = true;
        }
        //로그 기록
        if(bCheckSend)
            logList.Add("서버 : " + strSendMsg);
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
    /// 메시지 전송
    /// </summary>
    public void Send(ServerClient client, string message = "")
    {
        //서버가 연상태가 아니라면
        if (!serverReady)
            return;

        //공지가 아닌경우 입력한 텍스트 전송
        if(message == "")
        {
            // 정보 변경 구조체 초기화
            stSendMsg stSendMsgInfo = new stSendMsg();

            string strSendMsg = Text_Input.text;

            //메시지 작성
            stSendMsgInfo.sendClientName = 0;
            stSendMsgInfo.MsgID = 2;//메시지 ID
            stSendMsgInfo.PacketSize = (ushort)Marshal.SizeOf(stSendMsgInfo);//메시지 크기
            stSendMsgInfo.strSendMsg = strSendMsg;

            //구조체 바이트화 및 전송
            byte[] SendData = GetSendMsgToByte(stSendMsgInfo);

            foreach (var i in ConnectedClients)
            {
                i.Value.stream.Write(SendData, 0, SendData.Length);
                i.Value.stream.Flush();
            }

            //로그 기록
            logList.Add("서버 : " + strSendMsg);
        }
        else
        {
            try
            {
                stSendMsg stSendMsgInfo = new stSendMsg();

                stSendMsgInfo.sendClientName = 0;
                stSendMsgInfo.MsgID = 2;//메시지 ID
                stSendMsgInfo.PacketSize = (ushort)Marshal.SizeOf(stSendMsgInfo);//메시지 크기
                stSendMsgInfo.strSendMsg = message;

                //구조체 바이트화 및 전송
                byte[] sendMessageByte = GetSendMsgToByte(stSendMsgInfo);

                //전송
                client.stream.Write(sendMessageByte, 0, sendMessageByte.Length);
                client.stream.Flush();

                //로그 기록
                logList.Add("서버 : " + message);
            }
            catch (Exception e)
            {
                Debug.Log("SendException " + e.ToString());
            }
        }

        
    }   

    /// <summary>
    /// 서버 닫기
    /// </summary>
    public void CloseSocket()
    {
        //서버를 연적이 없다면
        if (!serverReady)
        {
            return;
        }
        else//초기화
        {
            //클라이언트에게 서버 종료 선언
            BroadCast("서버 종료!");

            
            //소켓 종료 및 초기화
            if (tcpListener != null) { tcpListener.Stop(); tcpListener = null; }

            //상태 초기화
            serverReady = false;

            //쓰레드 초기화
            tcpListenerThread.Abort();
            tcpListenerThread = null;

            //연결된 클라이언트 초기화
            foreach (var client in ConnectedClients)
            {
                client.Value.stream = null;
                client.Value.clientSocket.Close();
            }
            ConnectedClients.Clear();
        }
    }

    /// <summary>
    /// 추가 서버아이템 획득
    /// </summary>
    public void GainServerItem()
    {
        //연결상태가 아닌 경우
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
    /// 추가 서버 오브젝트 대응
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
    /// 추가 서버 대포 대응
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
    /// 어플 종료시
    /// </summary>
    private void OnApplicationQuit()
    {
        CloseSocket();
    }
}
