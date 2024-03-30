using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homie.Database.Models;

[Table("recipes")]
[Index("Author", Name = "author")]
[Index("ChangedBy", Name = "changed_by")]
[Index("Cover", Name = "cover")]
[Index("CoverSd", Name = "cover_sd")]
[Index("IngredientsListId", Name = "ingredients_list_id")]
[Index("PlatformId", Name = "platform_id")]
[Index("TodoListId", Name = "todo_list_id")]
public partial record Recipe : IBaseModel<Recipe>
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
    /// This is the TIME to COOK
    /// </summary>
    [Column("cooking_time", TypeName = "time")]
    public TimeOnly? CookingTime { get; set; }

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
    /// This is the TIME to COOK
    /// </summary>
    [Column("portions_from")]
    public uint? PortionsFrom { get; set; }

    /// <summary>
    /// This is the TIME to COOK
    /// </summary>
    [Column("portions_to")]
    public uint? PortionsTo { get; set; }

    /// <summary>
    /// list_id (ON DELETE SET null)
    /// </summary>
    [Column("ingredients_list_id")]
    public uint? IngredientsListId { get; set; }

    /// <summary>
    /// list_id (ON DELETE SET null)
    /// </summary>
    [Column("todo_list_id")]
    public uint? TodoListId { get; set; }

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
    [InverseProperty("RecipeAuthorNavigations")]
    public virtual User? AuthorNavigation { get; set; }

    [ForeignKey("ChangedBy")]
    [InverseProperty("RecipeChangedByNavigations")]
    public virtual User? ChangedByNavigation { get; set; }

    [ForeignKey("Cover")]
    [InverseProperty("RecipeCoverNavigations")]
    public virtual Attachment? CoverNavigation { get; set; }

    [ForeignKey("CoverSd")]
    [InverseProperty("RecipeCoverSdNavigations")]
    public virtual Attachment? CoverSdNavigation { get; set; }

    [ForeignKey("IngredientsListId")]
    [InverseProperty("RecipeIngredientsLists")]
    public virtual List? IngredientsList { get; set; }

    [ForeignKey("PlatformId")]
    [InverseProperty("Recipes")]
    public virtual Platform Platform { get; set; } = null!;

    [InverseProperty("Recipe")]
    public virtual ICollection<RecipeRating> RecipeRatings { get; set; } = new List<RecipeRating>();

    [InverseProperty("Recipe")]
    public virtual ICollection<RecipeTag> RecipeTags { get; set; } = new List<RecipeTag>();

    [ForeignKey("TodoListId")]
    [InverseProperty("RecipeTodoLists")]
    public virtual List? TodoList { get; set; }

    [ForeignKey("RecipeId")]
    [InverseProperty("Recipes")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();

    /// <summary>
    /// The configuration for the 'Recipe' entity, reflects `recipes` table in the database.
    /// </summary>
    /// <returns></returns>
    public static Action<EntityTypeBuilder<Recipe>> Configuration() => (
        entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Author).HasComment("user id (ON DELETE SET null)");
            entity.Property(e => e.Changed).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ChangedBy).HasComment("user id (ON DELETE SET null)");
            entity.Property(e => e.CookingTime).HasComment("This is the TIME to COOK");
            entity.Property(e => e.Cover).HasComment("attachment_id (ON DELETE SET null)");
            entity.Property(e => e.CoverSd).HasComment("(downscaled) attachment_id (ON DELETE SET null)");
            entity.Property(e => e.Created).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IngredientsListId).HasComment("list_id (ON DELETE SET null)");
            entity.Property(e => e.PlatformId).HasComment("platform_id (ON DELETE CASCADE)");
            entity.Property(e => e.PortionsFrom)
                .HasDefaultValueSql("'2'")
                .HasComment("This is the TIME to COOK");
            entity.Property(e => e.PortionsTo)
                .HasDefaultValueSql("'4'")
                .HasComment("This is the TIME to COOK");
            entity.Property(e => e.TodoListId).HasComment("list_id (ON DELETE SET null)");
            entity.Property(e => e.Visibility).HasDefaultValueSql("'global'");

            entity.HasOne(d => d.AuthorNavigation).WithMany(p => p.RecipeAuthorNavigations)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("recipes_ibfk_6");

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.RecipeChangedByNavigations)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("recipes_ibfk_7");

            entity.HasOne(d => d.CoverNavigation).WithMany(p => p.RecipeCoverNavigations)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("recipes_ibfk_2");

            entity.HasOne(d => d.CoverSdNavigation).WithMany(p => p.RecipeCoverSdNavigations)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("recipes_ibfk_3");

            entity.HasOne(d => d.IngredientsList).WithMany(p => p.RecipeIngredientsLists)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("recipes_ibfk_4");

            entity.HasOne(d => d.Platform).WithMany(p => p.Recipes).HasConstraintName("recipes_ibfk_1");

            entity.HasOne(d => d.TodoList).WithMany(p => p.RecipeTodoLists)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("recipes_ibfk_5");

            entity.HasMany(d => d.Users).WithMany(p => p.Recipes)
                .UsingEntity<Dictionary<string, object>>(
                    "RecipeFavorite",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("recipe_favorites_ibfk_2"),
                    l => l.HasOne<Recipe>().WithMany()
                        .HasForeignKey("RecipeId")
                        .HasConstraintName("recipe_favorites_ibfk_1"),
                    j =>
                    {
                        j.HasKey("RecipeId", "UserId")
                            .HasName("PRIMARY")
                            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                        j.ToTable("recipe_favorites");
                        j.HasIndex(new[] { "UserId" }, "user_id");
                        j.IndexerProperty<uint>("RecipeId")
                            .HasComment("recipe_id (ON DELETE CASCADE)")
                            .HasColumnName("recipe_id");
                        j.IndexerProperty<uint>("UserId")
                            .HasComment("user id (ON DELETE CASCADE)")
                            .HasColumnName("user_id");
                    });
        }
    );
}
