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
/// The 'ReminderTag' entity, reflects `reminder_tags` table in the database.
/// </summary>
[Table("reminder_tags")]
[Index("ReminderId", Name = "reminder_id")]
public partial record ReminderTag : IBaseModel<ReminderTag>
{
    /// <summary>PK</summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public uint? Id { get; set; }

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

    /// <summary>
    /// Convert the '<see cref="RemindersTag"/>' entity to a '<see cref="RemindersTagDTO"/>' instance.
    /// </summary>
    /// <returns><see cref="RemindersTagDTO"/></returns>
    public object ToDataTransferObject() => (
        new
        {
            Id = Id,
            ReminderId = ReminderId,
            Name = Name
        }
    );
}
