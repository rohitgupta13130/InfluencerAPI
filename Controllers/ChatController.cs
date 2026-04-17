using InfluencerAPI.Dtos;
using InfluencerAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly IChatRepository _chatRepository;

    public ChatController(IChatRepository chatRepository)
    {
        _chatRepository = chatRepository;
    }

    // ✅ Send Message (SECURE)
    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] ChatMessageRequest model)
    {
        if (model == null || string.IsNullOrEmpty(model.Message))
        {
            return BadRequest("Message cannot be empty");
        }

        var userIdClaim = User.FindFirst("UserId");

        if (userIdClaim == null)
            return Unauthorized();

        int senderId = Convert.ToInt32(userIdClaim.Value);

        model.SenderId = senderId; // 🔥 secure (don’t trust frontend)

        await _chatRepository.SendMessage(model);

        return Ok(new { message = "Message sent successfully" });
    }

    // ✅ Get Chat (SECURE)
    [HttpGet]
    public async Task<IActionResult> GetChat(int receiverId)
    {
        var userIdClaim = User.FindFirst("UserId");

        if (userIdClaim == null)
            return Unauthorized();

        int userId = Convert.ToInt32(userIdClaim.Value);

        var chats = await _chatRepository.GetChat(userId, receiverId);

        return Ok(chats);
    }

    // ✅ Mark as Read
    [HttpPatch("read/{id}")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid Id");

        await _chatRepository.MarkAsRead(id);

        return Ok(new { message = "Message marked as read" });
    }
}