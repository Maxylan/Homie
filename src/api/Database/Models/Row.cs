// (c) 2024 @Maxylan
// Scaffolded, then altered to suit my needs. @see ../scaffold.txt
namespace Homie.Database.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// The 'Row' entity, reflects `rows` table in the database.
/// </summary>
[Table("rows")]
[Index("CoverSd", Name = "cover_sd")]
[Index("GroupId", Name = "group_id")]
[Index("ItemId", Name = "item_id")]
[Index("ListId", Name = "list_id")]
[Index("ProductId", Name = "product_id")]
public partial record Row : IBaseModel<Row>
{
    [Key]
    [Column("id")]
    public uint Id { get; set; }

    /// <summary>
    /// list_id (ON DELETE CASCADE)
    /// </summary>
    [Column("list_id")]
    public uint ListId { get; set; }

    /// <summary>
    /// Ascending
    /// </summary>
    [Column("order")]
    public uint Order { get; set; } = 0;

    [Column("title")]
    [StringLength(127)]
    public string Title { get; set; } = "";

    /// <summary>
    /// (downscaled) attachment_id (ON DELETE SET null)
    /// </summary>
    [Column("cover_sd")]
    public uint? CoverSd { get; set; }

    [Column("has_group")]
    public bool HasGroup { get; set; } = false;

    /// <summary>
    /// group_id (ON DELETE SET null)
    /// </summary>
    [Column("group_id")]
    public uint? GroupId { get; set; } = null;

    [Column("has_checkbox")]
    public bool HasCheckbox { get; set; } = false;

    /// <summary>
    /// (has_checkbox)
    /// </summary>
    [Column("store_checkbox")]
    public bool StoreCheckbox { get; set; } = false;

    /// <summary>
    /// (has_checkbox)
    /// </summary>
    [Column("checked")]
    public bool? Checked { get; set; } = null;

    [Column("has_timer")]
    public bool HasTimer { get; set; } = false;

    /// <summary>
    /// (countdown, seconds)
    /// </summary>
    [Column("deadline")]
    public uint? Deadline { get; set; } = 30;

    [Column("has_autocomplete")]
    public bool HasAutocomplete { get; set; } = false;

    /// <summary>
    /// item_id (ON DELETE SET null)
    /// </summary>
    [Column("item_id")]
    public uint? ItemId { get; set; } = null;

    /// <summary>
    /// product_id (ON DELETE SET null)
    /// </summary>
    [Column("product_id")]
    public uint? ProductId { get; set; } = null;

    [ForeignKey("CoverSd")]
    [InverseProperty("Rows")]
    public virtual Attachment? CoverSdAttachment { get; set; }

    [ForeignKey("GroupId")]
    [InverseProperty("Rows")]
    public virtual Group? Group { get; set; }

    [ForeignKey("ItemId")]
    [InverseProperty("Rows")]
    public virtual Item? Item { get; set; }

    [ForeignKey("ListId")]
    [InverseProperty("Rows")]
    public virtual List List { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("Rows")]
    public virtual Product? Product { get; set; }

    /// <summary>
    /// The configuration for the 'Row' entity, reflects `rows` table in the database.
    /// </summary>
    /// <returns></returns>
    public static Action<EntityTypeBuilder<Row>> Configuration() => (
        entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Checked)
                .HasDefaultValueSql("'0'")
                .HasComment("(has_checkbox)");
            entity.Property(e => e.CoverSd).HasComment("(downscaled) attachment_id (ON DELETE SET null)");
            entity.Property(e => e.Deadline)
                .HasDefaultValueSql("'30'")
                .HasComment("(countdown, seconds)");
            entity.Property(e => e.GroupId).HasComment("group_id (ON DELETE SET null)");
            entity.Property(e => e.ItemId).HasComment("item_id (ON DELETE SET null)");
            entity.Property(e => e.ListId).HasComment("list_id (ON DELETE CASCADE)");
            entity.Property(e => e.Order).HasComment("Ascending");
            entity.Property(e => e.ProductId).HasComment("product_id (ON DELETE SET null)");
            entity.Property(e => e.StoreCheckbox).HasComment("(has_checkbox)");
            entity.Property(e => e.Title).HasDefaultValueSql("''");

            entity.HasOne(d => d.CoverSdAttachment).WithMany(p => p.Rows)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("rows_ibfk_2");

            entity.HasOne(d => d.Group).WithMany(p => p.Rows)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("rows_ibfk_3");

            entity.HasOne(d => d.Item).WithMany(p => p.Rows)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("rows_ibfk_4");

            entity.HasOne(d => d.List).WithMany(p => p.Rows).HasConstraintName("rows_ibfk_1");

            entity.HasOne(d => d.Product).WithMany(p => p.Rows)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("rows_ibfk_5");
        }
    );

    /// <summary>
    /// Convert the '<see cref="Row"/>' entity to a '<see cref="RowDTO"/>' instance.
    /// </summary>
    /// <returns><see cref="RowDTO"/></returns>
    public object ToDataTransferObject() => (
        new
        {
            Id,
            ListId,
            Order,
            Title,
            CoverSd,
            HasGroup,
            GroupId,
            HasCheckbox,
            StoreCheckbox,
            Checked,
            HasTimer,
            Deadline,
            HasAutocomplete,
            ItemId,
            ProductId
        }
    );
}
