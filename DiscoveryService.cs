using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class DiscoveryService
{
    private const int DiscoveryPort = 5000;
    private UdpClient _udpClient;
    private string _localIPAddress;
    private string _broadcastAddress;
    private string _userName;
    private Dictionary<string, string> _discoveredUsers = new Dictionary<string, string>();
    private Action<List<string>> _updateUserList;

    // Constructor modificado para aceptar el nombre de usuario personalizado
    public DiscoveryService(Action<List<string>> updateUserListCallback, string customUsername = null)
    {
        _updateUserList = updateUserListCallback;
        _localIPAddress = GetLocalIPAddress();
        _broadcastAddress = GetBroadcastAddress();

        // Usar el nombre personalizado si está definido, de lo contrario usar el del sistema
        _userName = !string.IsNullOrEmpty(customUsername) ? customUsername : Environment.UserName;
    }

    public void StartDiscovery()
    {
        _udpClient = new UdpClient();
        _udpClient.EnableBroadcast = true;
        IPEndPoint broadcastEndPoint = new IPEndPoint(IPAddress.Parse(_broadcastAddress), DiscoveryPort);

        // Hilo para enviar solicitudes de descubrimiento cada 3 segundos
        Thread discoverySenderThread = new Thread(() =>
        {
            while (true)
            {
                // Enviar IP + nombre de usuario
                string broadcastMessage = $"{_localIPAddress}|{_userName}";
                byte[] data = Encoding.UTF8.GetBytes(broadcastMessage);
                _udpClient.Send(data, data.Length, broadcastEndPoint);
                Thread.Sleep(3000);
            }
        });
        discoverySenderThread.IsBackground = true;
        discoverySenderThread.Start();

        // Hilo para recibir respuestas de usuarios en la red
        Thread discoveryReceiverThread = new Thread(() =>
        {
            UdpClient listener = new UdpClient(DiscoveryPort);
            while (true)
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, DiscoveryPort);
                byte[] receivedData = listener.Receive(ref remoteEP);
                string receivedMessage = Encoding.UTF8.GetString(receivedData);

                // Dividir el mensaje recibido en IP y nombre de usuario
                string[] parts = receivedMessage.Split('|');
                if (parts.Length == 2)
                {
                    string receivedIP = parts[0];
                    string receivedUserName = parts[1];

                    if (receivedIP != _localIPAddress && !_discoveredUsers.ContainsValue(receivedIP))
                    {
                        _discoveredUsers[receivedUserName] = receivedIP;
                        _updateUserList.Invoke(new List<string>(_discoveredUsers.Keys));
                    }
                }
            }
        });
        discoveryReceiverThread.IsBackground = true;
        discoveryReceiverThread.Start();
    }

    private string GetLocalIPAddress()
    {
        foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "127.0.0.1";
    }

    private string GetBroadcastAddress()
    {
        string[] ipParts = _localIPAddress.Split('.');
        if (ipParts.Length == 4)
        {
            return $"{ipParts[0]}.{ipParts[1]}.{ipParts[2]}.255";
        }
        return "255.255.255.255";
    }

    public Dictionary<string, string> GetDiscoveredUsers()
    {
        return _discoveredUsers;
    }

    // Método para obtener el nombre de usuario
    public string GetUserName()
    {
        return _userName;
    }
}