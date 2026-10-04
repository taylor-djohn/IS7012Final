namespace IS7012Final.Model
{
    public class Movie
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Director { get; set; }
        public int ReleaseYear { get; set; }
        public string? Genre { get; set; }
     
        public List<Genre>? Genres { get; set; } = new List<Genre>();
        public List<Review>? Reviews { get; set; } = new List<Review>();

        public List<Watchlist>? Watchlists { get; set; } = new List<Watchlist>();
    }
}
