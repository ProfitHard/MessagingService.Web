using FluentValidation;
using MessagingService.Web.Models;

namespace MessagingService.Web.Validators;

public class MessageValidator : AbstractValidator<Message>
{
    public MessageValidator()
    {
        RuleFor(m => m.Id).NotEmpty().WithMessage("Sender is required.");
        RuleFor(m => m.Text).NotEmpty().WithMessage("Content is required.").MaximumLength(500);
        RuleFor(m => m.Timestamp).NotEmpty().WithMessage("Timestamp is required.");
    }
}