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
/// The 'UserAvatar' entity, reflects `user_avatars` table in the database.
/// </summary>
[Table("user_avatars")]
[Index("Avatar", Name = "avatar")]
[Index("AvatarSd", Name = "avatar_sd")]
[Index("PlatformId", Name = "platform_id")]
[Index("UserId", Name = "user_id")]
public partial record UserAvatar : IBaseModel<UserAvatar>
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

    /// <summary>
    /// user id (ON DELETE CASCADE)
    /// </summary>
    [Column("user_id")]
    public uint UserId { get; set; }

    /// <summary>
    /// attachment_id (ON DELETE CASCADE)
    /// </summary>
    [Column("avatar")]
    public uint Avatar { get; set; }

    /// <summary>
    /// (downscaled) attachment_id (ON DELETE SET null)
    /// </summary>
    [Column("avatar_sd")]
    public uint? AvatarSd { get; set; }

    [ForeignKey("Avatar")]
    [InverseProperty("UserAvatarAttachments")]
    public virtual Attachment Attachment { get; set; } = null!;

    [ForeignKey("AvatarSd")]
    [InverseProperty("UserAvatarSdAttachment")]
    public virtual Attachment? AvatarSdNavigation { get; set; }

    [ForeignKey("PlatformId")]
    [InverseProperty("UserAvatars")]
    public virtual Platform Platform { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("UserAvatars")]
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// The configuration for the 'UserAvatar' entity, reflects `user_avatars` table in the database.
    /// </summary>
    /// <returns></returns>
    public static Action<EntityTypeBuilder<UserAvatar>> Configuration() => (
        entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Avatar).HasComment("attachment_id (ON DELETE CASCADE)");
            entity.Property(e => e.AvatarSd).HasComment("(downscaled) attachment_id (ON DELETE SET null)");
            entity.Property(e => e.PlatformId).HasComment("platform_id (ON DELETE CASCADE)");
            entity.Property(e => e.UserId).HasComment("user id (ON DELETE CASCADE)");

            entity.HasOne(d => d.Attachment).WithMany(p => p.UserAvatarAttachments).HasConstraintName("user_avatars_ibfk_3");

            entity.HasOne(d => d.AvatarSdNavigation).WithMany(p => p.UserAvatarSdAttachment)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("user_avatars_ibfk_4");

            entity.HasOne(d => d.Platform).WithMany(p => p.UserAvatars).HasConstraintName("user_avatars_ibfk_1");

            entity.HasOne(d => d.User).WithMany(p => p.UserAvatars).HasConstraintName("user_avatars_ibfk_2");
        }
    );

    /// <summary>
    /// Convert the '<see cref="UserAvatar"/>' entity to a '<see cref="UserAvatarDTO"/>' instance.
    /// </summary>
    /// <returns><see cref="UserAvatarDTO"/></returns>
    public object ToDataTransferObject() => (
        new
        {
            Id,
            PlatformId,
            UserId,
            Avatar,
            AvatarSd
        }
    );
}
