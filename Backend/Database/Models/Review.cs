using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Review
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
    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(20)]
    public string Title { get; set; }

    [MaxLength(500)]
    public string Comments { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    public Review()
    {
        CreatedAt = DateTime.UtcNow;
    }
}
