namespace CartDomain.Models;

public class CartItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string Name { get; set; } = string.Empty;
}