using FluentValidation;
using MusicService.Domain.Constants;
using MusicService.WebApi.Contracts.Requests;

namespace MusicService.WebApi.Contracts.Validators;

public class TrackAddRequestValidator : AbstractValidator<TrackAddRequest>
{
    public TrackAddRequestValidator()
    {
        RuleFor(t => t.Title).NotEmpty().MaximumLength(Constraints.TrackTitleMaxLength);
        RuleFor(e => e.Performer).NotEmpty().MaximumLength(Constraints.TrackPerformerMaxLength);
    }
}