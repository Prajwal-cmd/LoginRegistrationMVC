using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Data;
using System.Data.SqlClient;
using LoginRegistrationMVC.Data;
using System.Collections.Generic;
using System.Linq;

namespace LoginRegistrationMVC.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _userConnections = new ConcurrentDictionary<string, string>();
        private readonly UserRepository _userRepository = new UserRepository();
        private readonly string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public override Task OnConnected()
        {
            string userName = HttpContext.Current?.User?.Identity?.Name ?? "Anonymous";
            if (!string.IsNullOrEmpty(userName))
            {
                _userConnections.AddOrUpdate(userName, Context.ConnectionId, (key, oldValue) => Context.ConnectionId);
            }
            return base.OnConnected();
        }

        public void CreateChat(string chatType, string[] participantUserNames)
        {
            string userName = HttpContext.Current?.User?.Identity?.Name ?? "Anonymous";
            var user = _userRepository.GetUserByEmail(userName);
            if (user == null || user.Role != "instructor")
            {
                Clients.Caller.sendError("Only instructors can create chats.");
                return;
            }

            var participantIds = new DataTable();
            participantIds.Columns.Add("UserId", typeof(int));
            foreach (var participantUserName in participantUserNames)
            {
                var participant = _userRepository.GetUserByEmail(participantUserName);
                if (participant != null)
                {
                    participantIds.Rows.Add(participant.Id);
                }
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("sp_ChatCreate_1", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CreatorId", user.Id);
                    command.Parameters.AddWithValue("@ChatType", chatType);
                    command.Parameters.Add(new SqlParameter("@Participants", SqlDbType.Structured)
                    {
                        Value = participantIds,
                        TypeName = "dbo.UserIdTableType"
                    });

                    int chatId = (int)command.ExecuteScalar();
                    Clients.Caller.sendSuccess($"Chat {chatId} created successfully.");
                    // Refresh chat list after creation
                    RequestChatList();
                }
            }
        }

        public void SendMessage(int chatId, string message)
        {
            string userName = HttpContext.Current?.User?.Identity?.Name ?? "Anonymous";
            var user = _userRepository.GetUserByEmail(userName);
            if (user == null)
            {
                Clients.Caller.sendError("User not found.");
                return;
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("sp_ChatAddMessage_1", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ChatId", chatId);
                    command.Parameters.AddWithValue("@SenderId", user.Id);
                    command.Parameters.AddWithValue("@MessageText", message);

                    command.ExecuteNonQuery();
                }
            }

            // Broadcast to all connected users in the chat
            var chatMembers = GetChatMembers(chatId);
            foreach (var memberUserName in chatMembers)
            {
                if (_userConnections.TryGetValue(memberUserName, out string connectionId))
                {
                    Clients.Client(connectionId).broadcastMessage(userName, message);
                }
            }
        }

        public void RequestChatList()
        {
            string userName = HttpContext.Current?.User?.Identity?.Name ?? "Anonymous";
            var user = _userRepository.GetUserByEmail(userName);
            if (user == null) return;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    @"SELECT DISTINCT c.Id, c.ChatType 
                      FROM Chats c 
                      INNER JOIN ChatMembers cm ON c.Id = cm.ChatId 
                      WHERE cm.UserId = @UserId", connection))
                {
                    command.Parameters.AddWithValue("@UserId", user.Id);
                    var chats = new List<dynamic>();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            chats.Add(new { Id = reader.GetInt32(0), ChatType = reader.GetString(1) });
                        }
                    }
                    Clients.Caller.updateChatList(chats);
                }
            }
        }

        public void RequestChatHistory(int chatId)
        {
            string userName = HttpContext.Current?.User?.Identity?.Name ?? "Anonymous";
            var user = _userRepository.GetUserByEmail(userName);
            if (user == null) return;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    @"SELECT u.Email AS SenderName, m.MessageText, m.Timestamp 
                      FROM Messages m 
                      INNER JOIN Users u ON m.SenderId = u.Id 
                      WHERE m.ChatId = @ChatId 
                      ORDER BY m.Timestamp ASC", connection))
                {
                    command.Parameters.AddWithValue("@ChatId", chatId);
                    var messages = new List<dynamic>();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            messages.Add(new
                            {
                                SenderName = reader.GetString(0),
                                MessageText = reader.GetString(1),
                                Timestamp = reader.GetDateTime(2).ToString("g") // Short date and time format
                            });
                        }
                    }
                    Clients.Caller.updateChatHistory(messages);
                }
            }
        }

        private List<string> GetChatMembers(int chatId)
        {
            var members = new List<string>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    @"SELECT u.Email 
                      FROM ChatMembers cm 
                      INNER JOIN Users u ON cm.UserId = u.Id 
                      WHERE cm.ChatId = @ChatId", connection))
                {
                    command.Parameters.AddWithValue("@ChatId", chatId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            members.Add(reader.GetString(0)); // Email as username
                        }
                    }
                }
            }
            return members;
        }
    }
}