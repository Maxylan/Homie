using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homie.Database.Models;

[Table("notes")]
[Index("Author", Name = "author")]
[Index("ChangedBy", Name = "changed_by")]
[Index("PlatformId", Name = "platform_id")]
public partial record Note : IBaseModel<Note>
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
    public string Visibility { get; set; } = null!;

    [Column("message", TypeName = "mediumtext")]
    public string? Message { get; set; }

    [Column("color")]
    [StringLength(63)]
    public string? Color { get; set; }

    [Column("pin")]
    public bool Pin { get; set; }

    [Column("pinned", TypeName = "datetime")]
    public DateTime? Pinned { get; set; }

    /// <summary>
    /// user id (ON DELETE SET null)
    /// </summary>
    [Column("author")]
    public uint? Author { get; set; }

    [Column("created", TypeName = "datetime")]
    public DateTime Created { get; set; }

    [Column("changed", TypeName = "datetime")]
    public DateTime Changed { get; set; }

    /// <summary>
    /// user id (ON DELETE SET null)
    /// </summary>
    [Column("changed_by")]
    public uint? ChangedBy { get; set; }

    [ForeignKey("Author")]
    [InverseProperty("NoteAuthorNavigations")]
    public virtual User? AuthorNavigation { get; set; }

    [ForeignKey("ChangedBy")]
    [InverseProperty("NoteChangedByNavigations")]
    public virtual User? ChangedByNavigation { get; set; }

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
            entity.Property(e => e.Visibility).HasDefaultValueSql("'global'");

            entity.HasOne(d => d.AuthorNavigation).WithMany(p => p.NoteAuthorNavigations)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("notes_ibfk_2");

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.NoteChangedByNavigations)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("notes_ibfk_3");

            entity.HasOne(d => d.Platform).WithMany(p => p.Notes).HasConstraintName("notes_ibfk_1");
        }
    );
}
