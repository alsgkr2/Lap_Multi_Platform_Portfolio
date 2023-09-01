using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CommonDataNameSpace
{
    /// <summary>
    /// 상수 클래스
    /// </summary>
    static public class Constants
    {
        public const int HEADER_SIZE = 6;//헤더 사이즈는 36(ID:ushort + Size:ushort + 이름 : 32)
        public const int MAX_NAME_BYTE = 32;//이름의 최대 바이트 수 : 한글10글자, 영어숫자32글자
        public const int MAX_SEND_MSG_BYTE = 100;//전송 메시지의 최대 바이트 수 : 한글33글자, 영어숫자10글자
    }
}

public class Common_Data
{
    /// <summary>
    /// 헤더 구조체 마샬링
    /// </summary>
    [StructLayout(LayoutKind.Sequential/*들어오는순서대로(Queue)*/, Pack = 1/*데이터를 읽을 단위*/)]
    public struct stHeader
    {
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 sendClientName; // 클라이언트 이름
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 MsgID; // 메시지 ID
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 PacketSize; // 메시지 크기
    }
    /// <summary>
    /// 헤더 구조체 마샬링 함수(Byte->구조체)
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static stHeader HeaderfromByte(byte[] arr)
    {
        //구조체 초기화
        stHeader str = default(stHeader);
        int size = Marshal.SizeOf(str);//구조체 Size

        //Size만큼 메모리 할당(메모리 자리 빌리기)
        IntPtr ptr = Marshal.AllocHGlobal(size);

        //데이터를 복사하여 메모리에 넣기(데이터 쑤셔넣기)
        Marshal.Copy(arr, 0, ptr, size);

        //구조체에 넣기(쑤셔넣은 데이터 정리 해서 구조체에 넣기)
        str = (stHeader)Marshal.PtrToStructure(ptr, str.GetType());
        //할당한 메모리 해제
        Marshal.FreeHGlobal(ptr);

        //구조체 리턴S
        return str;
    }
    /// <summary>
    /// 헤더 구조체 마샬링 함수(구조체->Byte)
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static byte[] GetHeaderToByte(stHeader str)
    {
        int size = Marshal.SizeOf(str);
        byte[] arr = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(str, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);

        Marshal.FreeHGlobal(ptr);
        return arr;
    }
    /// <summary>
    /// 내 정보 변경 메시지 구조체 마샬링
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stChangeInfoMsg
    {
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 sendClientName; // 클라이언트 이름
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 MsgID; // 메시지 ID
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 PacketSize; // 나머지부분 메시지 크기

        [MarshalAs(UnmanagedType.ByValArray/*float array*/, SizeConst = 2)]
        public float[] position; // 클라이언트 위치 XYZ좌표
        [MarshalAs(UnmanagedType.I2, SizeConst = 2)]
        public Int16 scale;
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public UInt16 currentAni;
        [MarshalAs(UnmanagedType.R4, SizeConst = 4)]
        public float ItemRotationZ;
    }
    /// <summary>
    /// 내 정보 변경 메시지 구조체 마샬링 함수(Byte->구조체)
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static stChangeInfoMsg ChangeInfoMsgfromByte(byte[] arr)
    {
        //구조체 초기화
        stChangeInfoMsg str = default(stChangeInfoMsg);
        int size = Marshal.SizeOf(str);//구조체 Size

        //Size만큼 메모리 할당(메모리 자리 빌리기)
        IntPtr ptr = Marshal.AllocHGlobal(size);

        //데이터를 복사하여 메모리에 넣기(데이터 쑤셔넣기)
        Marshal.Copy(arr, 0, ptr, size);

        //구조체에 넣기(쑤셔넣은 데이터 정리 해서 구조체에 넣기)
        str = (stChangeInfoMsg)Marshal.PtrToStructure(ptr, str.GetType());
        //할당한 메모리 해제
        Marshal.FreeHGlobal(ptr);

        //구조체 리턴
        return str;
    }
    /// <summary>
    /// 내 정보 변경 메시지 구조체 마샬링 함수(구조체->Byte)
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static byte[] GetChangeInfoMsgToByte(stChangeInfoMsg str)
    {
        int size = Marshal.SizeOf(str);
        byte[] arr = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(str, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);

        Marshal.FreeHGlobal(ptr);
        return arr;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stAllChangeInfoMsg
    {
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 sendClientName; // 클라이언트 이름
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 MsgID; // 메시지 ID
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 PacketSize; // 나머지부분 메시지 크기

        [MarshalAs(UnmanagedType.ByValArray/*float array*/, SizeConst = 2)]
        public float[] position0; // 클라이언트 위치 XYZ좌표
        [MarshalAs(UnmanagedType.I2, SizeConst = 2)]
        public Int16 scale0;
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public UInt16 currentAni0;
        [MarshalAs(UnmanagedType.R4, SizeConst = 4)]
        public float ItemRotationZ0;

        [MarshalAs(UnmanagedType.ByValArray/*float array*/, SizeConst = 2)]
        public float[] position1; // 클라이언트 위치 XYZ좌표
        [MarshalAs(UnmanagedType.I2, SizeConst = 2)]
        public Int16 scale1;
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public UInt16 currentAni1;
        [MarshalAs(UnmanagedType.R4, SizeConst = 4)]
        public float ItemRotationZ1;

        [MarshalAs(UnmanagedType.ByValArray/*float array*/, SizeConst = 2)]
        public float[] position2; // 클라이언트 위치 XYZ좌표
        [MarshalAs(UnmanagedType.I2, SizeConst = 2)]
        public Int16 scale2;
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public UInt16 currentAni2;
        [MarshalAs(UnmanagedType.R4, SizeConst = 4)]
        public float ItemRotationZ2;

        [MarshalAs(UnmanagedType.ByValArray/*float array*/, SizeConst = 2)]
        public float[] position3; // 클라이언트 위치 XYZ좌표
        [MarshalAs(UnmanagedType.I2, SizeConst = 2)]
        public Int16 scale3;
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public UInt16 currentAni3;
        [MarshalAs(UnmanagedType.R4, SizeConst = 4)]
        public float ItemRotationZ3;
    }
    /// <summary>
    /// 내 정보 변경 메시지 구조체 마샬링 함수(Byte->구조체)
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static stAllChangeInfoMsg AllChangeInfoMsgfromByte(byte[] arr)
    {
        //구조체 초기화
        stAllChangeInfoMsg str = default(stAllChangeInfoMsg);
        int size = Marshal.SizeOf(str);//구조체 Size

        //Size만큼 메모리 할당(메모리 자리 빌리기)
        IntPtr ptr = Marshal.AllocHGlobal(size);

        //데이터를 복사하여 메모리에 넣기(데이터 쑤셔넣기)
        Marshal.Copy(arr, 0, ptr, size);

        //구조체에 넣기(쑤셔넣은 데이터 정리 해서 구조체에 넣기)
        str = (stAllChangeInfoMsg)Marshal.PtrToStructure(ptr, str.GetType());
        //할당한 메모리 해제
        Marshal.FreeHGlobal(ptr);

        //구조체 리턴
        return str;
    }
    /// <summary>
    /// 내 정보 변경 메시지 구조체 마샬링 함수(구조체->Byte)
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static byte[] GetAllChangeInfoMsgToByte(stAllChangeInfoMsg str)
    {
        int size = Marshal.SizeOf(str);
        byte[] arr = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(str, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);

        Marshal.FreeHGlobal(ptr);
        return arr;
    }

    /// <summary>
    /// 전송 메시지 구조체 마샬링
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stSendMsg
    {
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 sendClientName; // 클라이언트 이름
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 MsgID; // 메시지 ID
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 PacketSize; // 메시지 크기

        [MarshalAs(UnmanagedType.ByValTStr/*string*/, SizeConst = (int)(CommonDataNameSpace.Constants.MAX_SEND_MSG_BYTE)/*전송 메시지의 최대 바이트*/)]
        public string strSendMsg; // 전송 메시지

    }
    /// <summary>
    /// 내 정보 변경 메시지 구조체 마샬링 함수(Byte->구조체)
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static stSendMsg SendMsgfromByte(byte[] arr)
    {
        //구조체 초기화
        stSendMsg str = default(stSendMsg);
        int size = Marshal.SizeOf(str);//구조체 Size

        //Size만큼 메모리 할당(메모리 자리 빌리기)
        IntPtr ptr = Marshal.AllocHGlobal(size);

        //데이터를 복사하여 메모리에 넣기(데이터 쑤셔넣기)
        Marshal.Copy(arr, 0, ptr, size);

        //구조체에 넣기(쑤셔넣은 데이터 정리 해서 구조체에 넣기)
        str = (stSendMsg)Marshal.PtrToStructure(ptr, str.GetType());
        //할당한 메모리 해제
        Marshal.FreeHGlobal(ptr);

        //구조체 리턴
        return str;
    }
    /// <summary>
    /// 내 정보 변경 메시지 구조체 마샬링 함수(구조체->Byte)
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static byte[] GetSendMsgToByte(stSendMsg str)
    {
        int size = Marshal.SizeOf(str);
        byte[] arr = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(str, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);

        Marshal.FreeHGlobal(ptr);
        return arr;
    }
    
    /// <summary>
    /// 인트 전송 구조체 마샬링
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stSendInt
    {
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 sendClientName; // 클라이언트 이름
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 MsgID; // 메시지 ID
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 PacketSize; // 메시지 크기

        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public UInt16 integer; // 전송 메시지

    }
    /// <summary>
    /// 인트 전송 구조체 마샬링 함수(Byte->구조체)
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static stSendInt SendIntfromByte(byte[] arr)
    {
        //구조체 초기화
        stSendInt str = default(stSendInt);
        int size = Marshal.SizeOf(str);//구조체 Size

        //Size만큼 메모리 할당(메모리 자리 빌리기)
        IntPtr ptr = Marshal.AllocHGlobal(size);

        //데이터를 복사하여 메모리에 넣기(데이터 쑤셔넣기)
        Marshal.Copy(arr, 0, ptr, size);

        //구조체에 넣기(쑤셔넣은 데이터 정리 해서 구조체에 넣기)
        str = (stSendInt)Marshal.PtrToStructure(ptr, str.GetType());
        //할당한 메모리 해제
        Marshal.FreeHGlobal(ptr);

        //구조체 리턴
        return str;
    }

    /// <summary>
    /// 인트 전송 구조체 마샬링 함수(구조체->Byte)
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static byte[] GetSendIntToByte(stSendInt str)
    {
        int size = Marshal.SizeOf(str);
        byte[] arr = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(str, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);

        Marshal.FreeHGlobal(ptr);
        return arr;
    }

    /// <summary>
    /// 추가 오브젝트 전송 구조체 마샬링
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stSendObj
    {
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 sendClientName; // 클라이언트 이름
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 MsgID; // 메시지 ID
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 PacketSize; // 메시지 크기

        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public UInt16 integer; // 전송 오브젝트(발판, 고드름)
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public UInt16 objIndex; // 전송 오브젝트 구분 (몇번째 발판, 고드름)
    }
    /// <summary>
    /// 추가 오브젝트 전송 구조체 마샬링 함수(Byte->구조체)
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static stSendObj SendObjfromByte(byte[] arr)
    {
        //구조체 초기화
        stSendObj str = default(stSendObj);
        int size = Marshal.SizeOf(str);//구조체 Size

        //Size만큼 메모리 할당(메모리 자리 빌리기)
        IntPtr ptr = Marshal.AllocHGlobal(size);

        //데이터를 복사하여 메모리에 넣기(데이터 쑤셔넣기)
        Marshal.Copy(arr, 0, ptr, size);

        //구조체에 넣기(쑤셔넣은 데이터 정리 해서 구조체에 넣기)
        str = (stSendObj)Marshal.PtrToStructure(ptr, str.GetType());
        //할당한 메모리 해제
        Marshal.FreeHGlobal(ptr);

        //구조체 리턴
        return str;
    }
    /// <summary>
    /// 추가 오브젝트 전송 구조체 마샬링 함수(구조체->Byte)
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static byte[] GetSendObjToByte(stSendObj str)
    {
        int size = Marshal.SizeOf(str);
        byte[] arr = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(str, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);

        Marshal.FreeHGlobal(ptr);
        return arr;
    }

    /// <summary>
    /// 추가 대포 전송 구조체 마샬링
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stSendCannon
    {
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 sendClientName; // 클라이언트 이름
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 MsgID; // 메시지 ID
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 PacketSize; // 메시지 크기

        [MarshalAs(UnmanagedType.I1, SizeConst = 1)]
        public bool useCannon; // 사용가능 여부 
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public UInt16 CannonIndex; // 전송 오브젝트 구분 (몇번째 대포)
    }
    /// <summary>
    /// 추가 오브젝트 전송 구조체 마샬링 함수(Byte->구조체)
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static stSendCannon SendCannonfromByte(byte[] arr)
    {
        //구조체 초기화
        stSendCannon str = default(stSendCannon);
        int size = Marshal.SizeOf(str);//구조체 Size

        //Size만큼 메모리 할당(메모리 자리 빌리기)
        IntPtr ptr = Marshal.AllocHGlobal(size);

        //데이터를 복사하여 메모리에 넣기(데이터 쑤셔넣기)
        Marshal.Copy(arr, 0, ptr, size);

        //구조체에 넣기(쑤셔넣은 데이터 정리 해서 구조체에 넣기)
        str = (stSendCannon)Marshal.PtrToStructure(ptr, str.GetType());
        //할당한 메모리 해제
        Marshal.FreeHGlobal(ptr);

        //구조체 리턴
        return str;
    }
    /// <summary>
    /// 추가 오브젝트 전송 구조체 마샬링 함수(구조체->Byte)
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static byte[] GetSendCannonToByte(stSendCannon str)
    {
        int size = Marshal.SizeOf(str);
        byte[] arr = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(str, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);

        Marshal.FreeHGlobal(ptr);
        return arr;
    }

    /// <summary>
    /// 랭킹 전송 구조체 마샬링
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stSendRanking
    {
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 sendClientName; // 클라이언트 이름
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 MsgID; // 메시지 ID
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 PacketSize; // 메시지 크기

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4/*size*/)]
        public float[] time; // 클리어 시간
        [MarshalAs(UnmanagedType.ByValArray/*string*/, SizeConst = 4/*클라이언트 이름의 최대 바이트*/)]
        public UInt16[] ClientNames; // 클라이언
    }

    /// <summary>
    /// 랭킹 전송 구조체 마샬링 함수(Byte->구조체)
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static stSendRanking SendRankfromByte(byte[] arr)
    {
        //구조체 초기화
        stSendRanking str = default(stSendRanking);
        int size = Marshal.SizeOf(str);//구조체 Size

        //Size만큼 메모리 할당(메모리 자리 빌리기)
        IntPtr ptr = Marshal.AllocHGlobal(size);

        //데이터를 복사하여 메모리에 넣기(데이터 쑤셔넣기)
        Marshal.Copy(arr, 0, ptr, size);

        //구조체에 넣기(쑤셔넣은 데이터 정리 해서 구조체에 넣기)
        str = (stSendRanking)Marshal.PtrToStructure(ptr, str.GetType());
        //할당한 메모리 해제
        Marshal.FreeHGlobal(ptr);

        //구조체 리턴
        return str;
    }
    /// <summary>
    /// 랭킹 전송 구조체 마샬링 함수(구조체->Byte)
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static byte[] GetSendRankToByte(stSendRanking str)
    {
        int size = Marshal.SizeOf(str);
        byte[] arr = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(str, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);

        Marshal.FreeHGlobal(ptr);
        return arr;
    }

    /// <summary>
    /// 아이템 사용 전송 구조체 마샬링
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stSendUseItem
    {
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 sendClientName; // 클라이언트 이름
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 MsgID; // 메시지 ID
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 PacketSize; // 메시지 크기

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] rotation; // 아이템 사용 방향  
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public UInt16 useitem; // 아이템 사용 여부
    }

    /// <summary>
    /// 아이템 사용 전송 구조체 마샬링 함수(Byte->구조체)
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static stSendUseItem SendUseItemfromByte(byte[] arr)
    {
        //구조체 초기화
        stSendUseItem str = default(stSendUseItem);
        int size = Marshal.SizeOf(str);//구조체 Size

        //Size만큼 메모리 할당(메모리 자리 빌리기)
        IntPtr ptr = Marshal.AllocHGlobal(size);

        //데이터를 복사하여 메모리에 넣기(데이터 쑤셔넣기)
        Marshal.Copy(arr, 0, ptr, size);

        //구조체에 넣기(쑤셔넣은 데이터 정리 해서 구조체에 넣기)
        str = (stSendUseItem)Marshal.PtrToStructure(ptr, str.GetType());
        //할당한 메모리 해제
        Marshal.FreeHGlobal(ptr);

        //구조체 리턴
        return str;
    }

    /// <summary>
    /// 아이템 사용 전송 구조체 마샬링 함수(구조체->Byte)
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static byte[] GetSendUseItemToByte(stSendUseItem str)
    {
        int size = Marshal.SizeOf(str);
        byte[] arr = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(str, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);

        Marshal.FreeHGlobal(ptr);
        return arr;
    }
}