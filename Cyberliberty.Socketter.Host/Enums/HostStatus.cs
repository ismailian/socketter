namespace Cyberliberty.Socketter.Helpers
{
    /// <summary>
    /// Indicate the Server current status.
    /// </summary>
    public enum HostStatus
    {
        /// <summary>
        /// Waiting to start.
        /// </summary>
        Waiting,

        /// <summary>
        /// The server is running.
        /// </summary>
        Running,

        /// <summary>
        /// The server is suspended.
        /// </summary>
        Suspended,

        /// <summary>
        /// The server is stopped.
        /// </summary>
        Stopped,
    }
}
