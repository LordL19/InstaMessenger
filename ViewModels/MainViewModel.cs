using InstantMessenger.Helpers;
using InstantMessenger.Models;
using InstantMessenger.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace InstantMessenger.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly DiscoveryService _discoveryService;
        private readonly ChatService _chatService;
        private User _selectedUser;
        private ChatViewModel _activeChatViewModel;
        private Dictionary<string, ChatViewModel> _chatViewModels = new Dictionary<string, ChatViewModel>();

        public string Username { get; }

        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();

        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (SetProperty(ref _selectedUser, value) && value != null)
                {
                    // Cambiar al chat con el usuario seleccionado
                    ActiveChatViewModel = GetOrCreateChatViewModel(value);
                }
            }
        }

        public ChatViewModel ActiveChatViewModel
        {
            get => _activeChatViewModel;
            private set => SetProperty(ref _activeChatViewModel, value);
        }

        public ICommand SearchUsersCommand { get; }

        public MainViewModel(string username)
        {
            Username = username;

            _discoveryService = new DiscoveryService(UpdateUserList, username);
            _chatService = new ChatService(OnMessageReceived);

            SearchUsersCommand = new RelayCommand(_ => RefreshUserList());

            // Iniciar servicios
            _chatService.StartChatServer();
            _discoveryService.StartDiscovery();

            // Buscar usuarios inicialmente
            RefreshUserList();
        }

        private void UpdateUserList(Dictionary<string, string> users)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Users.Clear();

                foreach (var user in users)
                {
                    Users.Add(new User(user.Key, user.Value));
                }
            });
        }

        private void RefreshUserList()
        {
            var discoveredUsers = _discoveryService.GetDiscoveredUsers();
            UpdateUserList(discoveredUsers);
        }

        private void OnMessageReceived(string sender, string content)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Buscar el usuario correspondiente
                User user = null;

                foreach (var u in Users)
                {
                    if (u.Username == sender)
                    {
                        user = u;
                        break;
                    }
                }

                if (user != null)
                {
                    // Obtener o crear el ViewModel del chat
                    var chatViewModel = GetOrCreateChatViewModel(user);

                    // Agregar el mensaje
                    chatViewModel.AddMessage(sender, content);

                    // Si este chat no es el activo, podríamos notificar al usuario de alguna manera
                    // Por ejemplo, agregando un indicador visual para ese usuario en la lista
                }
            });
        }

        private ChatViewModel GetOrCreateChatViewModel(User user)
        {
            if (!_chatViewModels.TryGetValue(user.Username, out var chatViewModel))
            {
                chatViewModel = new ChatViewModel { SelectedUser = user };
                chatViewModel.SendMessageRequested += OnSendMessageRequested;
                _chatViewModels[user.Username] = chatViewModel;
            }

            return chatViewModel;
        }

        private async void OnSendMessageRequested(object sender, (string IpAddress, string Message, string Username) e)
        {
            bool success = await _chatService.SendMessageAsync(e.IpAddress, e.Message, Username);

            if (!success)
            {
                // Manejar error - por ejemplo, mostrar un mensaje
                MessageBox.Show($"No se pudo enviar el mensaje a {e.Username}. Verifique la conexión.",
                               "Error de envío",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }
        }

        public void Cleanup()
        {
            _discoveryService.StopDiscovery();
            _chatService.StopChatServer();
        }
    }
}