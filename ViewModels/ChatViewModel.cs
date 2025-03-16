using InstantMessenger.Helpers;
using InstantMessenger.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace InstantMessenger.ViewModels
{
    public class ChatViewModel : ViewModelBase
    {
        private string _newMessage;
        private User _selectedUser;

        public User SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        public string NewMessage
        {
            get => _newMessage;
            set => SetProperty(ref _newMessage, value);
        }

        public ObservableCollection<Message> Messages { get; } = new ObservableCollection<Message>();

        public ICommand SendMessageCommand { get; }

        public event EventHandler<(string, string, string)> SendMessageRequested;

        public ChatViewModel()
        {
            SendMessageCommand = new RelayCommand(ExecuteSendMessage, CanSendMessage);
        }

        private bool CanSendMessage(object parameter)
        {
            return SelectedUser != null && !string.IsNullOrWhiteSpace(NewMessage);
        }

        private void ExecuteSendMessage(object parameter)
        {
            if (SelectedUser == null || string.IsNullOrWhiteSpace(NewMessage)) return;

            // Notificar que se solicita enviar un mensaje
            SendMessageRequested?.Invoke(this, (SelectedUser.IpAddress, NewMessage, SelectedUser.Username));

            // Agregar mensaje a la colección local (optimista)
            Messages.Add(new Message("Tú", NewMessage, true));

            // Limpiar el texto del mensaje
            NewMessage = string.Empty;
        }

        public void AddMessage(string sender, string content, bool isFromMe = false)
        {
            var message = new Message(sender, content, isFromMe);
            Messages.Add(message);
        }

        public void Clear()
        {
            Messages.Clear();
            NewMessage = string.Empty;
        }
    }
}