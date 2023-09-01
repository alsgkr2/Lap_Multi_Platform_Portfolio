using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CommonDataNameSpace
{
    /// <summary>
    /// ��� Ŭ����
    /// </summary>
    static public class Constants
    {
        public const int HEADER_SIZE = 6;//��� ������� 36(ID:ushort + Size:ushort + �̸� : 32)
        public const int MAX_NAME_BYTE = 32;//�̸��� �ִ� ����Ʈ �� : �ѱ�10����, �������32����
        public const int MAX_SEND_MSG_BYTE = 100;//���� �޽����� �ִ� ����Ʈ �� : �ѱ�33����, �������10����
    }
}

public class Common_Data
{
    /// <summary>
    /// ��� ����ü ������
    /// </summary>
    [StructLayout(LayoutKind.Sequential/*�����¼������(Queue)*/, Pack = 1/*�����͸� ���� ����*/)]
    public struct stHeader
    {
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 sendClientName; // Ŭ���̾�Ʈ �̸�
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 MsgID; // �޽��� ID
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 PacketSize; // �޽��� ũ��
    }
    /// <summary>
    /// ��� ����ü ������ �Լ�(Byte->����ü)
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static stHeader HeaderfromByte(byte[] arr)
    {
        //����ü �ʱ�ȭ
        stHeader str = default(stHeader);
        int size = Marshal.SizeOf(str);//����ü Size

        //Size��ŭ �޸� �Ҵ�(�޸� �ڸ� ������)
        IntPtr ptr = Marshal.AllocHGlobal(size);

        //�����͸� �����Ͽ� �޸𸮿� �ֱ�(������ ���ųֱ�)
        Marshal.Copy(arr, 0, ptr, size);

        //����ü�� �ֱ�(���ų��� ������ ���� �ؼ� ����ü�� �ֱ�)
        str = (stHeader)Marshal.PtrToStructure(ptr, str.GetType());
        //�Ҵ��� �޸� ����
        Marshal.FreeHGlobal(ptr);

        //����ü ����S
        return str;
    }
    /// <summary>
    /// ��� ����ü ������ �Լ�(����ü->Byte)
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
    /// �� ���� ���� �޽��� ����ü ������
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stChangeInfoMsg
    {
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 sendClientName; // Ŭ���̾�Ʈ �̸�
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 MsgID; // �޽��� ID
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 PacketSize; // �������κ� �޽��� ũ��

        [MarshalAs(UnmanagedType.ByValArray/*float array*/, SizeConst = 2)]
        public float[] position; // Ŭ���̾�Ʈ ��ġ XYZ��ǥ
        [MarshalAs(UnmanagedType.I2, SizeConst = 2)]
        public Int16 scale;
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public UInt16 currentAni;
        [MarshalAs(UnmanagedType.R4, SizeConst = 4)]
        public float ItemRotationZ;
    }
    /// <summary>
    /// �� ���� ���� �޽��� ����ü ������ �Լ�(Byte->����ü)
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static stChangeInfoMsg ChangeInfoMsgfromByte(byte[] arr)
    {
        //����ü �ʱ�ȭ
        stChangeInfoMsg str = default(stChangeInfoMsg);
        int size = Marshal.SizeOf(str);//����ü Size

        //Size��ŭ �޸� �Ҵ�(�޸� �ڸ� ������)
        IntPtr ptr = Marshal.AllocHGlobal(size);

        //�����͸� �����Ͽ� �޸𸮿� �ֱ�(������ ���ųֱ�)
        Marshal.Copy(arr, 0, ptr, size);

        //����ü�� �ֱ�(���ų��� ������ ���� �ؼ� ����ü�� �ֱ�)
        str = (stChangeInfoMsg)Marshal.PtrToStructure(ptr, str.GetType());
        //�Ҵ��� �޸� ����
        Marshal.FreeHGlobal(ptr);

        //����ü ����
        return str;
    }
    /// <summary>
    /// �� ���� ���� �޽��� ����ü ������ �Լ�(����ü->Byte)
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
        public UInt16 sendClientName; // Ŭ���̾�Ʈ �̸�
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 MsgID; // �޽��� ID
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 PacketSize; // �������κ� �޽��� ũ��

        [MarshalAs(UnmanagedType.ByValArray/*float array*/, SizeConst = 2)]
        public float[] position0; // Ŭ���̾�Ʈ ��ġ XYZ��ǥ
        [MarshalAs(UnmanagedType.I2, SizeConst = 2)]
        public Int16 scale0;
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public UInt16 currentAni0;
        [MarshalAs(UnmanagedType.R4, SizeConst = 4)]
        public float ItemRotationZ0;

        [MarshalAs(UnmanagedType.ByValArray/*float array*/, SizeConst = 2)]
        public float[] position1; // Ŭ���̾�Ʈ ��ġ XYZ��ǥ
        [MarshalAs(UnmanagedType.I2, SizeConst = 2)]
        public Int16 scale1;
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public UInt16 currentAni1;
        [MarshalAs(UnmanagedType.R4, SizeConst = 4)]
        public float ItemRotationZ1;

        [MarshalAs(UnmanagedType.ByValArray/*float array*/, SizeConst = 2)]
        public float[] position2; // Ŭ���̾�Ʈ ��ġ XYZ��ǥ
        [MarshalAs(UnmanagedType.I2, SizeConst = 2)]
        public Int16 scale2;
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public UInt16 currentAni2;
        [MarshalAs(UnmanagedType.R4, SizeConst = 4)]
        public float ItemRotationZ2;

        [MarshalAs(UnmanagedType.ByValArray/*float array*/, SizeConst = 2)]
        public float[] position3; // Ŭ���̾�Ʈ ��ġ XYZ��ǥ
        [MarshalAs(UnmanagedType.I2, SizeConst = 2)]
        public Int16 scale3;
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public UInt16 currentAni3;
        [MarshalAs(UnmanagedType.R4, SizeConst = 4)]
        public float ItemRotationZ3;
    }
    /// <summary>
    /// �� ���� ���� �޽��� ����ü ������ �Լ�(Byte->����ü)
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static stAllChangeInfoMsg AllChangeInfoMsgfromByte(byte[] arr)
    {
        //����ü �ʱ�ȭ
        stAllChangeInfoMsg str = default(stAllChangeInfoMsg);
        int size = Marshal.SizeOf(str);//����ü Size

        //Size��ŭ �޸� �Ҵ�(�޸� �ڸ� ������)
        IntPtr ptr = Marshal.AllocHGlobal(size);

        //�����͸� �����Ͽ� �޸𸮿� �ֱ�(������ ���ųֱ�)
        Marshal.Copy(arr, 0, ptr, size);

        //����ü�� �ֱ�(���ų��� ������ ���� �ؼ� ����ü�� �ֱ�)
        str = (stAllChangeInfoMsg)Marshal.PtrToStructure(ptr, str.GetType());
        //�Ҵ��� �޸� ����
        Marshal.FreeHGlobal(ptr);

        //����ü ����
        return str;
    }
    /// <summary>
    /// �� ���� ���� �޽��� ����ü ������ �Լ�(����ü->Byte)
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
    /// ���� �޽��� ����ü ������
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stSendMsg
    {
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 sendClientName; // Ŭ���̾�Ʈ �̸�
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 MsgID; // �޽��� ID
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 PacketSize; // �޽��� ũ��

        [MarshalAs(UnmanagedType.ByValTStr/*string*/, SizeConst = (int)(CommonDataNameSpace.Constants.MAX_SEND_MSG_BYTE)/*���� �޽����� �ִ� ����Ʈ*/)]
        public string strSendMsg; // ���� �޽���

    }
    /// <summary>
    /// �� ���� ���� �޽��� ����ü ������ �Լ�(Byte->����ü)
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static stSendMsg SendMsgfromByte(byte[] arr)
    {
        //����ü �ʱ�ȭ
        stSendMsg str = default(stSendMsg);
        int size = Marshal.SizeOf(str);//����ü Size

        //Size��ŭ �޸� �Ҵ�(�޸� �ڸ� ������)
        IntPtr ptr = Marshal.AllocHGlobal(size);

        //�����͸� �����Ͽ� �޸𸮿� �ֱ�(������ ���ųֱ�)
        Marshal.Copy(arr, 0, ptr, size);

        //����ü�� �ֱ�(���ų��� ������ ���� �ؼ� ����ü�� �ֱ�)
        str = (stSendMsg)Marshal.PtrToStructure(ptr, str.GetType());
        //�Ҵ��� �޸� ����
        Marshal.FreeHGlobal(ptr);

        //����ü ����
        return str;
    }
    /// <summary>
    /// �� ���� ���� �޽��� ����ü ������ �Լ�(����ü->Byte)
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
    /// ��Ʈ ���� ����ü ������
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stSendInt
    {
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 sendClientName; // Ŭ���̾�Ʈ �̸�
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 MsgID; // �޽��� ID
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 PacketSize; // �޽��� ũ��

        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public UInt16 integer; // ���� �޽���

    }
    /// <summary>
    /// ��Ʈ ���� ����ü ������ �Լ�(Byte->����ü)
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static stSendInt SendIntfromByte(byte[] arr)
    {
        //����ü �ʱ�ȭ
        stSendInt str = default(stSendInt);
        int size = Marshal.SizeOf(str);//����ü Size

        //Size��ŭ �޸� �Ҵ�(�޸� �ڸ� ������)
        IntPtr ptr = Marshal.AllocHGlobal(size);

        //�����͸� �����Ͽ� �޸𸮿� �ֱ�(������ ���ųֱ�)
        Marshal.Copy(arr, 0, ptr, size);

        //����ü�� �ֱ�(���ų��� ������ ���� �ؼ� ����ü�� �ֱ�)
        str = (stSendInt)Marshal.PtrToStructure(ptr, str.GetType());
        //�Ҵ��� �޸� ����
        Marshal.FreeHGlobal(ptr);

        //����ü ����
        return str;
    }

    /// <summary>
    /// ��Ʈ ���� ����ü ������ �Լ�(����ü->Byte)
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
    /// �߰� ������Ʈ ���� ����ü ������
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stSendObj
    {
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 sendClientName; // Ŭ���̾�Ʈ �̸�
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 MsgID; // �޽��� ID
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 PacketSize; // �޽��� ũ��

        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public UInt16 integer; // ���� ������Ʈ(����, ��帧)
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public UInt16 objIndex; // ���� ������Ʈ ���� (���° ����, ��帧)
    }
    /// <summary>
    /// �߰� ������Ʈ ���� ����ü ������ �Լ�(Byte->����ü)
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static stSendObj SendObjfromByte(byte[] arr)
    {
        //����ü �ʱ�ȭ
        stSendObj str = default(stSendObj);
        int size = Marshal.SizeOf(str);//����ü Size

        //Size��ŭ �޸� �Ҵ�(�޸� �ڸ� ������)
        IntPtr ptr = Marshal.AllocHGlobal(size);

        //�����͸� �����Ͽ� �޸𸮿� �ֱ�(������ ���ųֱ�)
        Marshal.Copy(arr, 0, ptr, size);

        //����ü�� �ֱ�(���ų��� ������ ���� �ؼ� ����ü�� �ֱ�)
        str = (stSendObj)Marshal.PtrToStructure(ptr, str.GetType());
        //�Ҵ��� �޸� ����
        Marshal.FreeHGlobal(ptr);

        //����ü ����
        return str;
    }
    /// <summary>
    /// �߰� ������Ʈ ���� ����ü ������ �Լ�(����ü->Byte)
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
    /// �߰� ���� ���� ����ü ������
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stSendCannon
    {
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 sendClientName; // Ŭ���̾�Ʈ �̸�
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 MsgID; // �޽��� ID
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 PacketSize; // �޽��� ũ��

        [MarshalAs(UnmanagedType.I1, SizeConst = 1)]
        public bool useCannon; // ��밡�� ���� 
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public UInt16 CannonIndex; // ���� ������Ʈ ���� (���° ����)
    }
    /// <summary>
    /// �߰� ������Ʈ ���� ����ü ������ �Լ�(Byte->����ü)
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static stSendCannon SendCannonfromByte(byte[] arr)
    {
        //����ü �ʱ�ȭ
        stSendCannon str = default(stSendCannon);
        int size = Marshal.SizeOf(str);//����ü Size

        //Size��ŭ �޸� �Ҵ�(�޸� �ڸ� ������)
        IntPtr ptr = Marshal.AllocHGlobal(size);

        //�����͸� �����Ͽ� �޸𸮿� �ֱ�(������ ���ųֱ�)
        Marshal.Copy(arr, 0, ptr, size);

        //����ü�� �ֱ�(���ų��� ������ ���� �ؼ� ����ü�� �ֱ�)
        str = (stSendCannon)Marshal.PtrToStructure(ptr, str.GetType());
        //�Ҵ��� �޸� ����
        Marshal.FreeHGlobal(ptr);

        //����ü ����
        return str;
    }
    /// <summary>
    /// �߰� ������Ʈ ���� ����ü ������ �Լ�(����ü->Byte)
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
    /// ��ŷ ���� ����ü ������
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stSendRanking
    {
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 sendClientName; // Ŭ���̾�Ʈ �̸�
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 MsgID; // �޽��� ID
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 PacketSize; // �޽��� ũ��

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4/*size*/)]
        public float[] time; // Ŭ���� �ð�
        [MarshalAs(UnmanagedType.ByValArray/*string*/, SizeConst = 4/*Ŭ���̾�Ʈ �̸��� �ִ� ����Ʈ*/)]
        public UInt16[] ClientNames; // Ŭ���̾�
    }

    /// <summary>
    /// ��ŷ ���� ����ü ������ �Լ�(Byte->����ü)
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static stSendRanking SendRankfromByte(byte[] arr)
    {
        //����ü �ʱ�ȭ
        stSendRanking str = default(stSendRanking);
        int size = Marshal.SizeOf(str);//����ü Size

        //Size��ŭ �޸� �Ҵ�(�޸� �ڸ� ������)
        IntPtr ptr = Marshal.AllocHGlobal(size);

        //�����͸� �����Ͽ� �޸𸮿� �ֱ�(������ ���ųֱ�)
        Marshal.Copy(arr, 0, ptr, size);

        //����ü�� �ֱ�(���ų��� ������ ���� �ؼ� ����ü�� �ֱ�)
        str = (stSendRanking)Marshal.PtrToStructure(ptr, str.GetType());
        //�Ҵ��� �޸� ����
        Marshal.FreeHGlobal(ptr);

        //����ü ����
        return str;
    }
    /// <summary>
    /// ��ŷ ���� ����ü ������ �Լ�(����ü->Byte)
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
    /// ������ ��� ���� ����ü ������
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stSendUseItem
    {
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 sendClientName; // Ŭ���̾�Ʈ �̸�
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 MsgID; // �޽��� ID
        [MarshalAs(UnmanagedType.U2/*ushort*/, SizeConst = 2/*ushort size*/)]
        public UInt16 PacketSize; // �޽��� ũ��

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] rotation; // ������ ��� ����  
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public UInt16 useitem; // ������ ��� ����
    }

    /// <summary>
    /// ������ ��� ���� ����ü ������ �Լ�(Byte->����ü)
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static stSendUseItem SendUseItemfromByte(byte[] arr)
    {
        //����ü �ʱ�ȭ
        stSendUseItem str = default(stSendUseItem);
        int size = Marshal.SizeOf(str);//����ü Size

        //Size��ŭ �޸� �Ҵ�(�޸� �ڸ� ������)
        IntPtr ptr = Marshal.AllocHGlobal(size);

        //�����͸� �����Ͽ� �޸𸮿� �ֱ�(������ ���ųֱ�)
        Marshal.Copy(arr, 0, ptr, size);

        //����ü�� �ֱ�(���ų��� ������ ���� �ؼ� ����ü�� �ֱ�)
        str = (stSendUseItem)Marshal.PtrToStructure(ptr, str.GetType());
        //�Ҵ��� �޸� ����
        Marshal.FreeHGlobal(ptr);

        //����ü ����
        return str;
    }

    /// <summary>
    /// ������ ��� ���� ����ü ������ �Լ�(����ü->Byte)
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