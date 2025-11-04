using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace ViverAppVideoHub.Controllers
{
    public class VideoHub : Hub
    {
        private static readonly ConcurrentDictionary<string, HashSet<string>> Rooms = new();

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            foreach (var kvp in Rooms)
            {
                if (kvp.Value.Contains(Context.ConnectionId))
                {
                    kvp.Value.Remove(Context.ConnectionId);
                    if (kvp.Value.Count == 0)
                        Rooms.TryRemove(kvp.Key, out _);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(string roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            var users = Rooms.GetOrAdd(roomId, _ => new HashSet<string>());
            lock (users)
            {
                users.Add(Context.ConnectionId);
            }

            await Clients.Group(roomId).SendAsync("ReceiveSignal", "system",
                $"📢 Usuário entrou na sala {roomId} ({users.Count} conectado)");
        }

        public async Task LeaveRoom(string roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            if (Rooms.TryGetValue(roomId, out var users))
            {
                lock (users)
                {
                    users.Remove(Context.ConnectionId);
                    if (users.Count == 0)
                        Rooms.TryRemove(roomId, out _);
                }
            }
        }

        public async Task SendSignal(string roomId, string type, string data)
        {
            await Clients.GroupExcept(roomId, Context.ConnectionId)
                         .SendAsync("ReceiveSignal", type, data);
        }

        public static int GetRoomCount(string roomId)
        {
            if (Rooms.TryGetValue(roomId, out var users))
                return users.Count;
            return 0;
        }
    }
}
