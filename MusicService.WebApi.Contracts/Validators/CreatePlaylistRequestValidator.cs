using FluentValidation;
using MusicService.WebApi.Contracts.Requests;

namespace MusicService.WebApi.Contracts.Validators;

public class CreatePlaylistRequestValidator : AbstractValidator<CreatePlaylistRequest>
{
    public CreatePlaylistRequestValidator()
    {
        RuleFor(t => t.Title).NotEmpty();
    }
}