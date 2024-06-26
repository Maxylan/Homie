﻿// (c) 2024 @Maxylan
// Scaffolded, then altered to suit my needs. @see ../scaffold.txt
namespace Homie.Database.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// The 'RecipeTag' entity, reflects `recipe_tags` table in the database.
/// </summary>
[Table("recipe_tags")]
[Index("RecipeId", Name = "recipe_id")]
public partial record RecipeTag : IBaseModel<RecipeTag>
{
    /// <summary>PK</summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public uint? Id { get; set; }

    /// <summary>
    /// recipe_id (ON DELETE CASCADE)
    /// </summary>
    [Column("recipe_id")]
    public uint RecipeId { get; set; }

    [Column("name")]
    [StringLength(63)]
    public string Name { get; set; } = null!;

    [ForeignKey("RecipeId")]
    [InverseProperty("RecipeTags")]
    public virtual Recipe Recipe { get; set; } = null!;

    /// <summary>
    /// The configuration for the 'RecipeTag' entity, reflects `recipe_tags` table in the database.
    /// </summary>
    /// <returns></returns>
    public static Action<EntityTypeBuilder<RecipeTag>> Configuration() => (
        entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.RecipeId).HasComment("recipe_id (ON DELETE CASCADE)");

            entity.HasOne(d => d.Recipe).WithMany(p => p.RecipeTags).HasConstraintName("recipe_tags_ibfk_1");
        }
    );

    /// <summary>
    /// Convert the '<see cref="RecipeTag"/>' entity to a '<see cref="RecipeTagDTO"/>' instance.
    /// </summary>
    /// <returns><see cref="RecipeTagDTO"/></returns>
    public object ToDataTransferObject() => (
        new
        {
            Id = Id,
            RecipeId = RecipeId,
            Name = Name
        }
    );
}
