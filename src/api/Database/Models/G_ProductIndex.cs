// (c) 2024 @Maxylan
// Scaffolded, then altered to suit my needs. @see ../scaffold.txt
namespace Homie.Database.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// The configuration for the 'ProductIndex' entity, reflects `g_product_indexes` table in the database.
/// </summary>
[Table("g_product_indexes")]
[Index("ProductId", Name = "product_id")]
public partial record ProductIndex : IBaseModel<ProductIndex>
{
    /// <summary>PK</summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public uint? Id { get; set; }

    [Column("product_id")]
    public uint ProductId { get; set; }

    [Column("type", TypeName = "enum('category','super','bf','tag')")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProductIndexType Type { get; set; }

    [Column("name")]
    [StringLength(63)]
    public string Name { get; set; } = null!;

    [Column("value")]
    [StringLength(255)]
    public string? Value { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("ProductIndices")]
    public virtual Product Product { get; set; } = null!;

    /// <summary>
    /// The configuration for the 'ProductIndex' entity, reflects `g_product_indexes` table in the database.
    /// </summary>
    /// <returns></returns>
    public static Action<EntityTypeBuilder<ProductIndex>> Configuration() => (
        entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Type)
                .HasConversion<string>(
                    v => v.ToString(),
                    v => (ProductIndexType)Enum.Parse(typeof(ProductIndexType), v)
                );

            entity.HasOne(d => d.Product).WithMany(p => p.ProductIndices).HasConstraintName("g_product_indexes_ibfk_1");
        }
    );

    /// <summary>
    /// Convert the '<see cref="ProductIndex"/>' entity to a '<see cref="ProductIndexDTO"/>' instance.
    /// </summary>
    /// <returns><see cref="ProductIndexDTO"/></returns>
    public object ToDataTransferObject() => (
        new
        {
            Id,
            ProductId,
            Type,
            Name,
            Value
        }
    );
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProductIndexType
{
    Category,
    Super,
    Bf,
    Tag
}