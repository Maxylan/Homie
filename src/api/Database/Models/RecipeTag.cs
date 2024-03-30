using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homie.Database.Models;

[Table("recipe_tags")]
[Index("RecipeId", Name = "recipe_id")]
public partial record RecipeTag : IBaseModel<RecipeTag>
{
    [Key]
    [Column("id")]
    public uint Id { get; set; }

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
}
