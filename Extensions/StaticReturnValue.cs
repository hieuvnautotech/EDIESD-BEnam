namespace ESD_EDI_BE.Extensions
{
    public static class StaticReturnValue
    {
        // <summary>
        /// general
        /// </summary>
        public const string SUCCESS = $"general.success";
        public const string FAIL = $"general.fail";
        public const string NO_DATA = $"general.no_data";
        public const string SYSTEM_ERROR = $"general.system_error";
        public const string OBJECT_INVALID = $"general.object_invalid";
        public const string CHANGE_PASSWORD_NOT_ALLOWED = $"general.change_password_not_allowed";
        public const string REFRESH_REQUIRED = $"general.refresh_required";
        public const string UNAUTHORIZED = $"general.unauthorized";
        public const string DUPLICATED_CODE = $"general.duplicated_code";
        public const string DUPLICATED_NAME = $"general.duplicated_name";
        public const string FIELD_REQUIRED = $"general.field_required";

        // <summary>
        /// token
        /// </summary>
        public const string INVALID_ACCESS_TOKEN = $"token.access_token.invalid";
        public const string TOKEN_IS_NOT_EXPIRED = $"token.access_token.not_expired";
        public const string INVALID_REFRESH_TOKEN = $"token.refresh_token.invalid";
        public const string REFRESH_TOKEN_EXPIRED = $"token.refresh_token.expired";

        // <summary>
        /// login
        /// </summary>
        public const string ACCOUNT_PASSWORD_INVALID = $"login.account_password_invalid";
        public const string ACCOUNT_BLOCKED = $"login.account_blocked";
        public const string LOST_AUTHORIZATION = $"login.lost_authorization";

        // <summary>
        /// [dbo].[sysTbl_CommonMaster]
        /// </summary>
        public const string COMMONMASTER_NOTFOUND = $"commonMaster.not_found";
        public const string COMMONMASTER_DUPLICATED_NAME = $"commonMaster.duplicated_name";


        /// <summary>
        /// [dbo].[sysTbl_CommonDetail]
        /// </summary>
        public const string COMMONDETAIL_NOTFOUND = $"commonDetail.not_found";
        public const string COMMONDETAIL_DUPLICATED_NAME = $"commonDetail.duplicated_name";
        public const string COMMONDETAIL_EXISTED = $"commonDetail.commonDetail_existed";

        /// <summary>
        /// [dbo].[sysTbl_Menu]
        /// </summary>
        public const string MENU_DUPLICATED_COMPONENT = $"menu.duplicated_component";
        public const string MENU_DUPLICATED_NAVIGATEURL = $"menu.duplicated_navigateUrl";
        public const string MENU_PARENT_NOT_FOUND = $"menu.parent_not_found";

        /// <summary>
        /// [dbo].[sysTbl_Permission]
        /// </summary>
        public const string PERMISSION_EXISTED = $"permission.existed";

        /// <summary>
        /// [dbo].[sysTbl_Role]
        /// </summary>
        public const string ROLE_NOTFOUND = $"role.not_found";
        public const string ROLE_DUPLICATED_NAME = $"role.duplicated_name";

        /// <summary>
        /// [dbo].[sysTbl_User]
        /// </summary>
        public const string USER_NOTFOUND = $"UserInfo Not Found";
        public const string USER_EXISTED = $"Wrong Password";
        public const string PASSWORD_INVALID = $"Wrong Password";

        /// <summary>
        /// [dbo].[Supplier]
        /// </summary>
        public const string SUPPLIER_DUPLICATED_CODE = $"supplier.duplicated_code";

        /// <summary>
        /// [dbo].[Staff]
        /// </summary>
        public const string STAFF_DUPLICATED_CODE = $"staff.duplicated_code";

        /// <summary>
        /// [dbo].[Line]
        /// </summary>
        public const string LINE_DUPLICATED_NAME = $"line.duplicated_name";

        /// <summary>
        /// [dbo].[Buyer]
        /// </summary>
        public const string BUYER_DUPLICATED_CODE = $"buyer.duplicated_code";

        /// <summary>
        /// [dbo].[QCDetail]
        /// </summary>
        public const string QCDETAIL_EXISTED = $"qcDetail.qcDetail_existed";
        public const string QCDETAIL_DUPLICATED_QC = $"qcDetail.duplicated_code";

        /// <summary>
        /// [dbo].[Location]
        /// </summary>
        public const string LOCATION_DUPLICATED_CODE = $"location.duplicated_code";

    }
}
