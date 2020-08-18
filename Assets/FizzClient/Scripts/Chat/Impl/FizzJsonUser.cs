using Fizz.Common;
using Fizz.Common.Json;

namespace Fizz.Chat.Impl
{
    public class FizzJsonUser : IFizzUser
    {
        private static readonly string KEY_ONLINE = "is_online";

        public FizzJsonUser(string userId, string json) : this(userId, JSONNode.Parse(json))
        {
        }

        public FizzJsonUser(string userId, JSONNode json)
        {
            Id = userId;
            Online = json[KEY_ONLINE].AsBool;
        }

        public string Id { get; private set; }
        public bool Online { get; private set; }
    }
}