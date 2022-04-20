using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PutAway.Shared.Entities;

public class Item
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal? Price { get; set; }
    public string? Description { get; set; }
    public DateTime? BuyDate { get; set; }
    public int? YearsOfWarranty { get; set; }
    public ISet<Item> Items { get; set; } = new HashSet<Item>();
    public ISet<Image> Images { get; set; } = new HashSet<Image>();
}