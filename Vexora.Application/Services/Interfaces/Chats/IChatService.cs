using System;
using System.Collections.Generic;
using System.Text;
using Vexora.Application.Dtos.Chats;

namespace Vexora.Application.Services.Interfaces.Chats;

public interface IChatService
{
    Task<ChatResponseDto> ProcessAsync(
        ChatRequestDto request,
        CancellationToken cancellationToken = default);
}
