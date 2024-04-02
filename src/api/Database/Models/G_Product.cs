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
/// The 'Product' entity, reflects `g_products` table in the database.
/// </summary>
[Table("g_products")]
[Index("ChangedBy", Name = "changed_by")]
[Index("ChangedByPlatform", Name = "changed_by_platform")]
[Index("CoverSd", Name = "cover_sd")]
public partial record Product : IBaseModel<Product>
{
    /// <summary>PK</summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public uint? Id { get; set; }

    [Column("pid")]
    [StringLength(63)]
    public string Pid { get; set; } = null!;

    [Column("store", TypeName = "enum('citygross','ica','lidl','hemkop')")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Stores Store { get; set; }

    [Column("title")]
    [StringLength(127)]
    public string Title { get; set; } = null!;

    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Column("brand")]
    [StringLength(63)]
    public string? Brand { get; set; }

    [Column("source")]
    [StringLength(255)]
    public string? Source { get; set; }

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    /// <summary>
    /// (downscaled) attachment_id (ON DELETE SET null)
    /// </summary>
    [Column("cover_sd")]
    public uint? CoverSd { get; set; }

    [Column("has_price")]
    public bool HasPrice { get; set; }

    /// <summary>
    /// (has_price)
    /// </summary>
    [Column("price")]
    [Precision(10, 2)]
    public decimal? Price { get; set; }

    [Column("has_discount")]
    public bool HasDiscount { get; set; }

    /// <summary>
    /// (has_discount)
    /// </summary>
    [Column("discount")]
    [Precision(10, 2)]
    public decimal? Discount { get; set; }

    /// <summary>
    /// (has_discount)
    /// </summary>
    [Column("discount_start", TypeName = "datetime")]
    public DateTime? DiscountStart { get; set; } = null!;

    /// <summary>
    /// (has_discount)
    /// </summary>
    [Column("discount_end", TypeName = "datetime")]
    public DateTime? DiscountEnd { get; set; } = null!;

    [Column("has_amount")]
    public bool HasAmount { get; set; }

    /// <summary>
    /// (has_amount)
    /// </summary>
    [Column("amount")]
    [Precision(8, 4)]
    public decimal? Amount { get; set; }

    [Column("has_meassurements")]
    public bool HasMeassurements { get; set; }

    /// <summary>
    /// (has_meassurements)
    /// </summary>
    [Column("meassurements")]
    [StringLength(63)]
    public string? Meassurements { get; set; }

    [Column("has_weight")]
    public bool HasWeight { get; set; }

    /// <summary>
    /// (has_weight)
    /// </summary>
    [Column("weight")]
    [Precision(8, 4)]
    public decimal? Weight { get; set; }

    /// <summary>
    /// (has_weight)
    /// </summary>
    [Column("unit", TypeName = "enum('kg','hg','g')")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Units? Unit { get; set; } = Units.Kg;

    [Column("created", TypeName = "datetime")]
    public DateTime Created { get; set; } = DateTime.Now;

    [Column("changed", TypeName = "datetime")]
    public DateTime Changed { get; set; } = DateTime.Now;

    /// <summary>
    /// user id (ON DELETE SET null)
    /// </summary>
    [Column("changed_by")]
    public uint? ChangedBy { get; set; }

    /// <summary>
    /// platform_id (ON DELETE SET null)
    /// </summary>
    [Column("changed_by_platform")]
    public uint? ChangedByPlatform { get; set; }

    [ForeignKey("ChangedBy")]
    [InverseProperty("Products")]
    public virtual User? ChangedByUser { get; set; }

    [ForeignKey("ChangedByPlatform")]
    [InverseProperty("Products")]
    public virtual Platform? ChangedByPlatformNavigation { get; set; }

    [ForeignKey("CoverSd")]
    [InverseProperty("Products")]
    public virtual Attachment? CoverSdAttachment { get; set; }

    [InverseProperty("Product")]
    public virtual ICollection<ProductIndex> ProductIndices { get; set; } = new List<ProductIndex>();

    [InverseProperty("Product")]
    public virtual ICollection<Row> Rows { get; set; } = new List<Row>();

    /// <summary>
    /// The configuration for the 'Product' entity, reflects `g_products` table in the database.
    /// </summary>
    /// <returns></returns>
    public static Action<EntityTypeBuilder<Product>> Configuration() => (
        entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Store)
                .HasConversion<string>(
                    v => v.ToString(),
                    v => (Stores)Enum.Parse(typeof(Stores), v)
                );

            entity.Property(e => e.Amount).HasComment("(has_amount)");
            entity.Property(e => e.Changed).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ChangedBy).HasComment("user id (ON DELETE SET null)");
            entity.Property(e => e.ChangedByPlatform).HasComment("platform_id (ON DELETE SET null)");
            entity.Property(e => e.CoverSd).HasComment("(downscaled) attachment_id (ON DELETE SET null)");
            entity.Property(e => e.Created).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Discount).HasComment("(has_discount)");
            entity.Property(e => e.DiscountEnd).HasComment("(has_discount)");
            entity.Property(e => e.DiscountStart).HasComment("(has_discount)");
            entity.Property(e => e.Meassurements).HasComment("(has_meassurements)");
            entity.Property(e => e.Price).HasComment("(has_price)");
            entity.Property(e => e.Unit)
                .HasDefaultValueSql("'kg'")
                .HasComment("(has_weight)")
                .HasConversion<string?>(
                    v => v != null ? v.ToString() : "kg",
                    v => (Units)Enum.Parse(typeof(Units), v ?? "kg")
                );

            entity.Property(e => e.Weight).HasComment("(has_weight)");

            entity.HasOne(d => d.ChangedByUser).WithMany(p => p.Products)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("g_products_ibfk_2");

            entity.HasOne(d => d.ChangedByPlatformNavigation).WithMany(p => p.Products)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("g_products_ibfk_3");

            entity.HasOne(d => d.CoverSdAttachment).WithMany(p => p.Products)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("g_products_ibfk_1");
        }
    );

    /// <summary>
    /// Convert the '<see cref="Product"/>' entity to a '<see cref="ProductDTO"/>' instance.
    /// </summary>
    /// <returns><see cref="ProductDTO"/></returns>
    public object ToDataTransferObject() => (
        new
        {
            Id,
            Pid,
            Store,
            Title,
            Name,
            Brand,
            Source,
            Description,
            CoverSd,
            HasPrice,
            Price,
            HasDiscount,
            Discount,
            DiscountStart,
            DiscountEnd,
            HasAmount,
            Amount,
            HasMeassurements,
            Meassurements,
            HasWeight,
            Weight,
            Unit,
            Created,
            Changed,
            ChangedBy,
            ChangedByPlatform
        }
    );
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Stores
{
    Citygross,
    Ica,
    Lidl,
    Hemkop
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Units
{
    Kg,
    Hg,
    g
}