using System;
using System.Collections.Generic;
using System.Linq;
using Fizz.Chat;
using Fizz.Common;
using Fizz.Chat.Impl;
using Fizz.Common.Json;

namespace Fizz.UI.Model
{
    public class FizzGroupChannel : FizzChannelModel
    {
        public string GroupId { get; private set; }

        public override IList<FizzChannelMessage> Messages
        {
            get
            {
                if (FizzService.Instance.GroupRepository.GroupInvites.ContainsKey(GroupId))
                {
                    // Show Group invite cell for pending invite group
                    IList<FizzChannelMessage> inviteMessage = new List<FizzChannelMessage>();
                    inviteMessage.Add(CreateGroupInviteMessage(FizzService.Instance.GroupRepository.GroupInvites[GroupId]));
                    return inviteMessage;
                }
                return base.Messages;
            }
        }

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
                if (FizzService.Instance.GroupRepository.GroupInvites.ContainsKey(GroupId))
                {
                    // Can't fetch messages of group with pending membership
                    return;
                }

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
            if (FizzService.Instance.GroupRepository.GroupInvites.ContainsKey(GroupId))
            {
                // Can't send messages in group with pending membership
                return;
            }

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

        private FizzChannelMessage CreateGroupInviteMessage(IFizzUserGroup userGroup)
        {
            // Show Group invite cell for pending invite group
            JSONClass invite = new JSONClass();
            invite.Add(FizzJsonChannelMessage.KEY_ID, "1");
            invite.Add(FizzJsonChannelMessage.KEY_FROM, GroupId);
            invite.Add(FizzJsonChannelMessage.KEY_TO, _channelMeta.Id);
            //invite.Add("topic", _channelMeta.Id);
            invite.Add(FizzJsonChannelMessage.KEY_CREATED, userGroup.Created.ToString());
            invite.Add(FizzJsonChannelMessage.KEY_NICK, _channelMeta.Name);
            invite.Add(FizzJsonChannelMessage.KEY_BODY, "");
            JSONClass data = new JSONClass();
            data.Add("custom-type", "invite");
            data.Add("group-id", GroupId);
            data.Add("title", _channelMeta.Name);
            invite.Add(FizzJsonChannelMessage.KEY_DATA, data.ToString());

            return new FizzJsonChannelMessage(invite.ToString());
        }

    }
}