using Microsoft.AspNetCore.RateLimiting;
using Vexora.Api.Commons.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vexora.Api.Commons.Results;
using Vexora.Api.Contracts.Chats;
using Vexora.Api.Mappers.Chats;
using Vexora.Application.Caching.Conversations;
using Vexora.Application.Services.Interfaces.Chats;
using Vexora.Application.Services.Interfaces.CurrentUser;

namespace Vexora.Api.Controllers.Chats;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class ChatController(
    IChatService chatService,
    ICurrentUser currentUser)
    : ControllerBase
{
    [EnableRateLimiting("chat")]
    [RequireActiveSubscription]
    [HttpPost]
    public async Task<ApiResult<ChatResponse>> Post(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await chatService.ProcessAsync(
                request.ChatRequestMapToDto(CurrentConversationId()),
                cancellationToken);

        return ApiResult<ChatResponse>.SuccessResult(
            response.ChatResponseDtoMapToContract());
    }

    [HttpGet("pending")]
    public async Task<ApiResult<ChatResponse?>> GetPending(
        CancellationToken cancellationToken)
    {
        var response =
            await chatService.GetPendingAsync(
                CurrentConversationId(),
                cancellationToken);

        return ApiResult<ChatResponse?>.SuccessResult(
            response?.ChatResponseDtoMapToContract());
    }

    [EnableRateLimiting("chat")]
    [RequireActiveSubscription]
    [HttpPost("reset")]
    public async Task<ApiResult<object?>> Reset(
        CancellationToken cancellationToken)
    {
        await chatService.ResetAsync(
            CurrentConversationId(),
            cancellationToken);

        return ApiResult<object?>.SuccessResult(null, "گفتگوی جاری پاک شد.");
    }

    private string CurrentConversationId()
        => ConversationIds.ForUser(currentUser.UserId);
}
