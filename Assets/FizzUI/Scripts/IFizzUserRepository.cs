using System;
using Fizz.UI.Model;

namespace Fizz
{
    public interface IFizzUserRepository
    {
        void GetUser(string userId, Action<FizzUserModel, FizzException> cb);
        Action<FizzUserModel> OnUserUpdated { get; set; }
    }
}
