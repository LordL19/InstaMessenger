using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace InstantMessenger
{
    public partial class MainWindow : Window
    {
        private DiscoveryService _discoveryService;
        private ChatService _chatService;
        private Dictionary<string, string> _discoveredUsers = new Dictionary<string, string>();
        private Dictionary<string, ListBox> _chatSessions = new Dictionary<string, ListBox>();
        private Dictionary<string, TextBox> _chatTextBoxes = new Dictionary<string, TextBox>();
        private CancellationTokenSource _cts;
        private string _username;

        public MainWindow(string customUsername)
        {
            InitializeComponent();
            _cts = new CancellationTokenSource();
            _username = customUsername;
            this.Title = $"Instant Messenger - {_username}";

            _discoveryService = new DiscoveryService(UpdateUserList, _username);
            _chatService = new ChatService(UpdateChatBox);
            _discoveryService.StartDiscovery();
        }

        private void UpdateUserList(List<string> users)
        {
            Dispatcher.Invoke(() =>
            {
                UsersListBox.Items.Clear();
                foreach (var user in users)
                {
                    UsersListBox.Items.Add(user);
                }
            });
        }

        private void UpdateChatBox(string message)
        {
            Dispatcher.Invoke(() =>
            {
                string sender = message.Split(':')[0];

                if (_chatSessions.ContainsKey(sender))
                {
                    _chatSessions[sender].Items.Add(message);
                }
            });
        }

        private void SearchUsers_Click(object sender, RoutedEventArgs e)
        {
            UsersListBox.Items.Clear();
            _discoveredUsers = _discoveryService.GetDiscoveredUsers();
            foreach (var user in _discoveredUsers.Keys)
            {
                UsersListBox.Items.Add(user);
            }
        }

        private void UsersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UsersListBox.SelectedItem == null)
                return;

            string selectedUser = UsersListBox.SelectedItem.ToString();
            OpenChatTab(selectedUser);
        }

        private void OpenChatTab(string user)
        {
            foreach (TabItem tab in ChatTabControl.Items)
            {
                if (tab.Header.ToString() == user)
                {
                    ChatTabControl.SelectedItem = tab;
                    return;
                }
            }

            TabItem newTab = new TabItem
            {
                Header = user
            };

            Grid chatGrid = new Grid();
            chatGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            chatGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            ListBox chatListBox = new ListBox
            {
                Margin = new Thickness(10),
                BorderThickness = new Thickness(1),
                Background = Brushes.White
            };

            // Se aplica el template con colores para mensajes enviados y recibidos
            DataTemplate chatTemplate = new DataTemplate();
            FrameworkElementFactory stackPanel = new FrameworkElementFactory(typeof(StackPanel));
            stackPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            stackPanel.SetValue(StackPanel.HorizontalAlignmentProperty, new Binding(".")
            {
                Converter = new MessageAlignmentConverter()
            });

            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(10));
            border.SetValue(Border.PaddingProperty, new Thickness(8, 6, 8, 6));
            border.SetBinding(Border.BackgroundProperty, new Binding(".")
            {
                Converter = new MessageBubbleColorConverter()
            });

            FrameworkElementFactory textBlock = new FrameworkElementFactory(typeof(TextBlock));
            textBlock.SetBinding(TextBlock.TextProperty, new Binding());
            textBlock.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
            textBlock.SetValue(TextBlock.ForegroundProperty, Brushes.White);

            border.AppendChild(textBlock);
            stackPanel.AppendChild(border);
            chatTemplate.VisualTree = stackPanel;
            chatListBox.ItemTemplate = chatTemplate;

            _chatSessions[user] = chatListBox;

            Grid.SetRow(chatListBox, 0);
            chatGrid.Children.Add(chatListBox);

            Grid messagePanel = new Grid();
            messagePanel.Margin = new Thickness(10);
            messagePanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            messagePanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            TextBox messageTextBox = new TextBox
            {
                Height = 36,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(8, 5, 8, 5),
                Background = Brushes.White,
                BorderThickness = new Thickness(1),
                AcceptsReturn = false,
                IsReadOnly = false,
                IsEnabled = true,
                Focusable = true,
                TabIndex = 0
            };

            _chatTextBoxes[user] = messageTextBox;

            Grid.SetColumn(messageTextBox, 0);
            messagePanel.Children.Add(messageTextBox);

            Button sendButton = new Button
            {
                Content = "Enviar",
                Width = 100,
                Height = 36,
                Margin = new Thickness(10, 0, 0, 0),
                Background = (SolidColorBrush)FindResource("TelegramBlue"),
                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold,
                BorderThickness = new Thickness(0)
            };

            sendButton.MouseEnter += (s, e) =>
            {
                sendButton.Background = (SolidColorBrush)FindResource("TelegramLightBlue");
                sendButton.Cursor = Cursors.Hand;
            };

            sendButton.MouseLeave += (s, e) =>
            {
                sendButton.Background = (SolidColorBrush)FindResource("TelegramBlue");
            };

            sendButton.Click += SendButton_Click;
            Grid.SetColumn(sendButton, 1);
            messagePanel.Children.Add(sendButton);

            Grid.SetRow(messagePanel, 1);
            chatGrid.Children.Add(messagePanel);

            newTab.Content = chatGrid;
            ChatTabControl.Items.Add(newTab);
            ChatTabControl.SelectedItem = newTab;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                messageTextBox.Focus();
                Keyboard.Focus(messageTextBox);
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (ChatTabControl.SelectedItem is TabItem selectedTab)
            {
                string selectedUser = selectedTab.Header.ToString();
                if (_chatSessions.ContainsKey(selectedUser))
                {
                    Grid grid = (Grid)selectedTab.Content;
                    Grid messagePanel = (Grid)grid.Children[1];
                    TextBox messageTextBox = messagePanel.Children[0] as TextBox;

                    string message = messageTextBox.Text.Trim();

                    if (!string.IsNullOrEmpty(message))
                    {
                        if (_discoveredUsers.ContainsKey(selectedUser))
                        {
                            string recipientIP = _discoveredUsers[selectedUser];
                            _chatService.SendMessage(recipientIP, message, _username);

                            _chatSessions[selectedUser].Items.Add($"Tú: {message}");
                            messageTextBox.Clear();
                        }
                    }
                }
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _cts.Cancel();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SearchUsers_Click(sender, e);
        }
    }

    public class MessageBubbleColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string message = value as string;
            if (message != null && message.StartsWith("Tú:"))
            {
                return new SolidColorBrush(Color.FromRgb(0, 122, 255)); // Azul
            }
            return new SolidColorBrush(Color.FromRgb(199, 199, 199)); // Gris claro
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string message = value as string;
            return message != null && message.StartsWith("Tú:") ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
