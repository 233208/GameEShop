namespace CartDomain.Models;

public class Cart
{
    public string UserId { get; set; } 
    public List<CartItem> Items { get; set; } = new();


    public Cart() { }

    public Cart(string userId)
    {
        UserId = userId;
    }

    public void AddItem(int productId, int quantity, string name)
    {
        var existingItem = Items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            Items.Add(new CartItem { ProductId = productId, Quantity = quantity, Name = name });
        }
    }

    public void RemoveItem(int productId)
    {
        Items.RemoveAll(i => i.ProductId == productId);
    }

    public void Clear()
    {
        Items.Clear();
    }
}