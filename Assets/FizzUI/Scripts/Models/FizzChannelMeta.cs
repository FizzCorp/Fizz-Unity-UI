namespace Fizz.UI.Model
{
    public class FizzChannelMeta
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public bool PersistMessages { get; set; } = true;

        public bool FilterContent { get; set; } = true;

        public bool Readonly { get; set; } = false;

        public int InitialQueryMessageCount { get; set; } = 50;

        public int HistoryQueryMessageCount { get; set; } = 50;

        public FizzChannelMeta () { }

        public FizzChannelMeta(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public FizzChannelMeta (string id, string name, bool persist, int initialMessages, int historyMessages)
        {
            Id = id;
            Name = name;
            PersistMessages = persist;
            InitialQueryMessageCount = initialMessages;
            HistoryQueryMessageCount = historyMessages;
        }
    }
}