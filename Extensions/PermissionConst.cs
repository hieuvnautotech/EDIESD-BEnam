namespace ESD_EDI_BE.Extensions
{
    public static class PermissionConst
    {
        /// <summary>
        /// USERINFO
        /// </summary>
        public const string USER_READ = "menuPermission.standard.configuration.user.view";
        public const string USER_CREATE = "menuPermission.standard.configuration.user.create";
        public const string USER_UPDATE = "menuPermission.standard.configuration.user.modify";
        public const string USER_DELETE = "menuPermission.standard.configuration.user.delete";
        public const string USER_CHANGE_PASS = "menuPermission.standard.configuration.user.change_pass";

        /// <summary>
        /// ROLE
        /// </summary>
        public const string ROLE_READ = "menuPermission.standard.configuration.role.view";
        public const string ROLE_CREATE = "menuPermission.standard.configuration.role.create";
        public const string ROLE_UPDATE = "menuPermission.standard.configuration.role.modify";
        public const string ROLE_DELETE = "menuPermission.standard.configuration.role.delete";
        public const string ROLE_SET_MENU = "menuPermission.standard.configuration.role.set_menu";
        public const string ROLE_SET_PERMISSION = "menuPermission.standard.configuration.role.set_permission";

        /// <summary>
        /// COMMONMASTER
        /// </summary>
        public const string COMMONMASTER_READ = "menuPermission.standard.configuration.common.commonmaster.view";
        public const string COMMONMASTER_CREATE = "menuPermission.standard.configuration.common.commonmaster.create";
        public const string COMMONMASTER_UPDATE = "menuPermission.standard.configuration.common.commonmaster.modify";
        public const string COMMONMASTER_DELETE = "menuPermission.standard.configuration.common.commonmaster.delete";

        /// <summary>
        /// COMMONDETAIL
        /// </summary>
        public const string COMMONDETAIL_READ = "menuPermission.standard.configuration.common.commondetail.view";
        public const string COMMONDETAIL_CREATE = "menuPermission.standard.configuration.common.commondetail.create";
        public const string COMMONDETAIL_UPDATE = "menuPermission.standard.configuration.common.commondetail.modify";
        public const string COMMONDETAIL_DELETE = "menuPermission.standard.configuration.common.commondetail.delete";

        /// <summary>
        /// MENU
        /// </summary>
        public const string MENU_READ = "menuPermission.standard.configuration.menu.view";
        public const string MENU_CREATE = "menuPermission.standard.configuration.menu.create";
        public const string MENU_UPDATE = "menuPermission.standard.configuration.menu.modify";
        public const string MENU_DELETE = "menuPermission.standard.configuration.menu.delete";
        public const string MENU_SET_PERMISSION = "menuPermission.standard.configuration.menu.set_permission";

        /// <summary>
        /// DOCUMENT
        /// </summary>
        public const string DOCUMENT_READ = "menuPermission.standard.configuration.document.view";
        public const string DOCUMENT_CREATE = "menuPermission.standard.configuration.document.create";
        public const string DOCUMENT_UPDATE = "menuPermission.standard.configuration.document.modify";
        public const string DOCUMENT_DELETE = "menuPermission.standard.configuration.document.delete";

        /*------------------------------------------------------------------------*/

        /// <summary>
        /// Q1 Management
        /// </summary>
        public const string Q1MANAGEMENT_READ = "menuPermission.edi.q1management.view";
        public const string Q1MANAGEMENT_CREATE = "menuPermission.edi.q1management.create";
        public const string Q1MANAGEMENT_UPDATE = "menuPermission.edi.q1management.modify";
        public const string Q1MANAGEMENT_DELETE = "menuPermission.edi.q1management.delete";

        /// <summary>
        /// Q2 Management
        /// </summary>
        public const string Q2MANAGEMENT_READ = "menuPermission.edi.q2management.view";
        public const string Q2MANAGEMENT_CREATE = "menuPermission.edi.q2management.create";
        public const string Q2MANAGEMENT_UPDATE = "menuPermission.edi.q2management.modify";
        public const string Q2MANAGEMENT_DELETE = "menuPermission.edi.q2management.delete";

        /// <summary>
        /// Q2 Policy
        /// </summary>
        public const string Q2POLICY_READ = "menuPermission.standard.information.q2policy.view";
        public const string Q2POLICY_CREATE = "menuPermission.standard.information.q2policy.create";
        public const string Q2POLICY_UPDATE = "menuPermission.standard.information.q2policy.modify";
        public const string Q2POLICY_DELETE = "menuPermission.standard.information.q2policy.delete";

        /// <summary>
        /// Machine
        /// </summary>
        public const string MACHINE_READ = "menuPermission.standard.information.machine.view";
        public const string MACHINE_CREATE = "menuPermission.standard.information.machine.create";
        public const string MACHINE_UPDATE = "menuPermission.standard.information.machine.modify";
        public const string MACHINE_DELETE = "menuPermission.standard.information.machine.delete";
    }
}
