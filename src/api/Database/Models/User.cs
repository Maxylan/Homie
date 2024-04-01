// (c) 2024 @Maxylan
// Scaffolded, then altered to suit my needs. @see ../scaffold.txt
namespace Homie.Database.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Homie.Api.v1;
using Homie.Api.v1.TransferModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// The 'User' entity, reflects `users` table in the database.
/// </summary>
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
    public UserGroup Group { get; set; } = UserGroup.Guest;

    [Column("token")]
    [StringLength(63)]
    public string Token { get; set; } = null!;

    [Column("expires", TypeName = "datetime")]
    public DateTime? Expires { get; set; } = null;

    [Column("created", TypeName = "datetime")]
    public DateTime Created { get; set; } = DateTime.Now;

    [Column("changed", TypeName = "datetime")]
    public DateTime Changed { get; set; } = DateTime.Now;

    [InverseProperty("ChangedByUser")]
    public virtual ICollection<Attachment> AttachmentChangedByUsers { get; set; } = new List<Attachment>();

    [InverseProperty("UploadedByUser")]
    public virtual ICollection<Attachment> AttachmentUploadedByUsers { get; set; } = new List<Attachment>();

    [InverseProperty("ChangedByUser")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    [InverseProperty("CreatedByUser")]
    public virtual ICollection<Item> ItemCreatedByUsers { get; set; } = new List<Item>();

    [InverseProperty("ChangedByUser")]
    public virtual ICollection<Item> ItemChangedByUsers { get; set; } = new List<Item>();

    [InverseProperty("CreatedByUser")]
    public virtual ICollection<List> ListCreatedByUsers { get; set; } = new List<List>();

    [InverseProperty("ChangedByUser")]
    public virtual ICollection<List> ListChangedByUsers { get; set; } = new List<List>();

    [InverseProperty("CreatedByUser")]
    public virtual ICollection<Note> NoteCreatedByUsers { get; set; } = new List<Note>();

    [InverseProperty("ChangedByUser")]
    public virtual ICollection<Note> NoteChangedByUsers { get; set; } = new List<Note>();

    [InverseProperty("ChangedByUser")]
    public virtual ICollection<Option> Options { get; set; } = new List<Option>();

    [ForeignKey("PlatformId")]
    [InverseProperty("Users")]
    public virtual Platform Platform { get; set; } = null!;

    [InverseProperty("CreatedByUser")]
    public virtual ICollection<Recipe> RecipeCreatedByUsers { get; set; } = new List<Recipe>();

    [InverseProperty("ChangedByUser")]
    public virtual ICollection<Recipe> RecipeChangedByUsers { get; set; } = new List<Recipe>();

    [InverseProperty("User")]
    public virtual ICollection<RecipeRating> RecipeRatings { get; set; } = new List<RecipeRating>();

    [InverseProperty("CreatedByUser")]
    public virtual ICollection<Reminder> ReminderCreatedByUsers { get; set; } = new List<Reminder>();

    [InverseProperty("ChangedByUser")]
    public virtual ICollection<Reminder> ReminderChangedByUsers { get; set; } = new List<Reminder>();

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
            entity.Property(e => e.PlatformId).HasComment("platform_id (ON DELETE CASCADE)");
            entity.Property(e => e.Group)
                .HasDefaultValueSql("'guest'")
                .HasConversion<string>(
                    v => v.ToString(),
                    v => (UserGroup)Enum.Parse(typeof(UserGroup), v)
                );


            entity.HasOne(d => d.Platform).WithMany(p => p.Users).HasConstraintName("users_ibfk_1");
        }
    );

    /// <summary>
    /// Convert the '<see cref="User"/>' entity to a '<see cref="UserDTO"/>'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns><see cref="UserDTO"/></returns>
    public object ToDataTransferObject() => (
        new UserDTO()
        {
            Id = Id,
            PlatformId = PlatformId,
            Username = Username,
            FirstName = FirstName,
            LastName = LastName,
            Group = Group,
            Token = Token,
            Expires = Expires,
            Created = Created,
            Changed = Changed
        }
    );
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserGroup
{
    Banned,
    Guest,
    Member,
    Admin
}