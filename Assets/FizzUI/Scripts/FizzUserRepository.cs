using System;
using System.Collections.Generic;
using Fizz;
using Fizz.Chat;
using Fizz.Common;
using Fizz.UI.Model;

public class FizzUserRepository : IFizzUserRepository
{
    private IFizzClient Client { get; set; }
    private Dictionary<string, FizzUser> Users { get; set; }

    #region Events
    public Action<FizzUser> OnUserUpdated { get; set; }
    #endregion

    public FizzUserRepository(IFizzClient client)
    {
        Client = client;
        Users = new Dictionary<string, FizzUser>();
    }

    public void Open(string userId)
    {
        AddInternalListeners();
    }

    public void Close()
    {
        Users.Clear();
        RemoveInternalListeners();
    }

    void AddInternalListeners()
    {
        try
        {
            Client.Chat.Listener.OnConnected += Listener_OnConnected;
            Client.Chat.Listener.OnDisconnected += Listener_OnDisconnected;
            Client.Chat.UserListener.OnUserUpdated += Listener_OnUserUpdated;
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
            Client.Chat.UserListener.OnUserUpdated -= Listener_OnUserUpdated;
        }
        catch (FizzException ex)
        {
            FizzLogger.E("Unable to unbind group listeners. " + ex.Message);
        }
    }

    void Listener_OnUserUpdated(FizzUserUpdateEventData eventData)
    {
        if (OnUserUpdated != null)
        {
            FizzUser user = Users[eventData.UserId];
            if (user != null)
            {
                user.Update(eventData);
                OnUserUpdated.Invoke(user);
            }
        }
    }

    void Listener_OnDisconnected(FizzException obj)
    {

    }

    void Listener_OnConnected(bool syncRequired)
    {
        if (syncRequired)
        {
            FetchProfilesAndSubscibe();
        }
    }

    public void GetUser(string userId, Action<FizzUser, FizzException> cb)
    {
        if (Client.State == FizzClientState.Closed)
        {
            FizzLogger.W("FizzClient should be opened before fetching user.");
            return;
        }

        if (userId == null)
        {
            FizzLogger.E("FizzClient unable to fetch, userId is null.");
            return;
        }

        Client.Chat.Users.GetUser(userId, (userMeta, ex) =>
        {
            FizzUser user = null;
            if (ex == null)
            {
                user = new FizzUser(userMeta, Client);
                Users.Add(user.Id, user);
            }
            FizzUtils.DoCallback(user, ex, cb);
        });
    }

    private void FetchProfilesAndSubscibe()
    {
        if (Client.State == FizzClientState.Closed)
        {
            FizzLogger.W("FizzClient should be opened before subscribing user.");
            return;
        }
    }

}
