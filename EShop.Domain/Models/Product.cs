using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShop.Domain.Models;

public class Product
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    [Range(1900, 2155, ErrorMessage = "Year must be between 1900 and 2155.")]
    public int Year { get; set; }
    public string[] Genres { get; set; } = Array.Empty<string>();
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Producer { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public SystemRequirements SystemRequirements { get; set; } = new SystemRequirements();
    [Range(0, 5)]
    public decimal Rating { get; set; }
    public bool Deleted { get; set; }
}

