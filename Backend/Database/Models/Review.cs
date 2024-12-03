using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum ReviewType
{
    Review = 1,       // For evaluations
    Recommendation = 2 // For endorsements
}

public class Review : IAccountable
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ReviewerId { get; set; }

    [ForeignKey(nameof(Review.ReviewerId))]
    public User Reviewer { get; set; }

    [Required]
    public int RevieweeId { get; set; }

    [ForeignKey(nameof(Review.RevieweeId))]
    public User Reviewee { get; set; }

    [Required]
    public ReviewType Type { get; set; } // Differentiates Review vs Recommendation

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(20)]
    public string Title { get; set; }

    [MaxLength(500)]
    public string Comments { get; set; }

    public bool Active { get ; set; }
    public DateTime CreatedAt { get ; set; }
    public DateTime UpdatedAt { get ; set; }
    public DateTime? DeletedAt { get ; set; }

    public Review()
    {

    }
}
