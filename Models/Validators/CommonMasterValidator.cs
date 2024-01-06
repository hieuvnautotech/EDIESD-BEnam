using FluentValidation;
using ESD_EDI_BE.Models.Dtos.Common;

namespace ESD_EDI_BE.Models.Validators
{
    public class CommonMasterValidator : AbstractValidator<CommonMasterDto>
    {
        public CommonMasterValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
            RuleFor(s => s.commonMasterName).NotEmpty().WithMessage("model.commonMaster.error_message.commonMasterName_required");
        }
    }
}