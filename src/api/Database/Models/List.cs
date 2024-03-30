﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homie.Database.Models;

[Table("lists")]
[Index("Author", Name = "author")]
[Index("ChangedBy", Name = "changed_by")]
[Index("Cover", Name = "cover")]
[Index("CoverSd", Name = "cover_sd")]
[Index("PlatformId", Name = "platform_id")]
public partial record List : IBaseModel<List>
{
    [Key]
    [Column("id")]
    public uint Id { get; set; }

    /// <summary>
    /// platform_id (ON DELETE CASCADE)
    /// </summary>
    [Column("platform_id")]
    public uint PlatformId { get; set; }

    [Column("visibility", TypeName = "enum('private','selective','inclusive','members','global')")]
    public string Visibility { get; set; } = null!;

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
    public DateTime Created { get; set; }

    [Column("changed", TypeName = "datetime")]
    public DateTime Changed { get; set; }

    /// <summary>
    /// user id (ON DELETE SET null)
    /// </summary>
    [Column("changed_by")]
    public uint? ChangedBy { get; set; }

    [ForeignKey("Author")]
    [InverseProperty("ListAuthorNavigations")]
    public virtual User? AuthorNavigation { get; set; }

    [ForeignKey("ChangedBy")]
    [InverseProperty("ListChangedByNavigations")]
    public virtual User? ChangedByNavigation { get; set; }

    [ForeignKey("Cover")]
    [InverseProperty("ListCoverNavigations")]
    public virtual Attachment? CoverNavigation { get; set; }

    [ForeignKey("CoverSd")]
    [InverseProperty("ListCoverSdNavigations")]
    public virtual Attachment? CoverSdNavigation { get; set; }

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
            entity.Property(e => e.Visibility).HasDefaultValueSql("'global'");

            entity.HasOne(d => d.AuthorNavigation).WithMany(p => p.ListAuthorNavigations)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("lists_ibfk_4");

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.ListChangedByNavigations)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("lists_ibfk_5");

            entity.HasOne(d => d.CoverNavigation).WithMany(p => p.ListCoverNavigations)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("lists_ibfk_2");

            entity.HasOne(d => d.CoverSdNavigation).WithMany(p => p.ListCoverSdNavigations)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("lists_ibfk_3");

            entity.HasOne(d => d.Platform).WithMany(p => p.Lists).HasConstraintName("lists_ibfk_1");
        }
    );
}