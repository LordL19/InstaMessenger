using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class ChatService
{
    private const int ChatPort = 5001;
    private TcpListener _tcpListener;
    private Dictionary<string, List<string>> _chatHistory = new Dictionary<string, List<string>>();
    private Action<string> _updateChatBox;

    public ChatService(Action<string> updateChatBoxCallback)
    {
        _updateChatBox = updateChatBoxCallback;
        _tcpListener = new TcpListener(IPAddress.Any, ChatPort); 
        StartChatServer();
    }

    private void StartChatServer()
    {
        Thread serverThread = new Thread(() =>
        {
            _tcpListener.Start();
            while (true)
            {
                TcpClient client = _tcpListener.AcceptTcpClient();
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                string sender = message.Split(':')[0];
                if (!_chatHistory.ContainsKey(sender))
                {
                    _chatHistory[sender] = new List<string>();
                }
                _chatHistory[sender].Add(message);
                _updateChatBox.Invoke(message);
                client.Close();
            }
        });
        serverThread.IsBackground = true;
        serverThread.Start();
    }

    public void SendMessage(string recipientIP, string message, string senderName)
    {
        try
        {
            TcpClient client = new TcpClient(recipientIP, ChatPort);
            NetworkStream stream = client.GetStream();
            byte[] data = Encoding.UTF8.GetBytes(senderName + ": " + message);
            stream.Write(data, 0, data.Length);
            client.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ChatService] Error al enviar mensaje: " + ex.Message);
        }
    }

    public List<string> GetChatHistory(string user)
    {
        return _chatHistory.ContainsKey(user) ? _chatHistory[user] : new List<string>();
    }
}