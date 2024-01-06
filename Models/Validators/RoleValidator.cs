using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESD_EDI_BE.Models.Dtos.Common;
using FluentValidation;

namespace ESD_EDI_BE.Models.Validators
{
    public class RoleValidator : AbstractValidator<RoleDto>
    {
        public RoleValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
            RuleFor(s => s.roleName).NotEmpty().WithMessage("model.role.error_message.roleName_required");
        }
    }
}