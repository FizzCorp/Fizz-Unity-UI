using System;
using System.Collections.Generic;
using System.Linq;
using Fizz.Chat;
using Fizz.Common;

namespace Fizz.UI.Model
{
    public class FizzGroup
    {
        public string Id { get { return Meta.Id; } }
        public string Title { get { return Meta.Title; } }

        public IFizzGroup Meta { get; private set; }

        public FizzGroupChannel Channel { get; private set; }
       
        public FizzGroup(IFizzGroup groupMeta)
        {
            Meta = groupMeta;
            Channel = new FizzGroupChannel(groupMeta.Id, new FizzChannelMeta(groupMeta.ChannelId, groupMeta.Title, "Groups"));
        }
    }
}
