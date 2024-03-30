using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homie.Database.Models;

[Table("items")]
[Index("Author", Name = "author")]
[Index("ChangedBy", Name = "changed_by")]
[Index("CoverSd", Name = "cover_sd")]
[Index("ListId", Name = "list_id")]
public partial record Item : IBaseModel<Item>
{
    [Key]
    [Column("id")]
    public uint Id { get; set; }

    /// <summary>
    /// list_id (ON DELETE CASCADE)
    /// </summary>
    [Column("list_id")]
    public uint ListId { get; set; }

    [Column("title")]
    [StringLength(127)]
    public string Title { get; set; } = null!;

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
    public DateTime? DiscountStart { get; set; }

    /// <summary>
    /// (has_discount)
    /// </summary>
    [Column("discount_end", TypeName = "datetime")]
    public DateTime? DiscountEnd { get; set; }

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
    public string? Unit { get; set; }

    /// <summary>
    /// user id (ON DELETE SET null)
    /// </summary>
    [Column("author")]
    public uint? Author { get; set; }

    [Column("created", TypeName = "datetime")]
    public DateTime Created { get; set; }

    [Column("changed", TypeName = "datetime")]
    public DateTime Changed { get; set; }

    /// <summary>
    /// user id (ON DELETE SET null)
    /// </summary>
    [Column("changed_by")]
    public uint? ChangedBy { get; set; }

    [ForeignKey("Author")]
    [InverseProperty("ItemAuthorNavigations")]
    public virtual User? AuthorNavigation { get; set; }

    [ForeignKey("ChangedBy")]
    [InverseProperty("ItemChangedByNavigations")]
    public virtual User? ChangedByNavigation { get; set; }

    [ForeignKey("CoverSd")]
    [InverseProperty("Items")]
    public virtual Attachment? CoverSdNavigation { get; set; }

    [ForeignKey("ListId")]
    [InverseProperty("Items")]
    public virtual List List { get; set; } = null!;

    [InverseProperty("Item")]
    public virtual ICollection<Row> Rows { get; set; } = new List<Row>();

    /// <summary>
    /// The configuration for the 'Item' entity, reflects `items` table in the database.
    /// </summary>
    /// <returns></returns>
    public static Action<EntityTypeBuilder<Item>> Configuration() => (
        entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Amount).HasComment("(has_amount)");
            entity.Property(e => e.Author).HasComment("user id (ON DELETE SET null)");
            entity.Property(e => e.Changed).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ChangedBy).HasComment("user id (ON DELETE SET null)");
            entity.Property(e => e.CoverSd).HasComment("(downscaled) attachment_id (ON DELETE SET null)");
            entity.Property(e => e.Created).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Discount).HasComment("(has_discount)");
            entity.Property(e => e.DiscountEnd).HasComment("(has_discount)");
            entity.Property(e => e.DiscountStart).HasComment("(has_discount)");
            entity.Property(e => e.ListId).HasComment("list_id (ON DELETE CASCADE)");
            entity.Property(e => e.Meassurements).HasComment("(has_meassurements)");
            entity.Property(e => e.Price).HasComment("(has_price)");
            entity.Property(e => e.Unit)
                .HasDefaultValueSql("'kg'")
                .HasComment("(has_weight)");
            entity.Property(e => e.Weight).HasComment("(has_weight)");

            entity.HasOne(d => d.AuthorNavigation).WithMany(p => p.ItemAuthorNavigations)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("items_ibfk_3");

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.ItemChangedByNavigations)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("items_ibfk_4");

            entity.HasOne(d => d.CoverSdNavigation).WithMany(p => p.Items)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("items_ibfk_2");

            entity.HasOne(d => d.List).WithMany(p => p.Items).HasConstraintName("items_ibfk_1");
        }
    );
}
