using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InstantMessenger.Services
{
    public class ChatService
    {
        private const int ChatPort = 5001;
        private TcpListener _tcpListener;
        private Dictionary<string, List<string>> _chatHistory = new Dictionary<string, List<string>>();
        private Action<string, string> _messageReceivedCallback;
        private bool _isRunning;
        private CancellationTokenSource _cts;

        public ChatService(Action<string, string> messageReceivedCallback)
        {
            _messageReceivedCallback = messageReceivedCallback;
            _tcpListener = new TcpListener(IPAddress.Any, ChatPort);
            _cts = new CancellationTokenSource();
        }

        public void StartChatServer()
        {
            if (_isRunning) return;

            _isRunning = true;

            Task.Run(() =>
            {
                try
                {
                    _tcpListener.Start();

                    while (!_cts.Token.IsCancellationRequested)
                    {
                        TcpClient client;

                        try
                        {
                            client = _tcpListener.AcceptTcpClient();
                        }
                        catch (SocketException)
                        {
                            if (_cts.Token.IsCancellationRequested) break;
                            continue;
                        }

                        ProcessClientAsync(client);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en el servidor de chat: {ex.Message}");
                }
                finally
                {
                    _tcpListener.Stop();
                }
            }, _cts.Token);
        }

        private async void ProcessClientAsync(TcpClient client)
        {
            using (client)
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, _cts.Token);
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Formato esperado: "Nombre: Mensaje"
                    int separatorIndex = message.IndexOf(':');

                    if (separatorIndex > 0)
                    {
                        string sender = message.Substring(0, separatorIndex).Trim();
                        string content = message.Substring(separatorIndex + 1).Trim();

                        lock (_chatHistory)
                        {
                            if (!_chatHistory.ContainsKey(sender))
                            {
                                _chatHistory[sender] = new List<string>();
                            }
                            _chatHistory[sender].Add(message);
                        }

                        _messageReceivedCallback?.Invoke(sender, content);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error procesando cliente: {ex.Message}");
                }
            }
        }

        public async Task<bool> SendMessageAsync(string recipientIP, string message, string senderName)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    await client.ConnectAsync(recipientIP, ChatPort);

                    NetworkStream stream = client.GetStream();
                    byte[] data = Encoding.UTF8.GetBytes(senderName + ": " + message);
                    await stream.WriteAsync(data, 0, data.Length);

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar mensaje: {ex.Message}");
                return false;
            }
        }

        public List<string> GetChatHistory(string user)
        {
            lock (_chatHistory)
            {
                return _chatHistory.ContainsKey(user)
                    ? new List<string>(_chatHistory[user])
                    : new List<string>();
            }
        }

        public void StopChatServer()
        {
            if (!_isRunning) return;

            _cts.Cancel();
            _tcpListener.Stop();
            _isRunning = false;
        }
    }
}