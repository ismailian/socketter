using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cyberliberty.Socketter.Helpers;

namespace Cyberliberty.Socketter
{

    /// <summary>
    /// 
    /// </summary>
    public class Host : IHost
    {

        #region Private

        #region Private Members

        /// <summary>
        /// The TCP Listener.
        /// </summary>
        private TcpListener Server;

        /// <summary>
        /// The collection of sockets connected to the host.
        /// </summary>
        private List<Socket> Sockets;

        /// <summary>
        /// Indicates the server current status.
        /// </summary>
        private HostStatus Status = HostStatus.Waiting;

        /// <summary>
        /// 
        /// </summary>
        private Thread Context;

        /// <summary>
        /// 
        /// </summary>
        private string LastMessage;

        #endregion

        #region Private Methods

        /// <summary>
        /// Keep listening for incoming connections.
        /// </summary>
        private void ListenForIncomingConnections()
        {
            /* Start the threaded loop */
            this.Context = new Thread(async () =>
            {
                while (this.Status == HostStatus.Running)
                {
                    try
                    {

                        /* check for suspension */
                        if (this.Status == HostStatus.Suspended)
                            break;

                        /* check for shutdown. */
                        if (this.Status == HostStatus.Stopped)
                            this.Shutdown();

                        /* Asynciously accept incoming client */
                        Socket socket = await this.Server.AcceptSocketAsync();

                        /* Always interact with socket */
                        this.CheckForSocketStatus(socket);

                        /* always wait for incoming messages */
                        this.CheckForIncomingMessages(socket);

                        /* add to list */
                        this.Sockets.Add(socket);

                        /* trigger event */
                        this.OnClientConnected(socket);

                    }
                    catch (Exception ex)
                    {
                        this.OnErrorOccured(ex.Message);
                    }
                }
            }
            );

            /* start */
            this.Context.Start();
        }

        /// <summary>
        /// Keep checking on the socket.
        /// </summary>
        /// <param name="socket"></param>
        private void CheckForSocketStatus(Socket socket)
        {
            /* start a thread */
            new Thread(async () =>
            {
                while (socket.Connected)
                {
                    try
                    {
                        /* try to send it a zero-byte payload */
                        socket.Send(new byte[] {}, SocketFlags.None);
                    }
                    catch (SocketException ex)
                    {
                        /* check if socket is disconnected */
                        bool hasLeft = Regex.IsMatch(ex.Message, "closed by the remote host");
                        if (hasLeft)
                        {
                            /* remove from list */
                            this.Sockets.Remove(socket);

                            /* trigger event */
                            this.OnClientDisconnected(socket);
                        }
                        else
                        {
                            /* trigger an error event */
                            this.OnErrorOccured(ex.Message);
                        }
                    }

                    /* delay to relieve the CPU asshole */
                    await Task.Delay(100);
                }
            }
            ).Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        private void CheckForIncomingMessages(Socket socket)
        {
            /* start a thread */
            new Thread(async () =>
            {
                while (socket.Connected)
                {

                    /* wait incoming data */
                    List<byte> buffer = new List<byte>();
                    while (socket.Available > 0)
                    {
                        var cb = new Byte[1];
                        var ByteCounter = socket.Receive(cb, cb.Length, SocketFlags.None);
                        if (ByteCounter.Equals(1))
                        {
                            buffer.Add(cb[0]);
                        }
                    }

                    /* trigger the event */
                    if (buffer.ToArray().Length > 0)
                    {
                        string payload = Translator.Decrypt(buffer.ToArray()).Trim();
                        if (!string.IsNullOrEmpty(payload) && payload.Length > 1)
                        {
                            /* set the last message */
                            this.LastMessage = payload;

                            /* trigger event */
                            this.OnMessageReceived(socket, payload);
                        }
                    }

                    /* delay */
                    await Task.Delay(100);
                }
            }
            ).Start();
        }

        /// <summary>
        /// Shuts down the server.
        /// </summary>
        private void Shutdown()
        {
            /* disconnect from all sockets. */
            this.Sockets.ForEach(socket => { socket.Close(); });

            /* terminate the thread */
            if (this.Context.IsAlive)
            {
                this.Context.Abort();
                this.Context.Join();
            }

            /* stop server */
            this.Server.Stop();
        }

        #endregion

        #endregion

        #region Public

        #region Public Events

        /// <summary>
        /// 
        /// </summary>
        public delegate void ServerStarted();
        public ServerStarted OnServerStarted;

        /// <summary>
        /// Triggers when a sockets connects.
        /// </summary>
        /// <param name="socket"></param>
        public delegate void ClientConnected(Socket socket);
        public ClientConnected OnClientConnected;

        /// <summary>
        /// Triggers when a socket disconnects.
        /// </summary>
        /// <param name="socket"></param>
        public delegate void ClientDisconnected(Socket socket);
        public ClientDisconnected OnClientDisconnected;

        /// <summary>
        /// Triggers when a socket sends data.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="message"></param>
        public delegate void MessageReceived(Socket socket, string message);
        public MessageReceived OnMessageReceived;

        /// <summary>
        /// Triggers when a socket error occurs.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="errorMessage"></param>
        public delegate void ErrorOccured(string errorMessage);
        public ErrorOccured OnErrorOccured;

        /// <summary>
        /// A general error.
        /// </summary>
        public event EventHandler<string> OnError;

        /// <summary>
        /// Triggers when a message has been sent.
        /// </summary>
        public event EventHandler<Socket> OnMessageSent;

        #endregion

        #region Public Properties

        /// <summary>
        /// The IP address to listen on
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// The port number to listen on
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// The maximum number of connections to accept.
        /// </summary>
        public int MaximumConnections { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            /* instantiate a new tcp listener. */
            this.Server = new TcpListener(System.Net.IPAddress.Parse(this.IPAddress), this.Port);

            /* Start Listening for incoming connections */
            this.Server.Start(this.MaximumConnections);

            /* update sever status */
            this.Status = HostStatus.Running;

            /* Continiously listen for incoming connections */
            this.ListenForIncomingConnections();

            /* trigger event */
            this.OnServerStarted();
        }

        /// <summary>
        /// Close all connections and stop server.
        /// </summary>
        public void Stop()
        {
            /* update sever status */
            this.Status = HostStatus.Stopped;
        }

        /// <summary>
        /// Suspend server from accepting any incoming connections.
        /// </summary>
        public void Suspend()
        {
            /* update sever status */
            this.Status = HostStatus.Suspended;
        }

        /// <summary>
        /// Start accepting incoming connections.
        /// </summary>
        public void Resume()
        {
            /* call the method */
            this.ListenForIncomingConnections();

            /* update sever status */
            this.Status = HostStatus.Running;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        public Socket Get(int Index)
        {
            return this.Sockets[Index];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        public int GetSocketIndex(Socket socket)
        {
            return this.Sockets.IndexOf(socket);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Index"></param>
        public void Remove(int Index)
        {
            this.Sockets.RemoveAt(Index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetConnectionCount()
        {
            return this.Sockets.Count;
        }

        /// <summary>
        /// Send message to socket.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="message"></param>
        public void Send(Socket socket, string message)
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            
            /* on Complete */
            args.Completed += (object sender, SocketAsyncEventArgs e) =>
            {
                this.OnMessageSent(sender, socket);
            };

            byte[] buffer = Translator.Encrypt(message);
            args.SetBuffer(buffer, 0, buffer.Length);
            socket.SendAsync(args);
        }

        /// <summary>
        /// Get the last message.
        /// </summary>
        /// <returns></returns>
        public string GetLastMessage()
        {
            return this.LastMessage;
        }

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public Host()
        {
            /* instantiate a new list of sockets */
            this.Sockets = new List<Socket>();

            /* set maximum connections accepted by server. */
            this.MaximumConnections = 100;
        }

        #endregion

    }
}
