namespace CartDomain.Models;

public class CartItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string Name { get; set; } = string.Empty;
    // Można tu w przyszłości dodać cenę, jeśli byłaby potrzebna
    // public decimal Price { get; set; } 
}