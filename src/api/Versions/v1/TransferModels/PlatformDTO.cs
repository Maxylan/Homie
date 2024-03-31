// (c) 2024 @Maxylan
namespace Homie.Api.v1.TransferModels;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Homie.Database.Models;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// The 'PlatformDTO' 
/// </summary>
public class PlatformDTO : DTO<Platform>
{
    [JsonPropertyName("id")]
    public uint? Id { get; set; }

    [JsonPropertyName("name")]
    [StringLength(63)]
    public string? Name { get; set; }

    [JsonPropertyName("guest_code")]
    [StringLength(63)]
    public string? GuestCode { get; set; }

    [JsonPropertyName("member_code")]
    [StringLength(63)]
    public string? MemberCode { get; set; }

    [JsonPropertyName("master_pswd")]
    [StringLength(63)]
    public string? MasterPswd { get; set; }

    [JsonPropertyName("reset_token")]
    [StringLength(63)]
    public string? ResetToken { get; set; }

    [JsonPropertyName("created")]
    public DateTime? Created { get; set; }
}

/// <summary>
/// 
/// </summary>
public record CreatePlatform
{
    [JsonPropertyName("name")]
    [StringLength(63)]
    public string Name { get; set; } = null!;

    [JsonPropertyName("master_pswd")]
    [StringLength(63)]
    public string MasterPswd { get; set; } = null!;

    /// <summary>
    /// <see cref="UserDTO.Username"/>
    /// </summary>
    [JsonPropertyName("master_pswd")]
    [StringLength(63)]
    public string Username { get; set; } = null!;

    /// <summary>
    /// Explicit conversion from 'CreatePlatform' to 'PlatformDTO'.<br/>
    /// `null` values should be generated elsewhere.
    /// </summary>
    /// <param name="platform"></param>
    public static explicit operator PlatformDTO(CreatePlatform platform) => new PlatformDTO()
    {
        Name = platform.Name,
        GuestCode = null,
        MemberCode = null,
        MasterPswd = platform.MasterPswd,
        ResetToken = null,
        Created = DateTime.Now
    };
}

/// <summary>
/// 
/// </summary>
public record CreatePlatformSuccess : CreatePlatform
{
    [JsonPropertyName("id")]
    public uint Id { get; init; }

    [JsonPropertyName("guest_code")]
    [StringLength(63)]
    public string GuestCode { get; init; } = null!;

    [JsonPropertyName("member_code")]
    [StringLength(63)]
    public string MemberCode { get; init; } = null!;

    [JsonPropertyName("reset_token")]
    [StringLength(63)]
    public string ResetToken { get; init; } = null!;

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }

    /// <summary>
    /// <see cref="UserDTO.Token"/>
    /// </summary>
    [JsonPropertyName("token")]
    [StringLength(63)]
    public string Token { get; init; } = null!;

    public CreatePlatformSuccess(PlatformDTO platform, UserDTO user) 
    {
        if (platform.Id is null) { throw new ArgumentNullException(nameof(platform.Id)); }
        if (platform.GuestCode is null) { throw new ArgumentNullException(nameof(platform.GuestCode)); }
        if (platform.MemberCode is null) { throw new ArgumentNullException(nameof(platform.MemberCode)); }
        if (platform.ResetToken is null) { throw new ArgumentNullException(nameof(platform.ResetToken)); }
        if (user.Username is null) { throw new ArgumentNullException(nameof(user.Username)); }
        if (user.Token is null) { throw new ArgumentNullException(nameof(user.Token)); }

        Id = (uint) platform.Id!;
        Name = platform.Name!;
        GuestCode = platform.GuestCode;
        MemberCode = platform.MemberCode;
        MasterPswd = platform.MasterPswd!;
        ResetToken = platform.ResetToken;
        Created = platform.Created ?? DateTime.Now;
        Username = user.Username;
        Token = user.Token;
    }
}