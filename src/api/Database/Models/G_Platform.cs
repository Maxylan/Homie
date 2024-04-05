// (c) 2024 @Maxylan
// Scaffolded, then altered to suit my needs. @see ../scaffold.txt
namespace Homie.Database.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Homie.Api.v1.TransferModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// The 'Platform' entity, reflects `g_platforms` table in the database.
/// </summary>
[Table("g_platforms")]
[Index("GuestCode", Name = "guest_code", IsUnique = true)]
[Index("MemberCode", Name = "member_code", IsUnique = true)]
public partial record Platform : IBaseModel<Platform>
{
    /// <summary>PK</summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public uint? Id { get; set; }

    [Column("name")]
    [StringLength(63)]
    public string Name { get; set; } = null!;

    [Column("guest_code")]
    [StringLength(31)]
    public string GuestCode { get; set; } = null!;

    [Column("member_code")]
    [StringLength(31)]
    public string MemberCode { get; set; } = null!;

    [Column("master_pswd")]
    [StringLength(63)]
    public string MasterPswd { get; set; } = null!;

    [Column("reset_token")]
    [StringLength(63)]
    public string ResetToken { get; set; } = null!;

    [Column("created", TypeName = "datetime")]
    public DateTime Created { get; set; } = DateTime.Now;

    [InverseProperty("Platform")]
    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    [InverseProperty("Platform")]
    public virtual ICollection<AccessLog> AccessLogs { get; set; } = new List<AccessLog>();

    [InverseProperty("Platform")]
    public virtual ICollection<Export> Exports { get; set; } = new List<Export>();

    [InverseProperty("ChangedByPlatformNavigation")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    [InverseProperty("Platform")]
    public virtual ICollection<List> Lists { get; set; } = new List<List>();

    [InverseProperty("Platform")]
    public virtual ICollection<Note> Notes { get; set; } = new List<Note>();

    [InverseProperty("Platform")]
    public virtual ICollection<Option> Options { get; set; } = new List<Option>();

    [InverseProperty("Platform")]
    public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();

    [InverseProperty("Platform")]
    public virtual ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();

    [InverseProperty("Platform")]
    public virtual ICollection<UserAvatar> UserAvatars { get; set; } = new List<UserAvatar>();

    [InverseProperty("Platform")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();

    [InverseProperty("Platform")]
    public virtual ICollection<Visibility> Visibilities { get; set; } = new List<Visibility>();

    /// <summary>
    /// The configuration for the 'Platform' entity, reflects `g_platforms` table in the database.
    /// </summary>
    /// <returns></returns>
    public static Action<EntityTypeBuilder<Platform>> Configuration() => (
        entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Created).HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    );

    /// <summary>
    /// Convert the '<see cref="Platform"/>' entity to a '<see cref="PlatformDTO"/>' instance.
    /// </summary>
    /// <returns><see cref="PlatformDTO"/></returns>
    public object ToDataTransferObject() => (
        new PlatformDTO()
        {
            Id = Id,
            Name = Name,
            GuestCode = GuestCode,
            MemberCode = MemberCode,
            MasterPswd = MasterPswd,
            ResetToken = ResetToken,
            Created = Created
        }
    );
}
