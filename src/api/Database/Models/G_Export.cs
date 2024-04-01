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
/// The 'Export' entity, reflects `g_exports` table in the database.
/// </summary>
[Table("g_exports")]
[Index("Code", Name = "code", IsUnique = true)]
[Index("PlatformId", Name = "platform_id")]
public partial record Export : IBaseModel<Export>
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
    /// PK Of the exported resource
    /// </summary>
    [Column("resource_id")]
    public uint ResourceId { get; set; }

    [Column("code")]
    [StringLength(63)]
    public string Code { get; set; } = null!;

    [Column("created", TypeName = "datetime")]
    public DateTime Created { get; set; } = DateTime.Now;

    [Column("expires", TypeName = "datetime")]
    public DateTime Expires { get; set; } = DateTime.Now;

    [ForeignKey("PlatformId")]
    [InverseProperty("Exports")]
    public virtual Platform Platform { get; set; } = null!;

    /// <summary>
    /// The configuration for the 'Export' entity, reflects `g_exports` table in the database.
    /// </summary>
    /// <returns></returns>
    public static Action<EntityTypeBuilder<Export>> Configuration() => (
        entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Created).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Expires).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.PlatformId).HasComment("platform_id (ON DELETE CASCADE)");
            entity.Property(e => e.ResourceId).HasComment("PK Of the exported resource");

            entity.HasOne(d => d.Platform).WithMany(p => p.Exports).HasConstraintName("g_exports_ibfk_1");
        }
    );

    /// <summary>
    /// Convert the '<see cref="Export"/>' entity to a '<see cref="ExportDTO"/>' instance.
    /// </summary>
    /// <returns><see cref="ExportDTO"/></returns>
    public object ToDataTransferObject() => (
        new
        {
            Id,
            PlatformId,
            ResourceId,
            Code,
            Created,
            Expires
        }
    );
}
