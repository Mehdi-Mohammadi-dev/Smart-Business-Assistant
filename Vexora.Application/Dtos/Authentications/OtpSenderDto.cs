namespace Vexora.Application.Dtos.Authentications;

public record OtpSenderDto
(   
    string PhoneNumber,
    string Code
);
