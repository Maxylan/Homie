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
/// The 'Option' entity, reflects `options` table in the database.
/// </summary>
[Table("options")]
[Index("ChangedBy", Name = "changed_by")]
[Index("PlatformId", Name = "platform_id")]
public partial record Option : IBaseModel<Option>
{
    [Key]
    [Column("id")]
    public uint Id { get; set; }

    /// <summary>
    /// platform_id (ON DELETE CASCADE)
    /// </summary>
    [Column("platform_id")]
    public uint PlatformId { get; set; }

    [Column("key")]
    [StringLength(63)]
    public string Key { get; set; } = null!;

    [Column("value")]
    [StringLength(255)]
    public string? Value { get; set; }

    [Column("created", TypeName = "datetime")]
    public DateTime Created { get; set; } = DateTime.Now;

    [Column("changed", TypeName = "datetime")]
    public DateTime Changed { get; set; } = DateTime.Now;

    /// <summary>
    /// user id (ON DELETE SET null)
    /// </summary>
    [Column("changed_by")]
    public uint? ChangedBy { get; set; }

    [ForeignKey("ChangedBy")]
    [InverseProperty("Options")]
    public virtual User? ChangedByUser { get; set; }

    [ForeignKey("PlatformId")]
    [InverseProperty("Options")]
    public virtual Platform Platform { get; set; } = null!;

    /// <summary>
    /// The configuration for the 'Option' entity, reflects `options` table in the database.
    /// </summary>
    /// <returns></returns>
    public static Action<EntityTypeBuilder<Option>> Configuration() => (
        entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Changed).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ChangedBy).HasComment("user id (ON DELETE SET null)");
            entity.Property(e => e.Created).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.PlatformId).HasComment("platform_id (ON DELETE CASCADE)");

            entity.HasOne(d => d.ChangedByUser).WithMany(p => p.Options)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("options_ibfk_2");

            entity.HasOne(d => d.Platform).WithMany(p => p.Options).HasConstraintName("options_ibfk_1");
        }
    );

    /// <summary>
    /// Convert the '<see cref="Option"/>' entity to a '<see cref="OptionDTO"/>' instance.
    /// </summary>
    /// <returns><see cref="OptionDTO"/></returns>
    public object ToDataTransferObject() => (
        new
        {
            Id,
            PlatformId,
            Key,
            Value,
            Created,
            Changed,
            ChangedBy
        }
    );
}
