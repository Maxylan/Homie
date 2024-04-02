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
/// The 'AccessLog' entity, reflects `g_access_logs` table in the database.
/// </summary>
[Table("g_access_logs")]
public partial record AccessLog : IBaseModel<AccessLog>
{
    /// <summary>PK</summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public uint? Id { get; set; }

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
    [JsonConverter(typeof(JsonStringEnumConverter))]
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

            entity.Property(e => e.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Method)
                .HasDefaultValueSql("'UNKNOWN'")
                .HasConversion<string>(
                    v => v.ToString(),
                    v => (HttpMethod) Enum.Parse(typeof(HttpMethod), v, true)
                );
        }
    );

    /// <summary>
    /// Convert the '<see cref="AccessLog"/>' entity to a '<see cref="AccessLogDTO"/>' instance.
    /// </summary>
    /// <returns><see cref="AccessLogDTO"/></returns>
    public object ToDataTransferObject() => (
        new
        {
            Id,
            PlatformId,
            UserId,
            Username,
            Timestamp,
            Ip,
            Method,
            Uri,
            Path,
            Parameters,
            FullUrl,
            Headers,
            Body,
            Response,
            ResponseStatus
        }
    );
}

[JsonConverter(typeof(JsonStringEnumConverter))]
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