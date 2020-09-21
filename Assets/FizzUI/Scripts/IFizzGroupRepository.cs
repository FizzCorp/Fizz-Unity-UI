using System;
using Fizz;
using Fizz.Chat;
using Fizz.UI.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFizzGroupRepository
{
    List<FizzGroup> Groups { get; }
    Dictionary<string, string> GroupInvites { get; }

    FizzGroup GetGroup(string id);
    void JoinGroup(string groupId, Action<FizzException> cb);
    void RemoveGroup(string groupId, Action<FizzException> cb);

    Action<FizzGroup> OnGroupAdded { get; set; }
    Action<FizzGroup> OnGroupUpdated { get; set; }
    Action<FizzGroup> OnGroupRemoved { get; set; }
    Action<FizzGroup> OnGroupMembersUpdated { get; set; }


}
