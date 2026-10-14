using FluentValidation;
using Vexora.Api.Contracts.Authentications;

namespace Vexora.Api.Validators.Authentications;

public sealed class SendOtpContractValidator : AbstractValidator<SendOtpContract>
{
    public SendOtpContractValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("نام الزامی است")
            .MaximumLength(100).WithMessage("نام نباید بیشتر از ۱۰۰ کاراکتر باشد");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("نام خانوادگی الزامی است")
            .MaximumLength(100).WithMessage("نام خانوادگی نباید بیشتر از ۱۰۰ کاراکتر باشد");

        RuleFor(x => x.PhoneNumber).Phone();

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("نام الزامی است")
            .MinimumLength(8).WithMessage("رمز عبور نباید کمتر از 8 کاراکتر باشد");
    }
}

