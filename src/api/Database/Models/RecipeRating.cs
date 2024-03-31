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
/// The 'RecipeRating' entity, reflects `recipe_ratings` table in the database.
/// </summary>
[Table("recipe_ratings")]
[Index("RecipeId", Name = "recipe_id")]
[Index("UserId", Name = "user_id")]
public partial record RecipeRating : IBaseModel<RecipeRating>
{
    [Key]
    [Column("id")]
    public uint Id { get; set; }

    /// <summary>
    /// recipe_id (ON DELETE CASCADE)
    /// </summary>
    [Column("recipe_id")]
    public uint RecipeId { get; set; }

    /// <summary>
    /// user id (ON DELETE CASCADE)
    /// </summary>
    [Column("user_id")]
    public uint UserId { get; set; }

    /// <summary>
    /// 0-10
    /// </summary>
    [Column("rating")]
    public uint Rating { get; set; } = 5;

    [ForeignKey("RecipeId")]
    [InverseProperty("RecipeRatings")]
    public virtual Recipe Recipe { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("RecipeRatings")]
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// The configuration for the 'RecipeRating' entity, reflects `recipe_ratings` table in the database.
    /// </summary>
    /// <returns></returns>
    public static Action<EntityTypeBuilder<RecipeRating>> Configuration() => (
        entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Rating)
                .HasDefaultValueSql("'5'")
                .HasComment("0-10");
            entity.Property(e => e.RecipeId).HasComment("recipe_id (ON DELETE CASCADE)");
            entity.Property(e => e.UserId).HasComment("user id (ON DELETE CASCADE)");

            entity.HasOne(d => d.Recipe).WithMany(p => p.RecipeRatings).HasConstraintName("recipe_ratings_ibfk_1");

            entity.HasOne(d => d.User).WithMany(p => p.RecipeRatings).HasConstraintName("recipe_ratings_ibfk_2");
        }
    );
}
