using System.Net.Sockets;

namespace Cyberliberty.Socketter
{
    /// <summary>
    /// 
    /// </summary>
    public interface IGuest
    {

        #region Public Properties
        
        /// <summary>
        /// The Hostname to connect to.
        /// </summary>
        string Hostname { get; set; }

        /// <summary>
        /// The port number to connect to.
        /// </summary>
        int Port { get; set; }
        
        #endregion

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        void Connect();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        void Send(string message);

        /// <summary>
        /// 
        /// </summary>
        void Disconnect();
        
        #endregion

    }
}
