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
/// The 'Note' entity, reflects `notes` table in the database.
/// </summary>
[Table("notes")]
[Index("Author", Name = "author")]
[Index("ChangedBy", Name = "changed_by")]
[Index("PlatformId", Name = "platform_id")]
public partial record Note : IBaseModel<Note>
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

    [Column("visibility", TypeName = "enum('private','selective','inclusive','members','global')")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Visibilities Visibility { get; set; } = Visibilities.Global;

    [Column("message", TypeName = "mediumtext")]
    public string? Message { get; set; }

    [Column("color")]
    [StringLength(63)]
    public string? Color { get; set; }

    [Column("pin")]
    public bool Pin { get; set; } = false;

    [Column("pinned", TypeName = "datetime")]
    public DateTime? Pinned { get; set; } = null;

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
    [InverseProperty("NoteCreatedByUsers")]
    public virtual User? CreatedByUser { get; set; }

    [ForeignKey("ChangedBy")]
    [InverseProperty("NoteChangedByUsers")]
    public virtual User? ChangedByUser { get; set; }

    [ForeignKey("PlatformId")]
    [InverseProperty("Notes")]
    public virtual Platform Platform { get; set; } = null!;

    /// <summary>
    /// The configuration for the 'Note' entity, reflects `notes` table in the database.
    /// </summary>
    /// <returns></returns>
    public static Action<EntityTypeBuilder<Note>> Configuration() => (
        entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Author).HasComment("user id (ON DELETE SET null)");
            entity.Property(e => e.Changed).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ChangedBy).HasComment("user id (ON DELETE SET null)");
            entity.Property(e => e.Created).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.PlatformId).HasComment("platform_id (ON DELETE CASCADE)");
            entity.Property(e => e.Visibility)
                .HasComment("enum('private','selective','inclusive','members','global')")
                .HasDefaultValueSql("'global'")
                .HasConversion<string>(
                    v => v.ToString(),
                    v => (Visibilities) Enum.Parse(typeof(Visibilities), v, true)
                );

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.NoteCreatedByUsers)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("notes_ibfk_2");

            entity.HasOne(d => d.ChangedByUser).WithMany(p => p.NoteChangedByUsers)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("notes_ibfk_3");

            entity.HasOne(d => d.Platform).WithMany(p => p.Notes).HasConstraintName("notes_ibfk_1");
        }
    );

    /// <summary>
    /// Convert the '<see cref="Note"/>' entity to a '<see cref="NoteDTO"/>' instance.
    /// </summary>
    /// <returns><see cref="NoteDTO"/></returns>
    public object ToDataTransferObject() => (
        new
        {
            Id,
            PlatformId,
            Visibility,
            Message,
            Color,
            Pin,
            Pinned,
            Author,
            Created,
            Changed,
            ChangedBy
        }
    );
}
