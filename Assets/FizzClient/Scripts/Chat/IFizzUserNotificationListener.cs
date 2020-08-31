using System;

namespace Fizz.Chat
{
    public class FizzUserNotificationData
    {
        public enum UpdateReason
        {
            Group,
            Unknown
        }

        public FizzUserNotificationData()
        {
            Reason = UpdateReason.Unknown;
        }

        public UpdateReason Reason { get; set; }
        public string Name { get; set; }
    }


    public interface IFizzUserNotificationListener
    {
        Action<FizzUserNotificationData> OnUserNotification { get; set; }
    }
}