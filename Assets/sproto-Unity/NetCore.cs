using System;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;
using Sproto;
using SprotoType;
using UnityEngine;

public static class NetCore
{
    private static Socket socket;

    private static int CONNECT_TIMEOUT = 3000;

    private static Queue<byte[]> recvQueue = new Queue<byte[]>();
    private static Queue<Action> cbs = new Queue<Action>();
    private static SprotoPack sendPack = new SprotoPack();
    private static SprotoPack recvPack = new SprotoPack();

    private static SprotoStream sendStream = new SprotoStream();
    private static SprotoStream recvStream = new SprotoStream();

    private static ProtocolFunctionDictionary protocol = Protocol.Instance.Protocol;
    private static Dictionary<long, ProtocolFunctionDictionary.typeFunc> sessionDict;

    private static AsyncCallback connectCallback = new AsyncCallback(Connected);
    private static AsyncCallback receiveCallback = new AsyncCallback(Receive);

    public static void Init()
    {
        byte[] receiveBuffer = new byte[1 << 16];
        recvStream.Write(receiveBuffer, 0, receiveBuffer.Length);
        recvStream.Seek(0, SeekOrigin.Begin);

        sessionDict = new Dictionary<long, ProtocolFunctionDictionary.typeFunc>();
    }

    public static void Connect(string host, int port, Action connected, Action unconnected)
    {
        var t = new Thread(() =>
        {
            Disconnect();
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(host, port);
                lock (cbs)
                {
                    cbs.Enqueue(connected);
                }
            }
            catch (SocketException ex)
            {
                Disconnect();
                lock (cbs)
                {
                    cbs.Enqueue(() =>
                    {
                        _.Log("socket error code:", ex.ErrorCode);
                    });
                    cbs.Enqueue(unconnected);
                }
            }

        });
        t.IsBackground = true;
        t.Start();
    }

    private static void Connected(IAsyncResult ar)
    {
        socket.EndConnect(ar);
    }

    public static void Disconnect()
    {
        if (connected)
        {
            socket.Close();
        }
    }

    public static bool connected
    {
        get
        {
            return socket != null && (socket.Connected);
        }
    }

    public static bool IsConnected()
    {
        if (socket == null) return false;
        try
        {
            return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
        }
        catch (Exception) { return false; }
    }

    public static void Send<T>(SprotoTypeBase rpc = null, long? session = null)
    {
        Send(rpc, session, protocol[typeof(T)]);
    }

    private static int MAX_PACK_LEN = (1 << 16) - 1;
    private static void Send(SprotoTypeBase rpc, long? session, int tag)
    {
        if (!connected)
        {
            return;
        }

        Package pkg = new Package();
        pkg.type = tag;

        if (session != null)
        {
            pkg.session = (long)session;
            sessionDict.Add((long)session, protocol[tag].Response.Value);
        }

        sendStream.Seek(0, SeekOrigin.Begin);
        int len = pkg.encode(sendStream);
        if (rpc != null)
        {
            len += rpc.encode(sendStream);
        }

        byte[] data = sendPack.pack(sendStream.Buffer, len);
        if (data.Length > MAX_PACK_LEN)
        {
            Debug.Log("data.Length > " + MAX_PACK_LEN + " => " + data.Length);
            return;
        }

        sendStream.Seek(0, SeekOrigin.Begin);
        sendStream.WriteByte((byte)(data.Length >> 8));
        sendStream.WriteByte((byte)data.Length);
        sendStream.Write(data, 0, data.Length);

        try {
            socket.Send(sendStream.Buffer, sendStream.Position, SocketFlags.None);
        }
        catch (Exception e) {
            Debug.LogWarning(e.ToString());
        }
    }

    private static int receivePosition = 0;
    public static void Receive(IAsyncResult ar = null)
    {
        if (!connected)
        {
            return;
        }
        if (socket.Available > 0)
        {
            _.Log("socket.Available:" + socket.Available);
        }
        if (ar != null)
        {
            try {
                int len = socket.EndReceive(ar);
                //if(len > 0)_.Log("len:"+len);
                receivePosition += len;
                //if (len > 0) _.Log("receivePosition:" + receivePosition);
            }
            catch (Exception e) {
                Debug.LogWarning(e.ToString());
            }
        }

        int i = recvStream.Position;
        while (receivePosition >= i + 2)
        {
            int length = (recvStream[i] << 8) | recvStream[i+1];
            _.Log("length:" + length);
            int sz = length + 2;
            if (receivePosition < i + sz)
            {
                break;
            }

            recvStream.Seek(2, SeekOrigin.Current);

            if (length > 0)
            {
                byte[] data = new byte[length];
                recvStream.Read(data, 0, length);
                recvQueue.Enqueue(data);
            }

            i += sz;
        }

        if (receivePosition == recvStream.Buffer.Length)
        {
            recvStream.Seek(0, SeekOrigin.End);
            recvStream.MoveUp(i, i);
            receivePosition = recvStream.Position;
            recvStream.Seek(0, SeekOrigin.Begin);
        }

        try {
            socket.BeginReceive(recvStream.Buffer, receivePosition,
                recvStream.Buffer.Length - receivePosition,
                SocketFlags.None, receiveCallback, socket);
        }
        catch (Exception e) {
            Debug.LogWarning(e.ToString());
        }
    }

    public static void Recv()
    {
        if (!connected)
        {
            return;
        }

        try
        {
            if (socket.Available == 0) return;
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.ToString());
            return;
        }
        //_.Log("socket.Available:" + socket.Available);
        try
        {
            int rlen = 0;
            rlen = socket.Receive(recvStream.Buffer, receivePosition, socket.Available, SocketFlags.None);
            receivePosition += rlen;

        }
        catch (Exception e)
        {
            Debug.LogWarning(e.ToString());
        }
       // _.Log("receivePosition:" + receivePosition);
        int i = recvStream.Position;
        while (receivePosition >= i + 2)
        {
            int length = (recvStream[i] << 8) | recvStream[i + 1];
            //_.Log("length:" + length);
            int sz = length + 2;
            if (receivePosition < i + sz)
            {
                break;
            }

            recvStream.Seek(2, SeekOrigin.Current);

            if (length > 0)
            {
                byte[] data = new byte[length];
                recvStream.Read(data, 0, length);
                recvQueue.Enqueue(data);
                //_.Log("recvQueue++");
            }

            i += sz;
        }

        if (receivePosition == recvStream.Buffer.Length)
        {
            recvStream.Seek(0, SeekOrigin.End);
            recvStream.MoveUp(i, i);
            receivePosition = recvStream.Position;
            recvStream.Seek(0, SeekOrigin.Begin);
        }


    }
    public static void Dispatch()
    {
        Recv();

        if (cbs.Count > 0)
        {
            Action cb = cbs.Dequeue();
            cb();
        }

        if (recvQueue.Count == 0)
        {
            return;
        }

        Package pkg = new Package();

        if (recvQueue.Count > 20)
        {
            Debug.Log("recvQueue.Count: " + recvQueue.Count);
        }

        while (recvQueue.Count > 0)
        {
            byte[] data = recvPack.unpack(recvQueue.Dequeue());
            int offset = pkg.init(data);

            int tag = (int)pkg.type;
            long session = (long)pkg.session;

            if (pkg.HasType)
            {
                RpcReqHandler rpcReqHandler = NetReceiver.GetHandler(tag);
                if (rpcReqHandler != null)
                {
                    SprotoTypeBase rpcRsp = rpcReqHandler(protocol.GenRequest(tag, data, offset));
                    if (pkg.HasSession)
                    {
                        Send(rpcRsp, session, tag);
                    }
                }
            }
            else
            {
                RpcRspHandler rpcRspHandler = NetSender.GetHandler(session);
                if (rpcRspHandler != null)
                {
                    ProtocolFunctionDictionary.typeFunc GenResponse;
                    sessionDict.TryGetValue(session, out GenResponse);
                    rpcRspHandler(GenResponse(data, offset));
                }
            }
        }
    }

}
