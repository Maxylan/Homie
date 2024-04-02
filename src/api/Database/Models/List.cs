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
/// The 'List' entity, reflects `lists` table in the database.
/// </summary>
[Table("lists")]
[Index("Author", Name = "author")]
[Index("ChangedBy", Name = "changed_by")]
[Index("Cover", Name = "cover")]
[Index("CoverSd", Name = "cover_sd")]
[Index("PlatformId", Name = "platform_id")]
public partial record List : IBaseModel<List>
{
    /// <summary>PK</summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public uint? Id { get; set; }

    /// <summary>
    /// platform_id (ON DELETE CASCADE)
    /// </summary>
    [Column("platform_id")]
    public uint PlatformId { get; set; }

    [Column("visibility", TypeName = "enum('private','selective','inclusive','members','global')")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Visibilities Visibility { get; set; } = Visibilities.Global;

    [Column("title")]
    [StringLength(127)]
    public string Title { get; set; } = null!;

    [Column("description")]
    [StringLength(255)]
    public string? Description { get; set; }

    /// <summary>
    /// attachment_id (ON DELETE SET null)
    /// </summary>
    [Column("cover")]
    public uint? Cover { get; set; }

    /// <summary>
    /// (downscaled) attachment_id (ON DELETE SET null)
    /// </summary>
    [Column("cover_sd")]
    public uint? CoverSd { get; set; }

    /// <summary>
    /// user id (ON DELETE SET null)
    /// </summary>
    [Column("author")]
    public uint? Author { get; set; }

    [Column("created", TypeName = "datetime")]
    public DateTime Created { get; set; } = DateTime.Now;

    [Column("changed", TypeName = "datetime")]
    public DateTime Changed { get; set; } = DateTime.Now;

    /// <summary>
    /// user id (ON DELETE SET null)
    /// </summary>
    [Column("changed_by")]
    public uint? ChangedBy { get; set; }

    [ForeignKey("Author")]
    [InverseProperty("ListCreatedByUsers")]
    public virtual User? CreatedByUser { get; set; }

    [ForeignKey("ChangedBy")]
    [InverseProperty("ListChangedByUsers")]
    public virtual User? ChangedByUser { get; set; }

    [ForeignKey("Cover")]
    [InverseProperty("ListCoverAttachments")]
    public virtual Attachment? CoverAttachment { get; set; }

    [ForeignKey("CoverSd")]
    [InverseProperty("ListCoverSdAttachments")]
    public virtual Attachment? CoverSdAttachment { get; set; }

    [InverseProperty("List")]
    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    [InverseProperty("List")]
    public virtual ICollection<Item> Items { get; set; } = new List<Item>();

    [ForeignKey("PlatformId")]
    [InverseProperty("Lists")]
    public virtual Platform Platform { get; set; } = null!;

    [InverseProperty("IngredientsList")]
    public virtual ICollection<Recipe> RecipeIngredientsLists { get; set; } = new List<Recipe>();

    [InverseProperty("TodoList")]
    public virtual ICollection<Recipe> RecipeTodoLists { get; set; } = new List<Recipe>();

    [InverseProperty("List")]
    public virtual ICollection<Row> Rows { get; set; } = new List<Row>();

    /// <summary>
    /// The configuration for the 'List' entity, reflects `lists` table in the database.
    /// </summary>
    /// <returns></returns>
    public static Action<EntityTypeBuilder<List>> Configuration() => (
        entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Author).HasComment("user id (ON DELETE SET null)");
            entity.Property(e => e.Changed).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ChangedBy).HasComment("user id (ON DELETE SET null)");
            entity.Property(e => e.Cover).HasComment("attachment_id (ON DELETE SET null)");
            entity.Property(e => e.CoverSd).HasComment("(downscaled) attachment_id (ON DELETE SET null)");
            entity.Property(e => e.Created).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.PlatformId).HasComment("platform_id (ON DELETE CASCADE)");
            entity.Property(e => e.Visibility)
                .HasComment("enum('private','selective','inclusive','members','global')")
                .HasDefaultValueSql("'global'")
                .HasConversion<string>(
                    v => v.ToString(),
                    v => (Visibilities)Enum.Parse(typeof(Visibilities), v)
                );

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.ListCreatedByUsers)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("lists_ibfk_4");

            entity.HasOne(d => d.ChangedByUser).WithMany(p => p.ListChangedByUsers)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("lists_ibfk_5");

            entity.HasOne(d => d.CoverAttachment).WithMany(p => p.ListCoverAttachments)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("lists_ibfk_2");

            entity.HasOne(d => d.CoverSdAttachment).WithMany(p => p.ListCoverSdAttachments)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("lists_ibfk_3");

            entity.HasOne(d => d.Platform).WithMany(p => p.Lists).HasConstraintName("lists_ibfk_1");
        }
    );

    /// <summary>
    /// Convert the '<see cref="List"/>' entity to a '<see cref="ListDTO"/>' instance.
    /// </summary>
    /// <returns><see cref="ListDTO"/></returns>
    public object ToDataTransferObject() => (
        new
        {
            Id,
            PlatformId,
            Visibility,
            Title,
            Description,
            Cover,
            CoverSd,
            Author,
            Created,
            Changed,
            ChangedBy
        }
    );
}
