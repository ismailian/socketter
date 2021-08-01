# socketter
This is a socket management library

## This library will help you make use of sockets without the hustle of creating and handling all the work from zero,
just instantiate the object and use it.

# Instantiating Host

```
Host host = new Host()
{
    IPAddress = "127.0.0.1",
    Port = 5005,
};

/* events */
host.OnServerStarted += () => { };
host.OnErrorOccured += (string error) => { };
host.OnClientConnected += (Socket socket) => { };
host.OnClientDisconnected += (Socket socket) => { };
host.OnMessageReceived += (Socket socket, string message) => { };
/* events */

/* start */
host.Start();
```

# Instantiating Guest

```
Guest guest = new Guest()
{
    Hostname = "127.0.0.1",
    Port = 5005,
};

/* events */
guest.OnStarted += (object sender, EventArgs e) => { };
guest.OnError += (object sender, EventArgs e) => {};
guest.OnConnected += (object sender, EventArgs e) => { };
guest.OnDisconnected += (object sender, EventArgs e) => { };
guest.OnMessageReceived += (string message) => { };
/* events */

/* start */
guest.Connect();
```
