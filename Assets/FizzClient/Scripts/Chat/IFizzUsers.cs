using System;
using System.Collections.Generic;

namespace Fizz.Chat 
{
    public interface IFizzUser
    {
        string Id { get; }
        bool Online { get; }
    }

    public interface IFizzUsers 
    {
        void GetUser(string userId, Action<IFizzUser, FizzException> callback);
        void Subscribe(string userId, Action<FizzException> callback);
        void Unsubscribe(string userId, Action<FizzException> callback);
    }
}