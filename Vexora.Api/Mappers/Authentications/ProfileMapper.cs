using Vexora.Api.Contracts.Authentications.Profiles;
using Vexora.Application.Dtos.Authentications.Profiles;

namespace Vexora.Api.Mappers.Authentications;

public static class ProfileMapper
{
    public static UpdateProfileDto MapToDto(this UpdateProfileContract contract) =>
        new(contract.FirstName, contract.LastName, contract.BusinessName);
}
