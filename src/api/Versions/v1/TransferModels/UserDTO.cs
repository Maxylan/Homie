// (c) 2024 @Maxylan
namespace Homie.Api.v1.TransferModels;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Homie.Database.Models;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// The 'UserDTO' 
/// </summary>
public class UserDTO : DTO<User>
{
    [JsonPropertyName("id")]
    public uint? Id { get; set; }

    /// <summary>
    /// platform_id (ON DELETE CASCADE)
    /// </summary>
    [JsonPropertyName("platform_id")]
    public uint? PlatformId { get; set; }

    [JsonPropertyName("username")]
    [StringLength(63)]
    public string? Username { get; set; }

    [JsonPropertyName("first_name")]
    [StringLength(63)]
    public string? FirstName { get; set; }

    [JsonPropertyName("last_name")]
    [StringLength(63)]
    public string? LastName { get; set; }

    [JsonPropertyName("group")]
    public UserGroup? Group { get; set; } = UserGroup.Guest;

    [JsonPropertyName("token")]
    [StringLength(63)]
    public string? Token { get; set; }

    [JsonPropertyName("expires")]
    public DateTime? Expires { get; set; }

    [JsonPropertyName("created")]
    public DateTime? Created { get; set; }

    [JsonPropertyName("changed")]
    public DateTime? Changed { get; set; }
}

/// <summary>
/// 
/// </summary>
public record CreateUser
{
    /// <summary>
    /// Platform to register the new user on.
    /// </summary>
    [JsonPropertyName("platform_id")]
    public uint PlatformId { get; set; }

    [JsonPropertyName("username")]
    [StringLength(63)]
    public string Username { get; set; } = null!;

    [JsonPropertyName("group")]
    public UserGroup? Group { get; set; } = UserGroup.Guest;

    [JsonPropertyName("first_name")]
    [StringLength(63)]
    public string? FirstName { get; set; } = null;

    [JsonPropertyName("last_name")]
    [StringLength(63)]
    public string? LastName { get; set; } = null;

    /// <summary>
    /// Explicit conversion from 'CreateUser' to 'UserDTO'.<br/>
    /// `null` values should be generated elsewhere.
    /// </summary>
    /// <param name="user"></param>
    public static explicit operator UserDTO(CreateUser user) => new UserDTO()
    {
        Username = user.Username,
        PlatformId = user.PlatformId,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Group = UserGroup.Guest,
        Token = null,
        Expires = null,
        Created = DateTime.Now,
        Changed = DateTime.Now
    };
}