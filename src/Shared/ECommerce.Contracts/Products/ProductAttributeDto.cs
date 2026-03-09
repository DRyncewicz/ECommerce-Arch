namespace ECommerce.Contracts.Products;

public sealed record ProductAttributeDto(string Key, string Value, string? Unit = null);
