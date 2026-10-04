namespace IS7012Final.Model
{
    public class Genre
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<Movie>? Movie { get; set; } = new List<Movie>();
    }
}
