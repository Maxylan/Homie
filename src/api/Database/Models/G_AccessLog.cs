using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Homie.Database.Models;

/// <summary>
/// The 'AccessLog' entity, reflects `g_access_logs` table in the database.
/// </summary>
/// <returns></returns>
[Table("g_access_logs")]
public partial record AccessLog : IBaseModel<AccessLog>
{
    [Key]
    [Column("id")]
    public ulong Id { get; set; }

    [Column("platform_id")]
    public uint? PlatformId { get; set; }

    [Column("user_id")]
    public uint? UserId { get; set; }

    [Column("username")]
    public uint? Username { get; set; }

    [Column("timestamp", TypeName = "datetime")]
    public DateTime Timestamp { get; set; } = DateTime.Now;

    [Column("ip")]
    [StringLength(63)]
    public string Ip { get; set; } = null!;

    [Column("method", TypeName = "enum('GET','PUT','POST','DELETE','OPTIONS','HEAD','PATCH','UNKNOWN')")]
    public HttpMethod Method { get; set; } = HttpMethod.UNKNOWN;

    [Column("uri")]
    [StringLength(127)]
    public string Uri { get; set; } = null!;

    [Column("path")]
    [StringLength(255)]
    public string Path { get; set; } = null!;

    [Column("parameters")]
    [StringLength(255)]
    public string Parameters { get; set; } = null!;

    [Column("full_url")]
    [StringLength(511)]
    public string FullUrl { get; set; } = null!;

    [Column("headers")]
    [StringLength(1023)]
    public string Headers { get; set; } = null!;

    [Column("body", TypeName = "text")]
    public string? Body { get; set; }

    [Column("response", TypeName = "text")]
    public string? Response { get; set; }

    [Column("response_status")]
    public uint? ResponseStatus { get; set; }

    /// <summary>
    /// The configuration for the 'AccessLog' entity, reflects `g_access_logs` table in the database.
    /// </summary>
    /// <returns></returns>
    public static Action<EntityTypeBuilder<AccessLog>> Configuration() => (
        entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Method).HasDefaultValueSql("'UNKNOWN'");
            entity.Property(e => e.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    );
}

public enum HttpMethod
{
    GET,
    PUT,
    POST,
    DELETE,
    OPTIONS,
    HEAD,
    PATCH,
    UNKNOWN
}