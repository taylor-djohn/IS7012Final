namespace IS7012Final.Model
{
    public class Watchlist
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        public int MovieId { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.Now;

        // Navigation property
        public Movie? Movie { get; set; }
    }
}