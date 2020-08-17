using System;

namespace Fizz.Chat
{
    public class FizzUserUpdateEventData
    {
        public enum UpdateReason
        {
            Profile,
            Unknown
        }

        public FizzUserUpdateEventData()
        {
            Reason = UpdateReason.Unknown;
        }

        public UpdateReason Reason { get; set; }
        public string UserId { get; set; }
        public bool Online { get; set; }
    }


    public interface IFizzUserListener
    {
        Action<FizzUserUpdateEventData> OnUserUpdated { get; set; }
    }
}