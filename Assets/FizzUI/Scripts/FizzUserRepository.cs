﻿using System;
using System.Collections.Generic;
using Fizz;
using Fizz.Chat;
using Fizz.Common;
using Fizz.UI.Model;

public class FizzUserRepository : IFizzUserRepository
{
    private IFizzClient Client { get; set; }
    private Dictionary<string, FizzUser> Users { get; set; }

    private Queue<GetUserRequest> requestQueue = new Queue<GetUserRequest>();

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

        requestQueue.Enqueue(new GetUserRequest(userId, cb));
        if (requestQueue.Count > 1)
        {
            return;
        }
        GetUser(requestQueue.Peek());
    }


    private void GetUser(GetUserRequest request)
    {
        Client.Chat.Users.GetUser(request.UserId, (userMeta, ex) =>
        {
            FizzUser user = null;
            if (ex == null)
            {
                user = new FizzUser(userMeta, Client);
                
                Users[user.Id] = user;
            }
            FizzUtils.DoCallback(user, ex, request.Callback);
            requestQueue.Dequeue();
            if (requestQueue.Count > 0)
            {
                GetUser(requestQueue.Peek());
            }
        });
    }

    private void FetchProfilesAndSubscibe()
    {
        if (Client.State == FizzClientState.Closed)
        {
            FizzLogger.W("FizzClient should be opened before subscribing user.");
            return;
        }

        foreach (KeyValuePair<string, FizzUser> entry in Users)
        {
            GetUser(entry.Key, (user, ex) =>
            {
                if (ex == null)
                {
                    FizzUser existingUser = entry.Value;
                    existingUser.Apply(user);
                    if (existingUser.IsSubscribed)
                    {
                        existingUser.Subscribe(null);
                    }
                }
            });
        }
    }

    private class GetUserRequest
    {
        public string UserId { get; private set; }
        public Action<FizzUser, FizzException> Callback { get; private set; }

        public GetUserRequest(string userId, Action<FizzUser, FizzException> cb)
        {
            UserId = userId;
            Callback = cb;
        }
    }

}
