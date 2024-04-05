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
[Index("PlatformId", Name = "platform_id")]
[Index("UserToken", Name = "user_token")]
public partial record AccessLog : IBaseModel<AccessLog>
{
    /// <summary>PK</summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public uint? Id { get; set; }

    /// <summary>
    /// platform_id (ON DELETE SET null)
    /// </summary>
    [Column("platform_id")]
    public uint? PlatformId { get; set; }

    /// <summary>
    /// user_token/token (ON DELETE SET null)
    /// </summary>
    [Column("user_token")]
    [StringLength(31)]
    public string? UserToken { get; set; }

    /// <summary>
    /// username of the requesting user
    /// </summary>
    [Column("username")]
    [StringLength(63)]
    public string? Username { get; set; }

    [Column("timestamp", TypeName = "datetime")]
    public DateTime Timestamp { get; set; } = DateTime.Now;

    [Column("version")]
    [StringLength(31)]
    public string? Version { get; set; }

    [Column("ip")]
    [StringLength(63)]
    public string Ip { get; set; } = null!;

    [Column("method", TypeName = "enum('GET','PUT','POST','DELETE','OPTIONS','HEAD','PATCH','UNKNOWN')")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HttpMethod Method { get; set; } = HttpMethod.UNKNOWN;

    [Column("uri")]
    [StringLength(255)]
    public string Uri { get; set; } = null!;

    [Column("path")]
    [StringLength(255)]
    public string Path { get; set; } = null!;

    [Column("parameters")]
    [StringLength(511)]
    public string Parameters { get; set; } = null!;

    [Column("full_url")]
    [StringLength(1023)]
    public string FullUrl { get; set; } = null!;

    [Column("headers", TypeName = "text")]
    public string? Headers { get; set; }

    [Column("body", TypeName = "text")]
    public string? Body { get; set; }

    [Column("response_message")]
    [StringLength(1023)]
    public string? ResponseMessage { get; set; }

    [Column("response_status")]
    public uint? ResponseStatus { get; set; }

    [ForeignKey("PlatformId")]
    [InverseProperty("AccessLogs")]
    public virtual Platform? Platform { get; set; }

    [ForeignKey("UserToken")]
    [InverseProperty("AccessLogs")]
    public virtual User? User { get; set; }

    /// <summary>
    /// The configuration for the 'AccessLog' entity, reflects `g_access_logs` table in the database.
    /// </summary>
    /// <returns></returns>
    public static Action<EntityTypeBuilder<AccessLog>> Configuration() => (
        entity => {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.PlatformId).HasComment("platform_id (ON DELETE SET null)");
            entity.Property(e => e.ResponseStatus).HasDefaultValueSql("'503'");
            entity.Property(e => e.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UserToken).HasComment("user_token/token (ON DELETE SET null)");
            entity.Property(e => e.Username).HasComment("username of the requesting user");
            entity.Property(e => e.Method)
                .HasDefaultValueSql("'UNKNOWN'")
                .HasConversion<string>(
                    v => v.ToString(),
                    v => (HttpMethod) Enum.Parse(typeof(HttpMethod), v, true)
                );


            entity.HasOne(d => d.Platform).WithMany(p => p.AccessLogs)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("g_access_logs_ibfk_1");

            entity.HasOne(d => d.User).WithMany(p => p.AccessLogs)
                .HasPrincipalKey(p => p.Token)
                .HasForeignKey(d => d.UserToken)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("g_access_logs_ibfk_2");
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
            UserToken,
            Username,
            Timestamp,
            Version,
            Ip,
            Method,
            Uri,
            Path,
            Parameters,
            FullUrl,
            Headers,
            Body,
            ResponseMessage,
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