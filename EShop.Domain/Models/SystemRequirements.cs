namespace EShop.Domain.Models;
using Microsoft.EntityFrameworkCore;

public class SystemRequirements
{
    public string OS { get; set; } = string.Empty;
    public string Processor { get; set; } = string.Empty;
    public string Memory { get; set; } = string.Empty;
    public string Graphics { get; set; } = string.Empty;
    public string Storage { get; set; } = string.Empty;
}
