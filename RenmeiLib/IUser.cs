namespace RenmeiLib
{
    interface IUser
    {
        string BackgroundColor { get; set; }
        string Description { get; set; }
        int FavoritesCount { get; set; }
        int FollowersCount { get; set; }
        int FollowingCount { get; set; }
        string FullName { get; }
        int Id { get; set; }
        string ImageUrl { get; set; }
        string LinkColor { get; set; }
        string Location { get; set; }
        string Name { get; set; }
        event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        string ScreenName { get; set; }
        string SidebarBorderColor { get; set; }
        string SidebarFillColor { get; set; }
        string SiteUrl { get; set; }
        int StatusesCount { get; set; }
        string TextColor { get; set; }
        Tweet Tweet { get; set; }
        string TwitterUrl { get; }
    }
}
