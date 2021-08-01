using Cyberliberty.Socketter.Enums;
using Cyberliberty.Socketter.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cyberliberty.Socketter
{
    public class Guest : IGuest
    {

        #region Private

        #region Private Members

        /// <summary>
        /// 
        /// </summary>
        private TcpClient Client = new TcpClient();

        /// <summary>
        /// 
        /// </summary>
        private GuestStatus Status = GuestStatus.Pending;

        #endregion

        #region Private Methods

        /// <summary>
        /// Keep trying to connect as long as Host is not around yet.
        /// </summary>
        private void TryToConnectWhileHostNotAround()
        {
            new Thread(async () =>
            {
                while (this.Status == GuestStatus.Pending)
                {
                    try
                    {

                        await this.Client.ConnectAsync(this.Hostname, this.Port);
                        if (this.Client.Connected)
                        {
                            /* update status */
                            this.Status = GuestStatus.Connected;

                            /* Start Listening for incoming message */
                            this.ListenForIncomingMessages();

                            /* trigger event */
                            this.OnConnected(null, null);

                            /* stop the loop */
                            break;
                        }

                    }
                    catch (Exception) { }

                    /* just a precaution */
                    await Task.Delay(100);
                }
            }).Start();
        }

        /// <summary>
        /// Listen for incoming messages
        /// </summary>
        private void ListenForIncomingMessages()
        {
            /* start a thread */
            new Thread(async () =>
            {
                while (this.Status == GuestStatus.Connected)
                {

                    /* wait incoming data */
                    List<byte> buffer = new List<byte>();
                    while (this.Client.Client.Available > 0)
                    {
                        var cb = new Byte[1];
                        var ByteCounter = this.Client.Client.Receive(cb, cb.Length, SocketFlags.None);
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
                            this.OnMessageReceived(payload);
                        }
                    }

                    /* delay */
                    await Task.Delay(100);
                }
            }
            ).Start();
        }

        #endregion 

        #endregion

        #region Public

        #region Public Events And Delagates

        #region Events

        /// <summary>
        /// Triggers when start trying to connect to a Host.
        /// </summary>
        public event EventHandler OnStarted;

        /// <summary>
        /// Triggers when connected to a Host.
        /// </summary>
        public event EventHandler OnConnected;

        /// <summary>
        /// Triggers when disconnecting from a Host.
        /// </summary>
        public event EventHandler OnDisconnected;

        /// <summary>
        /// Triggers when an error occurs.
        /// </summary>
        public event EventHandler OnError;

        #endregion

        #region Delagates

        /// <summary>
        /// Triggers when a payload is received.
        /// </summary>
        /// <param name="message"></param>
        public delegate void MessageReceived(string message);
        public MessageReceived OnMessageReceived;

        #endregion

        #endregion

        #region Public Properties

        /// <summary>
        /// 
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Port { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Connect to a Host.
        /// </summary>
        public void Connect()
        {
            /* set the host and port */
            this.Client = new TcpClient();

            /* while not connected, keep trying. */
            this.TryToConnectWhileHostNotAround();

            /* trigger the event */
            this.OnStarted(null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void Send(string message)
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();

            /* on Complete */
            args.Completed += (object sender, SocketAsyncEventArgs e) =>
            {
                /* do something with this info */
            };

            byte[] buffer = Translator.Encrypt(message);
            args.SetBuffer(buffer, 0, buffer.Length);
            this.Client.Client.SendAsync(args);
        }

        /// <summary>
        /// Disconnect from a Host.
        /// </summary>
        public void Disconnect()
        {
            /* disconnect from Host */
            this.Client.Close();

            /* update status */
            this.Status = GuestStatus.Disconnected;

            /* trigger event */
            this.OnDisconnected(null, null);
        }

        #endregion 
        
        #endregion

        #region Constructor
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public Guest()
        {
        } 
        
        #endregion
        
    }
}
