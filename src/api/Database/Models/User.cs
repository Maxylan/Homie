using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homie.Database.Models;

[Table("users")]
[Index("PlatformId", Name = "platform_id")]
public partial record User : IBaseModel<User>
{
    [Key]
    [Column("id")]
    public uint Id { get; set; }

    /// <summary>
    /// platform_id (ON DELETE CASCADE)
    /// </summary>
    [Column("platform_id")]
    public uint PlatformId { get; set; }

    [Column("username")]
    [StringLength(63)]
    public string Username { get; set; } = null!;

    [Column("first_name")]
    [StringLength(63)]
    public string? FirstName { get; set; }

    [Column("last_name")]
    [StringLength(63)]
    public string? LastName { get; set; }

    [Column("group", TypeName = "enum('banned','guest','member','admin')")]
    public string Group { get; set; } = null!;

    [Column("token")]
    [StringLength(63)]
    public string Token { get; set; } = null!;

    [Column("expires", TypeName = "datetime")]
    public DateTime? Expires { get; set; }

    [Column("created", TypeName = "datetime")]
    public DateTime Created { get; set; }

    [Column("changed", TypeName = "datetime")]
    public DateTime Changed { get; set; }

    [InverseProperty("ChangedByNavigation")]
    public virtual ICollection<Attachment> AttachmentChangedByNavigations { get; set; } = new List<Attachment>();

    [InverseProperty("UploadedByNavigation")]
    public virtual ICollection<Attachment> AttachmentUploadedByNavigations { get; set; } = new List<Attachment>();

    [InverseProperty("ChangedByNavigation")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    [InverseProperty("AuthorNavigation")]
    public virtual ICollection<Item> ItemAuthorNavigations { get; set; } = new List<Item>();

    [InverseProperty("ChangedByNavigation")]
    public virtual ICollection<Item> ItemChangedByNavigations { get; set; } = new List<Item>();

    [InverseProperty("AuthorNavigation")]
    public virtual ICollection<List> ListAuthorNavigations { get; set; } = new List<List>();

    [InverseProperty("ChangedByNavigation")]
    public virtual ICollection<List> ListChangedByNavigations { get; set; } = new List<List>();

    [InverseProperty("AuthorNavigation")]
    public virtual ICollection<Note> NoteAuthorNavigations { get; set; } = new List<Note>();

    [InverseProperty("ChangedByNavigation")]
    public virtual ICollection<Note> NoteChangedByNavigations { get; set; } = new List<Note>();

    [InverseProperty("ChangedByNavigation")]
    public virtual ICollection<Option> Options { get; set; } = new List<Option>();

    [ForeignKey("PlatformId")]
    [InverseProperty("Users")]
    public virtual Platform Platform { get; set; } = null!;

    [InverseProperty("AuthorNavigation")]
    public virtual ICollection<Recipe> RecipeAuthorNavigations { get; set; } = new List<Recipe>();

    [InverseProperty("ChangedByNavigation")]
    public virtual ICollection<Recipe> RecipeChangedByNavigations { get; set; } = new List<Recipe>();

    [InverseProperty("User")]
    public virtual ICollection<RecipeRating> RecipeRatings { get; set; } = new List<RecipeRating>();

    [InverseProperty("AuthorNavigation")]
    public virtual ICollection<Reminder> ReminderAuthorNavigations { get; set; } = new List<Reminder>();

    [InverseProperty("ChangedByNavigation")]
    public virtual ICollection<Reminder> ReminderChangedByNavigations { get; set; } = new List<Reminder>();

    [InverseProperty("User")]
    public virtual ICollection<UserAvatar> UserAvatars { get; set; } = new List<UserAvatar>();

    [InverseProperty("User")]
    public virtual ICollection<Visibility> Visibilities { get; set; } = new List<Visibility>();

    [ForeignKey("UserId")]
    [InverseProperty("Users")]
    public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();

    /// <summary>
    /// The configuration for the 'User' entity, reflects `users` table in the database.
    /// </summary>
    /// <returns></returns>
    public static Action<EntityTypeBuilder<User>> Configuration() => (
        entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Changed).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Created).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Group).HasDefaultValueSql("'guest'");
            entity.Property(e => e.PlatformId).HasComment("platform_id (ON DELETE CASCADE)");

            entity.HasOne(d => d.Platform).WithMany(p => p.Users).HasConstraintName("users_ibfk_1");
        }
    );
}
