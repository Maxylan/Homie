using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homie.Database.Models;

[Table("reminder_tags")]
[Index("ReminderId", Name = "reminder_id")]
public partial record ReminderTag : IBaseModel<ReminderTag>
{
    [Key]
    [Column("id")]
    public uint Id { get; set; }

    /// <summary>
    /// reminder_id (ON DELETE CASCADE)
    /// </summary>
    [Column("reminder_id")]
    public uint ReminderId { get; set; }

    [Column("name")]
    [StringLength(63)]
    public string Name { get; set; } = null!;

    [ForeignKey("ReminderId")]
    [InverseProperty("ReminderTags")]
    public virtual Reminder Reminder { get; set; } = null!;

    /// <summary>
    /// The configuration for the 'ReminderTag' entity, reflects `reminder_tags` table in the database.
    /// </summary>
    /// <returns></returns>
    public static Action<EntityTypeBuilder<ReminderTag>> Configuration() => (
        entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.ReminderId).HasComment("reminder_id (ON DELETE CASCADE)");

            entity.HasOne(d => d.Reminder).WithMany(p => p.ReminderTags).HasConstraintName("reminder_tags_ibfk_1");
        }
    );
}
