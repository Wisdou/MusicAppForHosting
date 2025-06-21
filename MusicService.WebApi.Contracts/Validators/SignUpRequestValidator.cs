using FluentValidation;
using MusicService.WebApi.Contracts.Requests;

namespace MusicService.WebApi.Contracts.Validators;

public class SignUpRequestValidator : AbstractValidator<SignUpRequest>
{
    public SignUpRequestValidator()
    {
        RuleFor(e => e.Username).NotEmpty();
        RuleFor(e => e.Password).NotEmpty().MinimumLength(6);
        RuleFor(e => e.Email).NotEmpty().EmailAddress();
    }
}