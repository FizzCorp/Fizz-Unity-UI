using System;
using System.Collections.Generic;
using System.Linq;
using Fizz.Chat;
using Fizz.Common;

namespace Fizz.UI.Model
{
    public class FizzGroupChannel : FizzChannel
    {
        public string GroupId { get; private set; }

        public FizzGroupChannel(string groupId, FizzChannelMeta channelMeta) : base(channelMeta)
        {
            GroupId = groupId;
        }

        public override void Subscribe(Action<FizzException> cb)
        {
            FizzUtils.DoCallback(null, cb);
        }

        public override void Unsubscribe(Action<FizzException> cb)
        {
            FizzUtils.DoCallback(null, cb);
        }

        public override void SubscribeAndQueryLatest()
        {
            try
            {
                Subscribe(subEx =>
                {
                    if (subEx != null)
                    {
                        FizzLogger.E("Subscribe Error " + Id + " ex " + subEx.Message);
                    }
                    else
                    {
                        FizzLogger.D("Subscribed " + Id);

                        FizzService.Instance.Client.Chat.Groups.QueryLatest(GroupId, _channelMeta.InitialQueryMessageCount, (msgs, qEx) =>
                        {
                            if (qEx == null)
                            {
                                Reset();

                                if (msgs != null && msgs.Count > 0)
                                {
                                    AddMessages(msgs);
                                }

                                if (FizzService.Instance.OnChannelMessagesAvailable != null)
                                {
                                    FizzService.Instance.OnChannelMessagesAvailable.Invoke(Id);
                                }
                            }
                            else
                            {
                                FizzLogger.E("QueryLatest " + qEx.Message);
                            }
                        });
                    }
                });
            }
            catch (FizzException ex)
            {
                FizzLogger.E("SubscribeAndQuery ex " + ex.Message);
            }
        }

        public override bool FetchHistory(Action complete)
        {
            long beforeId = -1;
            if (_messageList.Count > 0)
                beforeId = _messageList.First().Value.Id;

            if (beforeId == -1)
                return false;

            try
            {
                FizzService.Instance.Client.Chat.Groups.QueryLatest(GroupId, _channelMeta.InitialQueryMessageCount, beforeId, (msgs, qEx) =>
                {
                    if (qEx == null)
                    {
                        if (msgs != null && msgs.Count > 0)
                        {
                            AddMessages(msgs);
                        }

                        if (FizzService.Instance.OnChannelMessagesAvailable != null)
                        {
                            FizzService.Instance.OnChannelMessagesAvailable.Invoke(Id);
                        }
                    }

                    if (complete != null)
                        complete.Invoke();
                });
            }
            catch (FizzException ex)
            {
                FizzLogger.E("FetchHistory ex " + ex.Message);
            }

            return true;
        }

        public override void PublishMessage(string nick,
                                    string body,
                                    Dictionary<string, string> data,
                                    bool translate,
                                    Action<FizzException> callback)
        {
            FizzService.Instance.Client.Chat.Groups.PublishMessage(
                    GroupId,
                    nick,
                    body,
                    data,
                    translate,
                    Meta.FilterContent,
                    Meta.PersistMessages,
                    ex =>
                    {
                        FizzUtils.DoCallback(ex, callback);
                    });
        }

    }
}