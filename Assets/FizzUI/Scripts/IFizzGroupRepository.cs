using System;
using Fizz.Chat;
using Fizz.UI.Model;
using System.Collections.Generic;

namespace Fizz
{
    public interface IFizzGroupRepository
    {
        List<FizzGroupModel> Groups { get; }
        Dictionary<string, IFizzUserGroup> GroupInvites { get; }

        void UpdateGroup(string groupId, Action<FizzException> cb);
        void RemoveGroup(string groupId, Action<FizzException> cb);

        Action<FizzGroupModel> OnGroupAdded { get; set; }
        Action<FizzGroupModel> OnGroupUpdated { get; set; }
        Action<FizzGroupModel> OnGroupRemoved { get; set; }
        Action<FizzGroupModel> OnGroupMembersUpdated { get; set; }
    }
}
