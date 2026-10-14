using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vexora.Api.Commons.Results;
using Vexora.Api.Contracts.Chats;
using Vexora.Api.Mappers.Chats;
using Vexora.Application.Dtos.Chats;
using Vexora.Application.Services.Interfaces.Chats;

namespace Vexora.Api.Controllers.Chats;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class ChatController(
    IChatService chatService)
    : ControllerBase
{
    [HttpPost]
    public async Task<ApiResult<ChatResponse>> Post(
        [FromBody] ChatRequestDto request,
        CancellationToken cancellationToken)
    {
        var response =
            await chatService.ProcessAsync(
                request,
                cancellationToken);

        return ApiResult<ChatResponse>.SuccessResult(
            response.ChatResponseDtoMapToContract());
    }
}