namespace CartDomain.Models;

public class Product
{
    public int Id { get; set; }
    public string GameName { get; set; } = string.Empty;
    public int CartId { get; set; }
    public int Quantity { get; set; }
}
