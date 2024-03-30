﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homie.Database.Models;

[Table("g_product_indexes")]
[Index("ProductId", Name = "product_id")]
public partial record ProductIndex : IBaseModel<ProductIndex>
{
    [Key]
    [Column("id")]
    public uint Id { get; set; }

    [Column("product_id")]
    public uint ProductId { get; set; }

    [Column("type", TypeName = "enum('category','super','bf','tag')")]
    public string Type { get; set; } = null!;

    [Column("name")]
    [StringLength(63)]
    public string Name { get; set; } = null!;

    [Column("value")]
    [StringLength(255)]
    public string? Value { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("ProductIndices")]
    public virtual Product Product { get; set; } = null!;

    /// <summary>
    /// The configuration for the 'ProductIndex' entity, reflects `g_product_indexes` table in the database.
    /// </summary>
    /// <returns></returns>
    public static Action<EntityTypeBuilder<ProductIndex>> Configuration() => (
        entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductIndices).HasConstraintName("g_product_indexes_ibfk_1");
        }
    );
}