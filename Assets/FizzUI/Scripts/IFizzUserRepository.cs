using System;
using Fizz;
using Fizz.UI.Model;

public interface IFizzUserRepository
{
    void GetUser(string userId, Action<FizzUserModel, FizzException> cb);
    Action<FizzUserModel> OnUserUpdated { get; set; }
}
