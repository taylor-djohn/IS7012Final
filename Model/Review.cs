using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IS7012Final.Model
{
    public class Review
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int MovieId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        // store the movie title for convenience; may be null if not set
        [Display(Name = "Title")]
        public string? MovieTitle { get; set; }
        [Range(1, 5)]
        public int Rating { get; set; }
        [Display(Name = "Review")]
        public string? ReviewText { get; set; }
        [Display(Name = "Date")]
        public DateTime Timestamp { get; set; } = DateTime.Now;
        // Navigation property to the related Movie
        public Movie? Movie { get; set; }
    }
}