using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IS7012Final.Model
{
    public class Review
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int MovieId { get; set; }
        [Range(1, 5)]
        public int Rating { get; set; }
        [Display(Name = "Review")]
        public string? ReviewText { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        // Navigation property to the related Movie
        public Movie? Movie { get; set; }
    }
}