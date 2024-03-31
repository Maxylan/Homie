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
/// The 'Recipe' entity, reflects `recipes` table in the database.
/// </summary>
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
    public Visibilities Visibility { get; set; } = Visibilities.Global;

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
    public TimeSpan? CookingTime { get; set; } = null;

    /// <summary>
    /// attachment_id (ON DELETE SET null)
    /// </summary>
    [Column("cover")]
    public uint? Cover { get; set; } = null;

    /// <summary>
    /// (downscaled) attachment_id (ON DELETE SET null)
    /// </summary>
    [Column("cover_sd")]
    public uint? CoverSd { get; set; } = null;

    [Column("portions_from")]
    public uint? PortionsFrom { get; set; } = 2;

    [Column("portions_to")]
    public uint? PortionsTo { get; set; } = 4;

    /// <summary>
    /// list_id (ON DELETE SET null)
    /// </summary>
    [Column("ingredients_list_id")]
    public uint? IngredientsListId { get; set; } = null;

    /// <summary>
    /// list_id (ON DELETE SET null)
    /// </summary>
    [Column("todo_list_id")]
    public uint? TodoListId { get; set; } = null;

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
    [InverseProperty("RecipeCreatedByUsers")]
    public virtual User? CreatedByUser { get; set; }

    [ForeignKey("ChangedBy")]
    [InverseProperty("RecipeChangedByUsers")]
    public virtual User? ChangedByUser { get; set; }

    [ForeignKey("Cover")]
    [InverseProperty("RecipeCoverAttachments")]
    public virtual Attachment? CoverAttachment { get; set; }

    [ForeignKey("CoverSd")]
    [InverseProperty("RecipeCoverSdAttachments")]
    public virtual Attachment? CoverSdAttachment { get; set; }

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
                .HasDefaultValueSql("'2'");
            entity.Property(e => e.PortionsTo)
                .HasDefaultValueSql("'4'");
            entity.Property(e => e.TodoListId).HasComment("list_id (ON DELETE SET null)");
            entity.Property(e => e.Visibility)
                .HasComment("enum('private','selective','inclusive','members','global')")
                .HasDefaultValueSql("'global'")
                .HasConversion<string>(
                    v => v.ToString(),
                    v => (Visibilities)Enum.Parse(typeof(Visibilities), v)
                );

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.RecipeCreatedByUsers)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("recipes_ibfk_6");

            entity.HasOne(d => d.ChangedByUser).WithMany(p => p.RecipeChangedByUsers)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("recipes_ibfk_7");

            entity.HasOne(d => d.CoverAttachment).WithMany(p => p.RecipeCoverAttachments)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("recipes_ibfk_2");

            entity.HasOne(d => d.CoverSdAttachment).WithMany(p => p.RecipeCoverSdAttachments)
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
