using System;
using System.Collections.Generic;
using Fizz.UI;
using Fizz.Chat;
using Fizz.Chat.Impl;
using Fizz.Common;
using Fizz.Common.Json;
using Fizz.UI.Model;

namespace Fizz
{
    public class FizzGroupRepository : IFizzGroupRepository
    {
        private Dictionary<string, FizzGroupModel> groupLookup;
        private IFizzClient Client { get; set; }
        private string UserId { get; set; }
        private string GroupTag { get; set; }

        public List<FizzGroupModel> Groups { get; private set; }
        public Dictionary<string, IFizzUserGroup> GroupInvites { get; private set; }

        #region Events
        public Action<FizzGroupModel> OnGroupAdded { get; set; }
        public Action<FizzGroupModel> OnGroupUpdated { get; set; }
        public Action<FizzGroupModel> OnGroupRemoved { get; set; }
        public Action<FizzGroupModel> OnGroupMembersUpdated { get; set; }
        #endregion

        public FizzGroupRepository(IFizzClient client, string groupTag)
        {
            Client = client;
            GroupTag = groupTag;

            Groups = new List<FizzGroupModel>();
            GroupInvites = new Dictionary<string, IFizzUserGroup>();
            groupLookup = new Dictionary<string, FizzGroupModel>();
        }

        public void Open(string userId)
        {
            UserId = userId;
            AddInternalListeners();
        }

        public void Close()
        {
            Groups.Clear();
            GroupInvites.Clear();
            groupLookup.Clear();

            RemoveInternalListeners();
        }

        void AddInternalListeners()
        {
            try
            {
                Client.Chat.Listener.OnConnected += Listener_OnConnected;
                Client.Chat.Listener.OnDisconnected += Listener_OnDisconnected;
                Client.Chat.GroupListener.OnGroupUpdated += Listener_OnGroupUpdated;
                Client.Chat.GroupListener.OnGroupMemberAdded += Listener_OnGroupMemberAdded;
                Client.Chat.GroupListener.OnGroupMemberRemoved += Listener_OnGroupMemberRemoved;
                Client.Chat.GroupListener.OnGroupMemberUpdated += Listener_OnGroupMemberUpdated;
            }
            catch (FizzException ex)
            {
                FizzLogger.E("Unable to bind group listeners. " + ex.Message);
            }
        }

        void RemoveInternalListeners()
        {
            try
            {
                Client.Chat.Listener.OnConnected -= Listener_OnConnected;
                Client.Chat.Listener.OnDisconnected -= Listener_OnDisconnected;
                Client.Chat.GroupListener.OnGroupUpdated -= Listener_OnGroupUpdated;
                Client.Chat.GroupListener.OnGroupMemberAdded -= Listener_OnGroupMemberAdded;
                Client.Chat.GroupListener.OnGroupMemberRemoved -= Listener_OnGroupMemberRemoved;
                Client.Chat.GroupListener.OnGroupMemberUpdated -= Listener_OnGroupMemberUpdated;
            }
            catch (FizzException ex)
            {
                FizzLogger.E("Unable to unbind group listeners. " + ex.Message);
            }
        }

        void Listener_OnGroupUpdated(FizzGroupUpdateEventData eventData)
        {
            FizzGroupModel group = GetGroup(eventData.GroupId);
            if (group != null && OnGroupUpdated != null)
            {
                group.Update(eventData);
                OnGroupUpdated.Invoke(group);
            }
        }

        void Listener_OnGroupMemberAdded(FizzGroupMemberEventData eventData)
        {
            if (UserId.Equals(eventData.MemberId))
            {
                Client.Chat.Groups.FetchGroup(eventData.GroupId, (groupMeta, ex) =>
                {
                    if (ex == null)
                    {
                        AddGroup(new FizzGroupModel(groupMeta, GroupTag));
                        FizzGroupModel group = GetGroup(groupMeta.Id);
                        if (eventData.State == FizzGroupMemberState.Pending)
                        {
                            GroupInvites.Add(group.Id, CreateUserGroup(eventData));
                        }
                        if (OnGroupAdded != null)
                        {
                            OnGroupAdded.Invoke(GetGroup(eventData.GroupId));
                        }
                    }
                });
            }
            else
            {
                FizzGroupModel group = GetGroup(eventData.GroupId);
                if (group != null && OnGroupMembersUpdated != null)
                {
                    OnGroupMembersUpdated.Invoke(group);
                }
            }
        }

        void Listener_OnGroupMemberRemoved(FizzGroupMemberEventData eventData)
        {
            if (UserId.Equals(eventData.MemberId))
            {
                FizzGroupModel group = GetGroup(eventData.GroupId);
                GroupInvites.Remove(eventData.GroupId);
                Groups.Remove(groupLookup[eventData.GroupId]);
                groupLookup.Remove(eventData.GroupId);

                if (group != null && OnGroupRemoved != null)
                {
                    OnGroupRemoved.Invoke(group);
                }
            }
            else
            {
                FizzGroupModel group = GetGroup(eventData.GroupId);
                if (group != null && OnGroupMembersUpdated != null)
                {
                    OnGroupMembersUpdated.Invoke(group);
                }
            }
        }

        void Listener_OnGroupMemberUpdated(FizzGroupMemberEventData eventData)
        {
            FizzGroupModel group = GetGroup(eventData.GroupId);
            if (group != null)
            {
                if (eventData.MemberId.Equals(UserId) && eventData.State == FizzGroupMemberState.Joined)
                {
                    GroupInvites.Remove(eventData.GroupId);
                }
                if (OnGroupMembersUpdated != null)
                    OnGroupMembersUpdated.Invoke(group);
            }
        }

        void Listener_OnDisconnected(FizzException obj)
        {

        }

        void Listener_OnConnected(bool syncRequired)
        {
            if (syncRequired)
            {
                SubscribeNotificationsAndFetchGroups();
            }
        }

        private FizzGroupModel GetGroup(string id)
        {
            if (groupLookup.ContainsKey(id))
                return groupLookup[id];

            return null;
        }

        public void UpdateGroup(string groupId, Action<FizzException> cb)
        {
            if (Client.State == FizzClientState.Closed)
            {
                FizzLogger.W("FizzClient should be opened before joining group.");
                return;
            }

            Client.Chat.Users.UpdateGroup(UserId, groupId, null, null, ex =>
            {
                if (ex == null)
                {
                    GroupInvites.Remove(groupId);
                    groupLookup[groupId].Channel.SubscribeAndQueryLatest();
                }
                FizzUtils.DoCallback(ex, cb);
            });
        }

        public void RemoveGroup(string groupId, Action<FizzException> cb)
        {
            if (Client.State == FizzClientState.Closed)
            {
                FizzLogger.W("FizzClient should be opened before removing group.");
                return;
            }

            Client.Chat.Users.RemoveGroup(UserId, groupId, ex =>
            {
                FizzUtils.DoCallback(ex, cb);
            });
        }

        private void SubscribeNotificationsAndFetchGroups()
        {
            if (Client.State == FizzClientState.Closed)
            {
                FizzLogger.W("FizzClient should be opened before subscribing user.");
                return;
            }

            Client.Chat.UserNotifications.Subscribe(ex =>
            {
                if (ex == null)
                {
                    if (ex == null)
                    {
                        IFizzFetchUserGroupsQuery groupFetchQuery = Client.Chat.Users.BuildFetchUserGroupsQuery(UserId);
                        FetchUserGroups(groupFetchQuery, new List<IFizzUserGroup>());
                    }
                }

            });
        }

        private void FetchUserGroups(IFizzFetchUserGroupsQuery groupFetchQuery, List<IFizzUserGroup> userGroups)
        {
            if (groupFetchQuery.HasNext)
            {
                groupFetchQuery.Next((groupList, ex) =>
                {
                    if (ex == null)
                    {
                        foreach (IFizzUserGroup group in groupList)
                        {
                            if (group.State == FizzGroupMemberState.Pending)
                            {
                                GroupInvites.Add(group.GroupId, group);
                            }
                            userGroups.Add(group);
                        }
                        FetchUserGroups(groupFetchQuery, userGroups);
                    }
                });
            }
            else
            {
                FetchGroups(userGroups);
            }
        }

        private void FetchGroups(List<IFizzUserGroup> userGroups)
        {
            foreach (IFizzUserGroup userGroup in userGroups)
            {
                Client.Chat.Groups.FetchGroup(userGroup.GroupId, (group, ex) =>
                {
                    if (ex == null)
                    {
                        AddGroup(new FizzGroupModel(group, GroupTag));
                    }
                });
            }
        }

        private void AddGroup(FizzGroupModel group)
        {
            if (Client.State == FizzClientState.Closed)
            {
                FizzLogger.W("FizzClient should be opened before adding group.");
                return;
            }

            if (group == null)
            {
                FizzLogger.E("FizzClient unable to add group, group is null.");
                return;
            }

            if (!groupLookup.ContainsKey(group.Id))
            {
                Groups.Add(group);
                groupLookup.Add(group.Id, group);
            }

            group = groupLookup[group.Id];
            group.Channel.SubscribeAndQueryLatest();
        }

        private IFizzUserGroup CreateUserGroup(FizzGroupMemberEventData eventData)
        {
            JSONClass userGroupJson = new JSONClass
        {
            { FizzJsonUserGroup.KEY_GROUP_ID, eventData.GroupId },
            { FizzJsonUserGroup.KEY_ROLE, eventData.Role.ToString() },
            { FizzJsonUserGroup.KEY_STATE, eventData.State.ToString() },
            { FizzJsonUserGroup.KEY_CREATED, Utils.GetCurrentUnixTimeStamp() }
        };

            return new FizzJsonUserGroup(userGroupJson.ToString());
        }
    }
}
