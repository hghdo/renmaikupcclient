using System;
namespace RenmeiLib
{
    interface IMessage
    {
        DateTime? DateCreated { get; set; }
        bool Equals(Tweet other);
        double Id { get; set; }
        int Index { get; set; }
        bool IsNew { get; set; }
        bool IsSearchResult { get; set; }
        event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        string RelativeTime { get; set; }
        string Source { get; set; }
        string Text { get; set; }
        void UpdateRelativeTime();
        User User { get; set; }
    }
}
