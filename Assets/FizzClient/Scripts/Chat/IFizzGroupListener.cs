using System;

namespace Fizz.Chat {
    public interface IFizzGroupListener
    {
        Action<FizzGroupUpdate> OnGroupUpdated { get; set; }
        Action<IFizzGroupMember> OnGroupMemberAdded { get; set; }
        Action<IFizzGroupMember> OnGroupMemberRemoved { get; set; }
        Action<IFizzGroupMember> OnGroupMemberUpdated { get; set; }
    }
}