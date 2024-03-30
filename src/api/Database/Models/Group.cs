using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homie.Database.Models;

[Table("groups")]
[Index("ListId", Name = "list_id")]
public partial record Group : IBaseModel<Group>
{
    [Key]
    [Column("id")]
    public uint Id { get; set; }

    [Column("title")]
    [StringLength(127)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// Ascending
    /// </summary>
    [Column("order")]
    public uint Order { get; set; }

    /// <summary>
    /// list_id (ON DELETE CASCADE)
    /// </summary>
    [Column("list_id")]
    public uint ListId { get; set; }

    [ForeignKey("ListId")]
    [InverseProperty("Groups")]
    public virtual List List { get; set; } = null!;

    [InverseProperty("Group")]
    public virtual ICollection<Row> Rows { get; set; } = new List<Row>();

    /// <summary>
    /// The configuration for the 'Group' entity, reflects `groups` table in the database.
    /// </summary>
    /// <returns></returns>
    public static Action<EntityTypeBuilder<Group>> Configuration() => (
        entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.ListId).HasComment("list_id (ON DELETE CASCADE)");
            entity.Property(e => e.Order).HasComment("Ascending");

            entity.HasOne(d => d.List).WithMany(p => p.Groups).HasConstraintName("groups_ibfk_1");
        }
    );
}
