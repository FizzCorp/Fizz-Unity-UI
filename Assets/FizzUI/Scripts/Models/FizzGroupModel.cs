using System;
using System.Collections.Generic;
using System.Linq;
using Fizz.Chat;
using Fizz.Common;

namespace Fizz.UI.Model
{
    public class FizzGroupModel
    {
        public string Id { get { return Meta.Id; } }
        public string Title { get { return Meta.Title; } }

        public IFizzGroup Meta { get; private set; }

        public FizzGroupChannelModel Channel { get; private set; }
       
        public FizzGroupModel(IFizzGroup groupMeta, string groupUITag)
        {
            Meta = groupMeta;
            Channel = new FizzGroupChannelModel(groupMeta.Id, new FizzChannelMeta(groupMeta.ChannelId, groupMeta.Title, groupUITag));
        }
    }
}
