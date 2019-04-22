using System;
using System.Collections.Generic;

namespace Fizz.Chat
{
    public interface IFizzChatClient
    {
        IFizzChannelMessageListener Listener { get; }

        void PublishMessage (string channel,
            string nick,
            string body,
            Dictionary<string, string> data,
            bool translate,
            bool persist,
            Action<FizzException> callback);
        void UpdateMessage (string channel,
            long messageId,
            string nick,
            string body,
            Dictionary<string, string> data,
            bool translate,
            bool persist,
            Action<FizzException> callback);
        void DeleteMessage (string channelId, 
            long messageId,
            Action<FizzException> callback);
        
        void QueryLatest (string channel,
            int count,
            Action<IList<FizzChannelMessage>, FizzException> callback);
        void QueryLatest (string channel,
            int count,
            long beforeId,
            Action<IList<FizzChannelMessage>, FizzException> callback);
        
        void Subscribe (string channel, Action<FizzException> callback);
        void Unsubscribe (string channel, Action<FizzException> callback);

        void Ban (string channel, string userId, Action<FizzException> callback);
        void Unban (string channel, string userId, Action<FizzException> callback);

        void Mute (string channel, string userId, Action<FizzException> callback);
        void Unmute (string channel, string userId, Action<FizzException> callback);
    }
}