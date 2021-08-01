using System.Net.Sockets;

namespace Cyberliberty.Socketter.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public interface IHost
    {

        #region Public Properties
        
        /// <summary>
        /// The IP address to listen on
        /// </summary>
        string IPAddress { get; set; }

        /// <summary>
        /// The port number to listen on
        /// </summary>
        int Port { get; set; }

        /// <summary>
        /// The maximum number of connections to accept.
        /// </summary>
        int MaximumConnections { get; set; } 
        
        #endregion

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        void Start();

        /// <summary>
        /// 
        /// </summary>
        void Stop();

        /// <summary>
        /// 
        /// </summary>
        void Suspend();

        /// <summary>
        /// 
        /// </summary>
        void Resume();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        Socket Get(int Index);

        /// <summary>
        /// Get Socket index By Socket.
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        int GetSocketIndex(Socket socket);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Index"></param>
        void Remove(int Index);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int GetConnectionCount();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="message"></param>
        void Send(Socket socket, string message);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        string GetLastMessage();

        #endregion

    }
}
