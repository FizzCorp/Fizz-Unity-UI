using System;
using System.Collections.Generic;

namespace Fizz.Chat 
{
    public class FizzGroupUpdate 
    {
        public enum UpdateReason 
        {
            Profile,
            Unknown
        }

        public FizzGroupUpdate() 
        {
            Reason = UpdateReason.Unknown;
        }

        public UpdateReason Reason { get; set; }
        public string GroupId { get; set; }
        public string Title { get; set; }
        public string ImageURL { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
    }

    public enum FizzGroupMemberState 
    {
        Pending,
        Joined,
        Unknown
    }

    public enum FizzGroupMemberRole 
    {
        Moderator,
        Member,
        Unknown
    }

    public interface IFizzGroupMember 
    {
        string UserId { get; }
        string GroupId { get; }
        FizzGroupMemberState State { get; }
        FizzGroupMemberRole Role { get; }
    }
    
    public interface IFizzGroup
    {
        string Id { get; }
        string ChannelId { get; }
        string CreatedBy { get; }
        string Title { get; }
        string ImageURL { get; }
        string Description { get; }
        string Type { get; }
        long Created { get; }
        long Updated { get; }
    }

    public interface IFizzGroups 
    {
        void CreateGroup(
            string title, 
            string imageURL, 
            string description, 
            string type,
            IList<string> memberIds,
            Action<IFizzGroup, FizzException> callback
        );

        void FetchGroup(
            string groupId, 
            Action<IFizzGroup, FizzException> callback
        );

        void UpdateGroup(
            string groupId,
            string title, 
            string imageURL, 
            string description, 
            string type, 
            Action<FizzException> callback
        );

        void GetGroupMembers(
            string groupId, 
            Action<IList<IFizzGroupMember>, FizzException> callback
        );

        void AddGroupMembers(
            string grouId,
            IList<string> memberIds, 
            Action<FizzException> callback
        );

        void RemoveGroupMember(
            string groupId,
            string memberId, 
            Action<FizzException> callback
        );

        void PublishMessage(
            string groupId,
            string nick, 
            string body, 
            Dictionary<string, string> data, 
            bool translate, 
            bool filter, 
            bool persist, 
            Action<FizzException> callback
        );

        void QueryLatest(
            string channelId, 
            int count, 
            Action<IList<FizzChannelMessage>, FizzException> callback
        );

        void QueryLatest(
            string channelId, 
            int count, 
            long beforeId, 
            Action<IList<FizzChannelMessage>, FizzException> callback
        );
    }
}