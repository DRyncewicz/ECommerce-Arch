namespace ECommerce.ProductService.Domain;

/// <summary>
/// EAV-style attribute stored as a sub-document in MongoDB.
/// Key = attribute name (e.g. "Color", "Weight")
/// Value = string representation (e.g. "Red", "500")
/// Unit = optional unit of measure (e.g. "g", "cm")
/// </summary>
public sealed record ProductAttribute(string Key, string Value, string? Unit = null);
