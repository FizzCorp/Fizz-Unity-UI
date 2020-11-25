using Fizz.Chat;

namespace Fizz.UI.Model
{
    public class FizzGroupModel
    {
        public string Id { get; private set; }
        public string ChannelId { get; private set; }
        public string CreatedBy { get; private set; }
        public string Title { get; private set; }
        public string ImageURL { get; private set; }
        public string Description { get; private set; }
        public string Type { get; private set; }
        public long Created { get; private set; }
        public long Updated { get; private set; }

        public FizzGroupChannelModel Channel { get; private set; }
       
        public FizzGroupModel(IFizzGroup groupMeta, string groupUITag)
        {
            Id = groupMeta.Id;
            ChannelId = groupMeta.ChannelId;
            CreatedBy = groupMeta.CreatedBy;
            Title = groupMeta.Title;
            ImageURL = groupMeta.Description;
            Description = groupMeta.Description;
            Type = groupMeta.Description;
            Created = groupMeta.Created;
            Updated = groupMeta.Updated;

            Channel = new FizzGroupChannelModel(groupMeta.Id, new FizzChannelMeta(groupMeta.ChannelId, groupMeta.Title, groupUITag));
        }

        public void Update(FizzGroupUpdateEventData data)
        {
            Title = data.Title;
            Description = data.Description;
            ImageURL = data.ImageURL;
            Type = data.Type;

            Channel.Meta.Name = data.Title;
        }
    }
}
