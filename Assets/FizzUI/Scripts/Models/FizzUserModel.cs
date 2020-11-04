using System;
using Fizz.Chat;
using Fizz.Common;

namespace Fizz.UI.Model
{
    public class FizzUserModel
    {
        private readonly IFizzClient Client;

        public string Id { get; private set; }
        public string Nick { get; private set; }
        public string StatusMessage { get; private set; }
        public string ProfileUrl { get; private set; }
        public bool Online { get; private set; }

        public bool IsSubscribed { get; private set; }

        public FizzUserModel(IFizzUser userMeta, IFizzClient client)
        {
            Id = userMeta.Id;
            Nick = userMeta.Nick;
            StatusMessage = userMeta.StatusMessage;
            ProfileUrl = userMeta.ProfileUrl;
            Online = userMeta.Online;

            Client = client;
        }

        public void Apply(FizzUserModel user)
        {
            Nick = user.Nick;
            StatusMessage = user.StatusMessage;
            ProfileUrl = user.ProfileUrl;
            Online = user.Online;
        }

        public void Update(FizzUserUpdateEventData eventData)
        {
            switch (eventData.Reason)
            {
                case FizzUserUpdateEventData.UpdateReason.Profile:
                    Nick = eventData.Nick;
                    StatusMessage = eventData.StatusMessage;
                    ProfileUrl = eventData.ProfileUrl;
                    break;

                case FizzUserUpdateEventData.UpdateReason.Presence:
                    Online = eventData.Online;
                    break;
            }
        }

        public void Subscribe(Action<FizzException> cb)
        {
            if (Client.State == FizzClientState.Closed)
            {
                FizzLogger.W("FizzClient should be opened before subscribing user.");
                return;
            }

            Client.Chat.Users.Subscribe(Id, ex =>
            {
                if (ex == null)
                {
                    IsSubscribed = true;
                }

                FizzUtils.DoCallback(ex, cb);
            });

        }

        public void Unsubscribe(Action<FizzException> cb)
        {
            if (Client.State == FizzClientState.Closed)
            {
                FizzLogger.W("FizzClient should be opened before unsubscribing user.");
                return;
            }

            try
            {
                Client.Chat.Users.Unsubscribe(Id, ex =>
                {
                    if (ex == null)
                    {
                        IsSubscribed = false;
                    }

                    FizzUtils.DoCallback(ex, cb);
                });
            }
            catch (Exception e)
            {
                FizzLogger.E(e);
            }
        }
    }
}
