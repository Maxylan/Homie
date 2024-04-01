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
/// The 'Reminder' entity, reflects `reminders` table in the database.
/// </summary>
[Table("reminders")]
[Index("Author", Name = "author")]
[Index("ChangedBy", Name = "changed_by")]
[Index("PlatformId", Name = "platform_id")]
public partial record Reminder : IBaseModel<Reminder>
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
    public Visibilities Visibility { get; set; } = Visibilities.Global;

    [Column("message")]
    [StringLength(127)]
    public string? Message { get; set; }

    [Column("deadline", TypeName = "datetime")]
    public DateTime Deadline { get; set; } = DateTime.Now;

    /// <summary>
    /// If this reminder should always be displayed on the dashboard
    /// </summary>
    [Column("has_show_always")]
    public bool HasShowAlways { get; set; } = false;

    [Column("has_interval")]
    public bool HasInterval { get; set; } = false;

    /// <summary>
    /// If has_interval (repeating) is true, this is the cron expression
    /// </summary>
    [Column("interval")]
    [StringLength(63)]
    public string? Interval { get; set; } = null;

    [Column("has_push_notification")]
    public bool HasPushNotification { get; set; } = false;

    /// <summary>
    /// If has_push_notification is true, this is the TIME that a notification should be sent (relative to deadline)
    /// </summary>
    [Column("notification_deadline", TypeName = "time")]
    public TimeSpan? NotificationDeadline { get; set; } = TimeSpan.Parse("12:00:00");

    [Column("has_alarm")]
    public bool HasAlarm { get; set; } = false;

    /// <summary>
    /// If has_alarm is true, this flags if the alarm should vibrate
    /// </summary>
    [Column("has_alarm_vibration")]
    public bool HasAlarmVibration { get; set; } = false;

    /// <summary>
    /// If has_alarm is true, this flags if the alarm should be silent
    /// </summary>
    [Column("has_alarm_sound")]
    public bool HasAlarmSound { get; set; } = false;

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

    [Column("archive_after", TypeName = "datetime")]
    public DateTime ArchiveAfter { get; set; }

    [ForeignKey("Author")]
    [InverseProperty("ReminderCreatedByUsers")]
    public virtual User? CreatedByUser { get; set; }

    [ForeignKey("ChangedBy")]
    [InverseProperty("ReminderChangedByUsers")]
    public virtual User? ChangedByUser { get; set; }

    [ForeignKey("PlatformId")]
    [InverseProperty("Reminders")]
    public virtual Platform Platform { get; set; } = null!;

    [InverseProperty("Reminder")]
    public virtual ICollection<ReminderTag> ReminderTags { get; set; } = new List<ReminderTag>();

    /// <summary>
    /// The configuration for the 'Reminder' entity, reflects `reminders` table in the database.
    /// </summary>
    /// <returns></returns>
    public static Action<EntityTypeBuilder<Reminder>> Configuration() => (
        entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.ArchiveAfter).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Author).HasComment("user id (ON DELETE SET null)");
            entity.Property(e => e.Changed).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ChangedBy).HasComment("user id (ON DELETE SET null)");
            entity.Property(e => e.Created).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Deadline).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.HasAlarmSound).HasComment("If has_alarm is true, this flags if the alarm should be silent");
            entity.Property(e => e.HasAlarmVibration).HasComment("If has_alarm is true, this flags if the alarm should vibrate");
            entity.Property(e => e.HasShowAlways).HasComment("If this reminder should always be displayed on the dashboard");
            entity.Property(e => e.Interval).HasComment("If has_interval (repeating) is true, this is the cron expression");
            entity.Property(e => e.NotificationDeadline).HasComment("If has_push_notification is true, this is the TIME that a notification should be sent (relative to deadline)");
            entity.Property(e => e.PlatformId).HasComment("platform_id (ON DELETE CASCADE)");
            entity.Property(e => e.Visibility)
                .HasComment("enum('private','selective','inclusive','members','global')")
                .HasDefaultValueSql("'global'")
                .HasConversion<string>(
                    v => v.ToString(),
                    v => (Visibilities)Enum.Parse(typeof(Visibilities), v)
                );

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.ReminderCreatedByUsers)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("reminders_ibfk_2");

            entity.HasOne(d => d.ChangedByUser).WithMany(p => p.ReminderChangedByUsers)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("reminders_ibfk_3");

            entity.HasOne(d => d.Platform).WithMany(p => p.Reminders).HasConstraintName("reminders_ibfk_1");
        }
    );

    /// <summary>
    /// Convert the '<see cref="Reminder"/>' entity to a '<see cref="ReminderDTO"/>' instance.
    /// </summary>
    /// <returns><see cref="ReminderDTO"/></returns>
    public object ToDataTransferObject() => (
        new
        {
            Id,
            PlatformId,
            Visibility,
            Message,
            Deadline,
            HasShowAlways,
            HasInterval,
            Interval,
            HasPushNotification,
            NotificationDeadline,
            HasAlarm,
            HasAlarmVibration,
            HasAlarmSound,
            Author,
            Created,
            Changed,
            ChangedBy,
            ArchiveAfter
        }
    );
}
