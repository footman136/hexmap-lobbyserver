using System.Collections;
using System.Text;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using UnityEngine;

/// <summary>
/// 来自于微软
/// https://docs.microsoft.com/zh-cn/dotnet/api/system.net.sockets.socketasynceventargs?view=netframework-4.7.2
/// 
/// Implements the connection logic for the socket server.  
/// After accepting a connection, all data read from the client 
/// is sent back to the client. The read and echo back to the client pattern 
/// is continued until the client disconnects.
/// </summary>
public class MicrosoftServer
{
    private int m_numConnections;   // the maximum number of connections the sample is designed to handle simultaneously 
    private int m_receiveBufferSize;// buffer size to use for each socket I/O operation 
    BufferManager m_bufferManager;  // represents a large reusable set of buffers for all socket operations
    const int opsToPreAlloc = 2;    // read, write (don't alloc buffer space for accepts)
    Socket listenSocket;            // the socket used to listen for incoming connection requests
    // pool of reusable SocketAsyncEventArgs objects for write, read and accept socket operations
    SocketAsyncEventArgsPool m_readPool;
    SocketAsyncEventArgsPool m_writePool;
    int m_totalBytesRead;           // counter of the total # bytes received by the server
    int m_numConnectedSockets;      // the total number of clients connected to the server 
    Semaphore m_maxNumberAcceptedClients;

    // 以下的代码是我加的，提供了外部函数和外部回调函数
    public int ClientCount => m_numConnectedSockets;
    public int MaxClientCount => m_numConnections;
    public int Port { get; private set; }
    public string Address { get; private set; }
    public bool LogEnabled;
    
    //定义接收数据的对象  
    List<byte> m_buffer; // 把这个定义成为链表，我也是服了。。。       
    
    /// <summary>
    /// 接收到消息后的回调函数
    /// </summary>
    public delegate void ReceiveCallBack(SocketAsyncEventArgs s, byte[] content, int offset, int size);
    private ReceiveCallBack receiveCallBack;

    public event Action<SocketAsyncEventArgs, ServerSocketAction> Completed; 
    
    #region 初始化
    // Create an uninitialized server instance.  
    // To start the server listening for connection requests
    // call the Init method followed by Start method 
    //
    // <param name="numConnections">the maximum number of connections the sample is designed to handle simultaneously</param>
    // <param name="receiveBufferSize">buffer size to use for each socket I/O operation</param>
    // <param name="ReceiveCallBack">接收到消息的回调函数</param>
    public MicrosoftServer(int numConnections, int receiveBufferSize, ReceiveCallBack rcb)
    {
        m_totalBytesRead = 0;
        m_numConnectedSockets = 0;
        m_numConnections = numConnections;
        m_receiveBufferSize = receiveBufferSize;
        receiveCallBack = rcb;

        LogEnabled = true;
        // allocate buffers such that the maximum number of sockets can have one outstanding read and 
        //write posted to the socket simultaneously  
        m_bufferManager = new BufferManager(receiveBufferSize * numConnections * opsToPreAlloc,
            receiveBufferSize);
  
        m_readPool = new SocketAsyncEventArgsPool(numConnections);
        m_writePool = new SocketAsyncEventArgsPool(numConnections);
        m_maxNumberAcceptedClients = new Semaphore(numConnections, numConnections);

        m_buffer = new List<Byte>();
    }

    // Initializes the server by preallocating reusable buffers and 
    // context objects.  These objects do not need to be preallocated 
    // or reused, but it is done this way to illustrate how the API can 
    // easily be used to create reusable objects to increase server performance.
    //
    public void Init()
    {
        // Allocates one large byte buffer which all I/O operations use a piece of.  This gaurds 
        // against memory fragmentation
        m_bufferManager.InitBuffer();

        // preallocate pool of SocketAsyncEventArgs objects
        SocketAsyncEventArgs readEventArg;
        SocketAsyncEventArgs writeEventArg;

        for (int i = 0; i < m_numConnections; i++)
        {
            ////////////////
            //Pre-allocate a set of reusable SocketAsyncEventArgs
            readEventArg = new SocketAsyncEventArgs();
            readEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            readEventArg.UserToken = new AsyncUserToken();

            // assign a byte buffer from the buffer pool to the SocketAsyncEventArg object
            m_bufferManager.SetBuffer(readEventArg);

            // add SocketAsyncEventArg to the pool
            m_readPool.Push(readEventArg);
            
            ////////////////
            //Pre-allocate a set of reusable SocketAsyncEventArgs
            writeEventArg = new SocketAsyncEventArgs();
            writeEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            writeEventArg.UserToken = new AsyncUserToken();

            // assign a byte buffer from the buffer pool to the SocketAsyncEventArg object
            m_bufferManager.SetBuffer(writeEventArg);

            // add SocketAsyncEventArg to the pool
            m_writePool.Push(writeEventArg);
        }

    }

    // Starts the server such that it is listening for 
    // incoming connection requests.    
    //
    // <param name="localEndPoint">The endpoint which the server will listening 
    // for connection requests on</param>
    public void Start(IPEndPoint localEndPoint)
    {
        Address = localEndPoint.Address.ToString();
        Port = localEndPoint.Port;
        
        // create the socket which listens for incoming connections
        listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        
        // 使用Ngale算法，把小数据包合并为大数据包一起发送，这个方法也许从某种程度上可以防止一条消息被截断
        // https://docs.microsoft.com/zh-cn/dotnet/api/system.net.sockets.socket.nodelay?redirectedfrom=MSDN&view=netframework-4.8#System_Net_Sockets_Socket_NoDelay
        listenSocket.NoDelay = true;
        
        listenSocket.Bind(localEndPoint);
        // start the server with a listen backlog of 100 connections
        listenSocket.Listen(100);
        
        // post accepts on the listening socket
        StartAccept(null);            

        //Console.WriteLine("{0} connected sockets with one outstanding receive posted to each....press any key", m_outstandingReadCount);
        //Console.WriteLine("Press any key to terminate the server process....");
        //Console.ReadKey();
    }

    public void Stop()
    {
        listenSocket.Close();
        Completed?.Invoke(null, ServerSocketAction.Close);
    }
    
    public void Log(string msg)
    {
        if(LogEnabled)
            Debug.Log(msg);
    }

    #endregion

    #region Accept
    // Begins an operation to accept a connection request from the client 
    //
    // <param name="acceptEventArg">The context object to use when issuing 
    // the accept operation on the server's listening socket</param>
    public void StartAccept(SocketAsyncEventArgs acceptEventArg)
    {
        if (acceptEventArg == null)
        {
            acceptEventArg = new SocketAsyncEventArgs();
            acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
        }
        else
        {
            // socket must be cleared since the context object is being reused
            acceptEventArg.AcceptSocket = null;
        }

        m_maxNumberAcceptedClients.WaitOne();
        bool willRaiseEvent = listenSocket.AcceptAsync(acceptEventArg);
        if (!willRaiseEvent)
        {
            ProcessAccept(acceptEventArg);
        }
    }

    // This method is the callback method associated with Socket.AcceptAsync 
    // operations and is invoked when an accept operation is complete
    //
    void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
    {
        try
        {
            ProcessAccept(e);
        }
        catch (Exception ex)
        {
            Debug.LogError("Exception - AcceptEvent - Completed...");            
        }
    }

    private void ProcessAccept(SocketAsyncEventArgs e)
    {
        Interlocked.Increment(ref m_numConnectedSockets);
        //Console.WriteLine("Client connection accepted. There are {0} clients connected to the server",
        //    m_numConnectedSockets);

        // Get the socket for the accepted client connection and put it into the 
        //ReadEventArg object user token
        SocketAsyncEventArgs readEventArgs = m_readPool.Pop();
        ((AsyncUserToken)readEventArgs.UserToken).Socket = e.AcceptSocket;
        
        Completed?.Invoke(e, ServerSocketAction.Accept); // 触发事件

        // As soon as the client is connected, post a receive to the connection
        bool willRaiseEvent = e.AcceptSocket.ReceiveAsync(readEventArgs);
        if(!willRaiseEvent){
            ProcessReceive(readEventArgs);
        }

        // Accept the next connection request
        StartAccept(e);
    }

    // This method is called whenever a receive or send operation is completed on a socket 
    //
    // <param name="e">SocketAsyncEventArg associated with the completed receive operation</param>
    void IO_Completed(object sender, SocketAsyncEventArgs e)
    {
        // determine which type of operation just completed and call the associated handler
        switch (e.LastOperation)
        {
            case SocketAsyncOperation.Receive:
                ProcessReceive(e);
                break;
            case SocketAsyncOperation.Send:
                ProcessSend(e);
                break;
            default:
                throw new ArgumentException("The last operation completed on the socket was not a receive or send");
        }       

    }
    #endregion
    
    #region 接收
    // This method is invoked when an asynchronous receive operation completes. 
    // If the remote host closed the connection, then the socket is closed.  
    // If data was received then the data is echoed back to the client.
    //
    private void ProcessReceive(SocketAsyncEventArgs e)
    {
        // check if the remote host closed the connection
        AsyncUserToken token = (AsyncUserToken)e.UserToken;
        if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
        {
            //increment the count of the total bytes receive by the server
            Interlocked.Add(ref m_totalBytesRead, e.BytesTransferred);
            //Console.WriteLine("The server has read a total of {0} bytes", m_totalBytesRead);
            
            try
            {
//                // 消息提交外部的回调函数处理
//                Completed?.Invoke(e, ServerSocketAction.Receive);
//                receiveCallBack?.Invoke(e, e.Buffer, e.Offset, e.BytesTransferred);
//                
//                if (!token.Socket.ReceiveAsync(e))//为接收下一段数据，投递接收请求，这个函数有可能同步完成，这时返回false，并且不会引发SocketAsyncEventArgs.Completed事件
//                {
//                    //同步接收时处理接收完成事件
//                    ProcessReceive(e);
//                }

                // 真正的互联网环境下会有消息包被截断的情况，所以发送的时候必须在开始定义4个字节的包长度，目前是测试阶段，暂时不开放。
                //读取数据  
                byte[] data = new byte[e.BytesTransferred];
                Log($"Server Found data received - {e.BytesTransferred} byts");
                Array.Copy(e.Buffer, e.Offset, data, 0, e.BytesTransferred);  
                lock (m_buffer)  
                {  
                    m_buffer.AddRange(data);  
                }  
  
                do  
                {  
                    //注意: 这里是需要和服务器有协议的,我做了个简单的协议,就是一个完整的包是包长(4字节)+包数据,便于处理,当然你可以定义自己需要的;   
                    //判断包的长度,前面4个字节.  
                    byte[] lenBytes = m_buffer.GetRange(0, 4).ToArray();  
                    int packageLen = BitConverter.ToInt32(lenBytes, 0);  
                    if (packageLen <= m_buffer.Count - 4)  
                    {  
                        //包够长时,则提取出来,交给后面的程序去处理  
                        byte[] rev = m_buffer.GetRange(4, packageLen).ToArray();  
                        //从数据池中移除这组数据,为什么要lock,你懂的  
                        lock (m_buffer)  
                        {  
                            m_buffer.RemoveRange(0, packageLen + 4);  
                        }  
                        //将数据包交给前台去处理  
                        Completed?.Invoke(e, ServerSocketAction.Receive);
                        receiveCallBack?.Invoke(e, rev, 0, rev.Length);
                    }  
                    else  
                    {   //长度不够,还得继续接收,需要跳出循环  
                        break;  
                    }  
                } while (m_buffer.Count > 4);  
                //注意:你一定会问,这里为什么要用do-while循环?     
                //如果当服务端发送大数据流的时候,e.BytesTransferred的大小就会比服务端发送过来的完整包要小,    
                //需要分多次接收.所以收到包的时候,先判断包头的大小.够一个完整的包再处理.    
                //如果服务器短时间内发送多个小数据包时, 这里可能会一次性把他们全收了.    
                //这样如果没有一个循环来控制,那么只会处理第一个包,    
                //剩下的包全部留在m_buffer中了,只有等下一个数据包过来后,才会放出一个来.  
                //继续接收  
                if (!token.Socket.ReceiveAsync(e))
                {
                    ProcessReceive(e);
                }
            }
            catch (Exception exp)
            {
                int size = e.BytesTransferred;
                string dataStr = System.Text.Encoding.UTF8.GetString(e.Buffer);
                string errorMsg = $"Server ProcessReceive() Exception - size:{size} - data:{dataStr} - {exp}";
                e.SetBuffer(System.Text.Encoding.UTF8.GetBytes(errorMsg), 0, errorMsg.Length);
                Completed?.Invoke(e, ServerSocketAction.Error);
                throw;
            }
        }
        else
        {
            CloseClientSocket(e);
        }
    }
    #endregion

    #region 发送
    // This method is invoked when an asynchronous send operation completes.  
    // The method issues another receive on the socket to read any additional 
    // data sent from the client
    //
    // <param name="e"></param>
    private void ProcessSend(SocketAsyncEventArgs e)
    {
        if (e.SocketError == SocketError.Success)
        {
            m_writePool.Push(e);
            Completed?.Invoke(e, ServerSocketAction.Send);
        }
        else
        {
            CloseClientSocket(e);
        }
    }

    public void Send(SocketAsyncEventArgs e, byte[] bytes, int size)
    {
        AsyncUserToken token = (AsyncUserToken)e.UserToken;
        SocketAsyncEventArgs writeEventArgs = m_writePool.Pop();
        writeEventArgs.UserToken = token;
        byte[] bytesRealSend = new byte[bytes.Length+4];
        byte[] bytesHeader = System.BitConverter.GetBytes(bytes.Length);
        Array.Copy(bytesHeader, 0, bytesRealSend, 0, 4);
        Array.Copy(bytes, 0, bytesRealSend, 4, bytes.Length);
        writeEventArgs.SetBuffer(bytesRealSend, 0, bytesRealSend.Length);
        //writeEventArgs.SetBuffer(bytes, 0, size);
        bool willRaiseEvent = token.Socket.SendAsync(writeEventArgs);
        if (!willRaiseEvent)
        {
            ProcessSend(e);
        }
    }
    #endregion

    #region 关闭
    
    private void CloseClientSocket(SocketAsyncEventArgs e)
    {
        AsyncUserToken token = e.UserToken as AsyncUserToken;

        // close the socket associated with the client
        try
        {
            //Console.WriteLine("A client has been disconnected from the server. There are {0} clients connected to the server", m_numConnectedSockets);
            Completed?.Invoke(e, ServerSocketAction.Drop);
            
            token.Socket.Shutdown(SocketShutdown.Receive);
            token.Socket.Shutdown(SocketShutdown.Send);
            token.Socket.Close();
        }
        // throws if client process has already closed
        catch (Exception ex) { Log($"Microsoft Server CloseClientSocket - {ex}");}

        // decrement the counter keeping track of the total number of clients connected to the server
        Interlocked.Decrement(ref m_numConnectedSockets);
        
        // Free the SocketAsyncEventArg so they can be reused by another client
        m_readPool.Push(e);
        
        m_maxNumberAcceptedClients.Release();
    }

    public void CloseASocket(SocketAsyncEventArgs e)
    {
        AsyncUserToken token = e.UserToken as AsyncUserToken;
        try
        {
            token.Socket.Close();
        }
        // throws if client process has already closed
        catch (Exception ex) { Log($"Microsoft Server CloseClientSocket - {ex}");}
    }
    #endregion

    class SocketAsyncEventArgsPool
    {
        private Queue<SocketAsyncEventArgs> _queue;

        public SocketAsyncEventArgsPool(int capacity)
        {
            _queue = new Queue<SocketAsyncEventArgs>(capacity);
        }

        public void Push(SocketAsyncEventArgs args)
        {
            _queue.Enqueue(args);
        }

        public SocketAsyncEventArgs Pop()
        {
            return _queue.Dequeue();
        }
    }
    
    class AsyncUserToken
    {
        public Socket Socket;
    }
    
}

/// <summary>
/// 接收socket的行为
/// </summary>
public enum ServerSocketAction
{
    /// <summary>
    /// socket发生连接
    /// </summary>
    Listen = 1,
    /// <summary>
    /// socket发送数据
    /// </summary>
    Send = 2,
    /// <summary>
    /// socket发送数据
    /// </summary>
    Receive = 3,
    /// <summary>
    /// socket关闭
    /// </summary>
    Close = 4,
    /// <summary>
    /// 接受某个客户端的连接
    /// </summary>
    Accept = 5,
    /// <summary>
    /// 断开某个客户端的连接
    /// </summary>
    Drop = 6,
    /// <summary>
    /// 错误
    /// </summary>
    Error = 9,
}
