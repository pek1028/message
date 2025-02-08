using Code1.Models;
using Code1.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Code1.Hubs
{
    public class ChatHub : Hub
    {
        private static ConcurrentDictionary<string, User> ConnectedUsers = new();
        private static ConcurrentDictionary<string, List<string>> UserMessages = new();
        private readonly UserService _userService;

        public ChatHub()
        {
            _userService = new UserService();
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"Client Connected - Connection ID: {Context.ConnectionId}");

            await base.OnConnectedAsync();
        }

        public async Task RegisterUser(string userId, string username)
        {
            if (!_userService.IsUserRegistered(userId))
            {
                await Clients.Caller.SendAsync("ErrorOccurred", "User ID is not registered.");
                return;
            }

            if (!string.IsNullOrEmpty(username) && !ConnectedUsers.Values.Any(u => u.Username == username))
            {
                var user = new User
                {
                    Id = userId,
                    Username = username,
                    ConnectionId = Context.ConnectionId,
                    IsOnline = true
                };

                ConnectedUsers[Context.ConnectionId] = user;
                Console.WriteLine($"User registered: {username} with ConnectionId: {Context.ConnectionId}");

                await UpdateUserList();
            }
            else
            {
                await Clients.Caller.SendAsync("ErrorOccurred", "Username is already taken or invalid.");
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (ConnectedUsers.TryRemove(Context.ConnectionId, out var user))
            {
                user.IsOnline = false;
                await UpdateUserList();
            }

            Console.WriteLine($"Client Disconnected - Connection ID: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        private async Task UpdateUserList()
        {
            var userList = ConnectedUsers.Values.Select(u => u.Username).ToList();
            await Clients.All.SendAsync("UpdateUserList", userList);
        }

        public async Task SendMessage(string receiverUsername, string content)
        {
            try
            {
                var sender = ConnectedUsers[Context.ConnectionId];
                var receiver = ConnectedUsers.Values.FirstOrDefault(u => u.Username == receiverUsername);

                if (receiver == null)
                {
                    await Clients.Caller.SendAsync("ErrorOccurred", "Receiver not found.");
                    return;
                }

                var message = new
                {
                    FromUser = sender.Username,
                    ToUser = receiver.Username,
                    Message = content,
                    CreateDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                };

                Console.WriteLine($"Sending message from {sender.Username} to {receiver.Username}: {content}");

                // Add message to database
                // MessageHistory.Add(message);

                // Send message to both sender and receiver
                await Clients.Client(receiver.ConnectionId).SendAsync("ReceiveMessage", message);
                await Clients.Caller.SendAsync("ReceiveMessage", message);

                Console.WriteLine($"Message received by {receiver.Username}: {content}");
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ErrorOccurred", "Failed to send message: " + ex.Message);
            }
        }

        public async Task AcknowledgeMessage(string senderConnectionId, string messageContent)
        {
            Console.WriteLine($"Messages '{messageContent}' acknowledged by receiver.");
            await Clients.Client(senderConnectionId).SendAsync("AcknowledgeReceived", messageContent);
        }

        public async Task GetMessages(string otherUserUsername)
        {
            try
            {
                var sender = ConnectedUsers[Context.ConnectionId];
                var messages = new List<string>();

                if (UserMessages.ContainsKey(sender.Username))
                {
                    messages = UserMessages[sender.Username].Where(m => m.Contains(otherUserUsername)).ToList();
                }

                await Clients.Caller.SendAsync("LoadMessages", messages);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ErrorOccurred", "Failed to retrieve messages: " + ex.Message);
            }
        }

        public async Task DeleteMessage(string messageContent)
        {
            try
            {
                var sender = ConnectedUsers[Context.ConnectionId];

                if (UserMessages.ContainsKey(sender.Username))
                {
                    var messageToDelete = UserMessages[sender.Username].FirstOrDefault(m => m == messageContent);

                    if (messageToDelete != null)
                    {
                        UserMessages[sender.Username].Remove(messageToDelete);
                        await Clients.Caller.SendAsync("MessageDeleted", messageContent);
                    }
                    else
                    {
                        await Clients.Caller.SendAsync("ErrorOccurred", "Messages not found.");
                    }
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ErrorOccurred", "Failed to delete message: " + ex.Message);
            }
        }

        //public async Task GetMessageHistory(string otherUserUsername)
        //{
        //    var sender = ConnectedUsers[Context.ConnectionId];
        //    var messages = MessageHistory
        //        .Where(m => (m.FromUser == sender.Username && m.ToUser == otherUserUsername) ||
        //                    (m.FromUser == otherUserUsername && m.ToUser == sender.Username))
        //        .OrderBy(m => m.Timestamp)
        //        .ToList();

        //    await Clients.Caller.SendAsync("LoadMessages", messages);
        //}
    }
}
