// (c) 2024 @Maxylan
// Scaffolded, then altered to suit my needs. @see ../scaffold.txt
namespace Homie.Database.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
/// The configuration for the 'Attachment' entity, reflects `attachments` table in the database.
/// </summary>
[Table("attachments")]
[Index("ChangedBy", Name = "changed_by")]
[Index("PlatformId", Name = "platform_id")]
[Index("UploadedBy", Name = "uploaded_by")]
public partial record Attachment : IBaseModel<Attachment>
{
    [Key]
    [Column("id")]
    public uint Id { get; set; }

    /// <summary>
    /// platform_id (ON DELETE CASCADE)
    /// </summary>
    [Column("platform_id")]
    public uint PlatformId { get; set; }

    /// <summary>
    /// PK Of the resource this attachment is attached to
    /// </summary>
    [Column("resource_id")]
    public uint ResourceId { get; set; }

    [Column("file")]
    [StringLength(255)]
    public string File { get; set; } = null!;

    [Column("type")]
    [StringLength(127)]
    public string Type { get; set; } = null!;

    [Column("alt")]
    [StringLength(255)]
    public string? Alt { get; set; }

    [Column("blob", TypeName = "mediumblob")]
    public byte[]? Blob { get; set; }

    [Column("source")]
    [StringLength(255)]
    public string? Source { get; set; }

    /// <summary>
    /// created
    /// </summary>
    [Column("uploaded", TypeName = "datetime")]
    public DateTime Uploaded { get; set; } = DateTime.Now;

    /// <summary>
    /// author user id (ON DELETE SET null)
    /// </summary>
    [Column("uploaded_by")]
    public uint? UploadedBy { get; set; }

    [Column("changed", TypeName = "datetime")]
    public DateTime Changed { get; set; } = DateTime.Now;

    /// <summary>
    /// user id (ON DELETE SET null)
    /// </summary>
    [Column("changed_by")]
    public uint? ChangedBy { get; set; }

    [ForeignKey("ChangedBy")]
    [InverseProperty("AttachmentChangedByUsers")]
    public virtual User? ChangedByUser { get; set; }

    [InverseProperty("CoverSdAttachment")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    [InverseProperty("CoverSdAttachment")]
    public virtual ICollection<Item> Items { get; set; } = new List<Item>();

    [InverseProperty("CoverAttachment")]
    public virtual ICollection<List> ListCoverAttachments { get; set; } = new List<List>();

    [InverseProperty("CoverSdAttachment")]
    public virtual ICollection<List> ListCoverSdAttachments { get; set; } = new List<List>();

    [ForeignKey("PlatformId")]
    [InverseProperty("Attachments")]
    public virtual Platform Platform { get; set; } = null!;

    [InverseProperty("CoverAttachment")]
    public virtual ICollection<Recipe> RecipeCoverAttachments { get; set; } = new List<Recipe>();

    [InverseProperty("CoverSdAttachment")]
    public virtual ICollection<Recipe> RecipeCoverSdAttachments { get; set; } = new List<Recipe>();

    [InverseProperty("CoverSdAttachment")]
    public virtual ICollection<Row> Rows { get; set; } = new List<Row>();

    [ForeignKey("UploadedBy")]
    [InverseProperty("AttachmentUploadedByUsers")]
    public virtual User? UploadedByUser { get; set; }

    [InverseProperty("Attachment")]
    public virtual ICollection<UserAvatar> UserAvatarAttachments { get; set; } = new List<UserAvatar>();

    [InverseProperty("AvatarSdNavigation")]
    public virtual ICollection<UserAvatar> UserAvatarSdAttachment { get; set; } = new List<UserAvatar>();

    /// <summary>
    /// The configuration for the 'Attachment' entity, reflects `attachments` table in the database.
    /// </summary>
    /// <returns></returns>
    public static Action<EntityTypeBuilder<Attachment>> Configuration() => (
        entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Changed).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ChangedBy).HasComment("user id (ON DELETE SET null)");
            entity.Property(e => e.PlatformId).HasComment("platform_id (ON DELETE CASCADE)");
            entity.Property(e => e.ResourceId).HasComment("PK Of the resource this attachment is attached to");
            entity.Property(e => e.Uploaded)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("created");
            entity.Property(e => e.UploadedBy).HasComment("author user id (ON DELETE SET null)");

            entity.HasOne(d => d.ChangedByUser).WithMany(p => p.AttachmentChangedByUsers)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("attachments_ibfk_3");

            entity.HasOne(d => d.Platform).WithMany(p => p.Attachments).HasConstraintName("attachments_ibfk_1");

            entity.HasOne(d => d.UploadedByUser).WithMany(p => p.AttachmentUploadedByUsers)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("attachments_ibfk_2");
        }
    );

    /// <summary>
    /// Convert the '<see cref="Attachment"/>' entity to a '<see cref="AttachmentDTO"/>' instance.
    /// </summary>
    /// <returns><see cref="AttachmentDTO"/></returns>
    public object ToDataTransferObject() => (
        new
        {
            Id,
            PlatformId,
            ResourceId,
            File,
            Type,
            Alt,
            Blob,
            Source,
            Uploaded,
            UploadedBy,
            Changed,
            ChangedBy
        }
    );
}
