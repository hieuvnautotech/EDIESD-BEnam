using FluentValidation;
using ESD_EDI_BE.Models.Dtos;
using ESD_EDI_BE.Models.Dtos.Common;

namespace ESD_EDI_BE.Models.Validators
{
    public class CommonDetailValidator : AbstractValidator<CommonDetailDto>
    {
        public CommonDetailValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
            RuleFor(s => s.commonDetailName).NotEmpty().WithMessage("model.commonDetail.error_message.commonDetailName_required");
        }
    }
}
