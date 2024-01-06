using FluentValidation;
using ESD_EDI_BE.Models.Dtos;
using ESD_EDI_BE.Models.Dtos.Common;

namespace ESD_EDI_BE.Models.Validators
{
    public class ExpoTokenValidator : AbstractValidator<ExpoTokenDto>
    {
        public ExpoTokenValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
            RuleFor(s => s.ExpoToken).NotEmpty().WithMessage("Token is required");
        }
    }
}
