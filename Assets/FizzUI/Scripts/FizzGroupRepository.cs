using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fizz;
using Fizz.Chat;
using Fizz.Common;
using Fizz.UI.Model;

public class FizzGroupRepository : IFizzGroupRepository
{
    private Dictionary<string, FizzGroup> groupLookup;
    public List<FizzGroup> Groups { get; private set; }
    public Dictionary<string, string> GroupInvites { get; private set; }

    #region Events
    public Action<FizzGroup> OnGroupAdded { get; set; }
    public Action<FizzGroup> OnGroupUpdated { get; set; }
    public Action<FizzGroup> OnGroupRemoved { get; set; }
    public Action<FizzGroup> OnGroupMembersUpdated { get; set; }
    #endregion

    public FizzGroupRepository()
    {
        Groups = new List<FizzGroup>();
        GroupInvites = new Dictionary<string, string>();
        groupLookup = new Dictionary<string, FizzGroup>();
    }

    public void Open()
    {
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
            FizzService.Instance.Client.Chat.Listener.OnConnected += Listener_OnConnected;
            FizzService.Instance.Client.Chat.Listener.OnDisconnected += Listener_OnDisconnected;
            FizzService.Instance.Client.Chat.Listener.OnMessageUpdated += Listener_OnMessageUpdated;
            FizzService.Instance.Client.Chat.Listener.OnMessageDeleted += Listener_OnMessageDeleted;
            FizzService.Instance.Client.Chat.Listener.OnMessagePublished += Listener_OnMessagePublished;
            FizzService.Instance.Client.Chat.GroupListener.OnGroupUpdated += Listener_OnGroupUpdated;
            FizzService.Instance.Client.Chat.GroupListener.OnGroupMemberAdded += Listener_OnGroupMemberAdded;
            FizzService.Instance.Client.Chat.GroupListener.OnGroupMemberRemoved += Listener_OnGroupMemberRemoved;
            FizzService.Instance.Client.Chat.GroupListener.OnGroupMemberUpdated += Listener_OnGroupMemberUpdated;
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
            FizzService.Instance.Client.Chat.Listener.OnConnected -= Listener_OnConnected;
            FizzService.Instance.Client.Chat.Listener.OnDisconnected -= Listener_OnDisconnected;
            FizzService.Instance.Client.Chat.Listener.OnMessageUpdated -= Listener_OnMessageUpdated;
            FizzService.Instance.Client.Chat.Listener.OnMessageDeleted -= Listener_OnMessageDeleted;
            FizzService.Instance.Client.Chat.Listener.OnMessagePublished -= Listener_OnMessagePublished;
            FizzService.Instance.Client.Chat.GroupListener.OnGroupUpdated -= Listener_OnGroupUpdated;
            FizzService.Instance.Client.Chat.GroupListener.OnGroupMemberAdded -= Listener_OnGroupMemberAdded;
            FizzService.Instance.Client.Chat.GroupListener.OnGroupMemberRemoved -= Listener_OnGroupMemberRemoved;
            FizzService.Instance.Client.Chat.GroupListener.OnGroupMemberUpdated -= Listener_OnGroupMemberUpdated;
        }
        catch (FizzException ex)
        {
            FizzLogger.E("Unable to unbind group listeners. " + ex.Message);
        }
    }

    private FizzChannel GetChannel(string id)
    {
        foreach (FizzGroup group in Groups)
        {
            if (group.Channel.Id.Equals(id))
                return group.Channel;
        }

        return null;
    }


    void Listener_OnMessagePublished(FizzChannelMessage msg)
    {
        FizzChannel channel = GetChannel(msg.To);
        if (channel != null)
        {
            channel.AddMessage(msg);

            if (FizzService.Instance.OnChannelMessagePublish != null)
            {
                FizzService.Instance.OnChannelMessagePublish.Invoke(msg.To, msg);
            }
        }
    }

    void Listener_OnMessageDeleted(FizzChannelMessage msg)
    {
        FizzChannel channel = GetChannel(msg.To);
        if (channel != null)
        {
            channel.RemoveMessage(msg);

            if (FizzService.Instance.OnChannelMessageDelete != null)
            {
                FizzService.Instance.OnChannelMessageDelete.Invoke(msg.To, msg);
            }
        }
    }

    void Listener_OnMessageUpdated(FizzChannelMessage msg)
    {
        FizzChannel channel = GetChannel(msg.To);
        if (channel != null)
        {
            channel.UpdateMessage(msg);

            if (FizzService.Instance.OnChannelMessageUpdate != null)
            {
                FizzService.Instance.OnChannelMessageUpdate.Invoke(msg.To, msg);
            }
        }
    }

    void Listener_OnGroupUpdated(FizzGroupUpdateEventData eventData)
    {
        FizzGroup group = GetGroup(eventData.GroupId);
        if (group != null && OnGroupUpdated != null)
        {
            OnGroupUpdated.Invoke(group);
        }
    }

    void Listener_OnGroupMemberAdded(FizzGroupMemberEventData eventData)
    {
        if (FizzService.Instance.UserId.Equals(eventData.MemberId))
        {
            FizzService.Instance.Client.Chat.Groups.FetchGroup(eventData.GroupId, (groupMeta, ex) =>
            {
                if (ex == null)
                {
                    AddGroup(new FizzGroup(groupMeta));
                    FizzGroup group = GetGroup(groupMeta.Id);
                    if (eventData.State == FizzGroupMemberState.Pending)
                    {
                        GroupInvites.Add(group.Id, group.Id);
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
            FizzGroup group = GetGroup(eventData.GroupId);
            if (group != null && OnGroupMembersUpdated != null)
            {
                OnGroupMembersUpdated.Invoke(group);
            }
        }
    }

    void Listener_OnGroupMemberRemoved(FizzGroupMemberEventData eventData)
    {
        if (FizzService.Instance.UserId.Equals(eventData.MemberId))
        {
            FizzGroup group = GetGroup(eventData.GroupId);
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
            FizzGroup group = GetGroup(eventData.GroupId);
            if (group != null && OnGroupMembersUpdated != null)
            {
                OnGroupMembersUpdated.Invoke(group);
            }
        }
    }

    void Listener_OnGroupMemberUpdated(FizzGroupMemberEventData eventData)
    {
        FizzGroup group = GetGroup(eventData.GroupId);
        if (group != null)
        {
            if (eventData.MemberId.Equals(FizzService.Instance.UserId) && eventData.State == FizzGroupMemberState.Joined)
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

    public FizzGroup GetGroup(string id)
    {
        if (groupLookup.ContainsKey(id))
            return groupLookup[id];

        return null;
    }

    public void JoinGroup(string groupId, Action<FizzException> cb)
    {
        if (FizzService.Instance.Client.State == FizzClientState.Closed)
        {
            FizzLogger.W("FizzClient should be opened before joining group.");
            return;
        }

        FizzService.Instance.Client.Chat.Users.JoinGroup(FizzService.Instance.UserId, groupId, ex =>
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
        if (FizzService.Instance.Client.State == FizzClientState.Closed)
        {
            FizzLogger.W("FizzClient should be opened before removing group.");
            return;
        }

        FizzService.Instance.Client.Chat.Users.RemoveGroup(FizzService.Instance.UserId, groupId, ex =>
        {
            FizzUtils.DoCallback(ex, cb);
        });
    }

    private void SubscribeNotificationsAndFetchGroups()
    {
        FizzService.Instance.SubscribeUserNotifications(ex =>
        {
            if (ex == null)
            {
                IFizzFetchUserGroupsQuery groupFetchQuery = FizzService.Instance.Client.Chat.Users.BuildFetchUserGroupsQuery(FizzService.Instance.UserId);
                FetchUserGroups(groupFetchQuery, new List<IFizzUserGroup>());
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
                            GroupInvites.Add(group.GroupId, group.GroupId);
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
            FizzService.Instance.Client.Chat.Groups.FetchGroup(userGroup.GroupId, (group, ex) =>
            {
                if (ex == null)
                {
                    AddGroup(new FizzGroup(group));
                }
            });
        }
    }

    private void AddGroup(FizzGroup group)
    {
        if (FizzService.Instance.Client.State == FizzClientState.Closed)
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
}
