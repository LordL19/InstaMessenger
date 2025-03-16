using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InstantMessenger.Services
{
    public class DiscoveryService
    {
        private const int DiscoveryPort = 5000;
        private UdpClient _udpClient;
        private string _localIPAddress;
        private string _broadcastAddress;
        private string _userName;
        private Dictionary<string, string> _discoveredUsers = new Dictionary<string, string>();
        private Action<Dictionary<string, string>> _updateUserList;
        private bool _isRunning;
        private CancellationTokenSource _cts;

        // Constructor modificado para aceptar el nombre de usuario personalizado
        public DiscoveryService(Action<Dictionary<string, string>> updateUserListCallback, string customUsername = null)
        {
            _updateUserList = updateUserListCallback;
            _localIPAddress = GetLocalIPAddress();
            _broadcastAddress = GetBroadcastAddress();
            _cts = new CancellationTokenSource();

            // Usar el nombre personalizado si está definido, de lo contrario usar el del sistema
            _userName = !string.IsNullOrEmpty(customUsername) ? customUsername : Environment.UserName;
        }

        public void StartDiscovery()
        {
            if (_isRunning) return;

            _isRunning = true;
            _udpClient = new UdpClient();
            _udpClient.EnableBroadcast = true;
            IPEndPoint broadcastEndPoint = new IPEndPoint(IPAddress.Parse(_broadcastAddress), DiscoveryPort);

            // Hilo para enviar solicitudes de descubrimiento cada 3 segundos
            Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        // Enviar IP + nombre de usuario
                        string broadcastMessage = $"{_localIPAddress}|{_userName}";
                        byte[] data = Encoding.UTF8.GetBytes(broadcastMessage);
                        _udpClient.Send(data, data.Length, broadcastEndPoint);
                        await Task.Delay(3000, _cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error en el hilo de envío de descubrimiento: {ex.Message}");
                    }
                }
            }, _cts.Token);

            // Hilo para recibir respuestas de usuarios en la red
            Task.Run(() =>
            {
                try
                {
                    UdpClient listener = new UdpClient(DiscoveryPort);

                    while (!_cts.Token.IsCancellationRequested)
                    {
                        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, DiscoveryPort);
                        byte[] receivedData;

                        try
                        {
                            receivedData = listener.Receive(ref remoteEP);
                        }
                        catch (SocketException)
                        {
                            if (_cts.Token.IsCancellationRequested) break;
                            continue;
                        }

                        string receivedMessage = Encoding.UTF8.GetString(receivedData);

                        // Dividir el mensaje recibido en IP y nombre de usuario
                        string[] parts = receivedMessage.Split('|');
                        if (parts.Length == 2)
                        {
                            string receivedIP = parts[0];
                            string receivedUserName = parts[1];

                            if (receivedIP != _localIPAddress)
                            {
                                bool updated = false;

                                lock (_discoveredUsers)
                                {
                                    if (!_discoveredUsers.ContainsKey(receivedUserName) ||
                                        _discoveredUsers[receivedUserName] != receivedIP)
                                    {
                                        _discoveredUsers[receivedUserName] = receivedIP;
                                        updated = true;
                                    }
                                }

                                if (updated)
                                {
                                    Dictionary<string, string> usersCopy;
                                    lock (_discoveredUsers)
                                    {
                                        usersCopy = new Dictionary<string, string>(_discoveredUsers);
                                    }
                                    _updateUserList.Invoke(usersCopy);
                                }
                            }
                        }
                    }

                    listener.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en el hilo de recepción de descubrimiento: {ex.Message}");
                }
            }, _cts.Token);
        }

        public void StopDiscovery()
        {
            if (!_isRunning) return;

            _cts.Cancel();
            _udpClient?.Close();
            _isRunning = false;
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
            lock (_discoveredUsers)
            {
                return new Dictionary<string, string>(_discoveredUsers);
            }
        }

        // Método para obtener el nombre de usuario
        public string GetUserName()
        {
            return _userName;
        }

        // Método para obtener la dirección IP local
        public string GetCurrentIPAddress()
        {
            return _localIPAddress;
        }
    }
}