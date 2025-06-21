using FluentValidation;
using MusicService.WebApi.Contracts.Requests;

namespace MusicService.WebApi.Contracts.Validators;

public class SignInRequestValidator : AbstractValidator<SignInRequest>
{
    public SignInRequestValidator()
    {
        RuleFor(e => e.EmailOrUsername)
            .NotEmpty();
        
        RuleFor(e => e.Password)
            .NotEmpty();
    }
}