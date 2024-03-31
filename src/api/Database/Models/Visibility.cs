// (c) 2024 @Maxylan
// Scaffolded, then altered to suit my needs. @see ../scaffold.txt
namespace Homie.Database.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// The 'Visibility' entity, reflects `visibility` table in the database.
/// </summary>
[Table("visibility")]
[Index("PlatformId", Name = "platform_id")]
[Index("UserId", Name = "user_id")]
public partial record Visibility : IBaseModel<Visibility>
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
    /// PK Of the resource this member does or does not have access to
    /// </summary>
    [Column("resource_id")]
    public uint ResourceId { get; set; }

    /// <summary>
    /// user id (ON DELETE CASCADE)
    /// </summary>
    [Column("user_id")]
    public uint UserId { get; set; }

    [ForeignKey("PlatformId")]
    [InverseProperty("Visibilities")]
    public virtual Platform Platform { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Visibilities")]
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// The configuration for the 'Visibility' entity, reflects `visibility` table in the database.
    /// </summary>
    /// <returns></returns>
    public static Action<EntityTypeBuilder<Visibility>> Configuration() => (
        entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.PlatformId).HasComment("platform_id (ON DELETE CASCADE)");
            entity.Property(e => e.ResourceId).HasComment("PK Of the resource this member does or does not have access to");
            entity.Property(e => e.UserId).HasComment("user id (ON DELETE CASCADE)");

            entity.HasOne(d => d.Platform).WithMany(p => p.Visibilities).HasConstraintName("visibility_ibfk_1");

            entity.HasOne(d => d.User).WithMany(p => p.Visibilities).HasConstraintName("visibility_ibfk_2");
        }
    );
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Visibilities 
{
    Private, 
    Selective, 
    Inclusive,
    Members, 
    Global
}