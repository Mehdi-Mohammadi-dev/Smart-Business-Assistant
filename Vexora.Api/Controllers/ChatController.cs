using Microsoft.AspNetCore.Mvc;
using Vexora.Application.Dtos.Chats;
using Vexora.Application.Services.Interfaces.Chats;

namespace Vexora.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ChatController(IChatService chatService): ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ChatResponseDto>> Post(
        [FromBody] ChatRequestDto request,
        CancellationToken cancellationToken)
    {
        var response =
            await chatService.ProcessAsync(
                request,
                cancellationToken);

        return Ok(response);
    }
}