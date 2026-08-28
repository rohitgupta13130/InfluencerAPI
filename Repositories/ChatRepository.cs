using InfluencerAPI.Dtos;
using InfluencerBackendAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace InfluencerAPI.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Send Message
        public async Task SendMessage(ChatMessageRequest model)
        {
            using (var conn = _context.Database.GetDbConnection())
            {
                await conn.OpenAsync();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "influencer.SendMessage";
                    cmd.CommandType = CommandType.StoredProcedure;

                    var senderParam = cmd.CreateParameter();
                    senderParam.ParameterName = "@SenderId";
                    senderParam.Value = model.SenderId;
                    cmd.Parameters.Add(senderParam);

                    var receiverParam = cmd.CreateParameter();
                    receiverParam.ParameterName = "@ReceiverId";
                    receiverParam.Value = model.ReceiverId;
                    cmd.Parameters.Add(receiverParam);

                    var messageParam = cmd.CreateParameter();
                    messageParam.ParameterName = "@Message";
                    messageParam.Value = model.Message;
                    cmd.Parameters.Add(messageParam);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // ✅ Get Chat Between Two Users
        public async Task<List<ChatResponseRequest>> GetChat(int user1, int user2)
        {
            var chatList = new List<ChatResponseRequest>();

            using (var conn = _context.Database.GetDbConnection())
            {
                await conn.OpenAsync();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "influencer.GetChat";
                    cmd.CommandType = CommandType.StoredProcedure;

                    var param1 = cmd.CreateParameter();
                    param1.ParameterName = "@User1";
                    param1.Value = user1;
                    cmd.Parameters.Add(param1);

                    var param2 = cmd.CreateParameter();
                    param2.ParameterName = "@User2";
                    param2.Value = user2;
                    cmd.Parameters.Add(param2);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            chatList.Add(new ChatResponseRequest
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                SenderId = Convert.ToInt32(reader["SenderId"]),
                                ReceiverId = Convert.ToInt32(reader["ReceiverId"]),
                                Message = reader["Message"].ToString(),
                                SentAt = Convert.ToDateTime(reader["SentAt"]),
                                IsRead = Convert.ToBoolean(reader["IsRead"])
                            });
                        }
                    }
                }
            }

            return chatList;
        }

        public async Task MarkAsRead(int id)
        {
            using (var conn = _context.Database.GetDbConnection())
            {
                await conn.OpenAsync();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "influencer.MarkMessageAsRead";
                    cmd.CommandType = CommandType.StoredProcedure;

                    var param = cmd.CreateParameter();
                    param.ParameterName = "@Id";
                    param.Value = id;
                    cmd.Parameters.Add(param);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}