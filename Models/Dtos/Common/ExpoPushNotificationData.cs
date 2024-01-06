namespace ESD_EDI_BE.Models.Dtos.Common
{
    public class ExpoPushNotificationData
    {

        public IList<string> registration_ids { get; set; }
        public FCMNotification data { get; set; }

        public ExpoPushNotificationData()
        {
            registration_ids = new List<string>();
            data = new FCMNotification();
        }
    }

    public class FCMNotification
    {
        public string title { get; set; }
        public string message { get; set; }
        public FCMNotificationBody body { get; set; }
        public FCMNotification()
        {
            title = "SoluM Information";
            message = "New application version is coming";
            body = new FCMNotificationBody();
        }
    }

    public class FCMNotificationBody
    {
        public int update_type { get; set; }
        public string link_url { get; set; }
        public FCMNotificationBody()
        {
            update_type = 0;
            link_url = "https://play.google.com/store/apps/details?id=com.evilhero.solumapp";
        }
    }

    public class FCMNotificationData
    {
        public string experienceId { get; set; }
        public string scopeKey { get; set; }
        public string title { get; set; }
        public string detail { get; set; }
    }

}
