using System;
using Fizz;
using Fizz.UI.Model;

public interface IFizzUserRepository
{
    void GetUser(string userId, Action<FizzUser, FizzException> cb);
    Action<FizzUser> OnUserUpdated { get; set; }
}
