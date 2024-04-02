// (c) 2024 @Maxylan
namespace Homie.Api.v1.TransferModels;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Homie.Database.Models;

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

    /// <summary>
    /// Explicit conversion from '<see cref="PlatformDTO"/>' to '<see cref="Platform"/>' DB Model.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public override Platform ToModel() => new Platform()
    {
        /* Id = Id, */
        Name = Name ?? throw new ArgumentNullException(nameof(Name)),
        GuestCode = GuestCode ?? throw new ArgumentNullException(nameof(GuestCode)),
        MemberCode = MemberCode ?? throw new ArgumentNullException(nameof(MemberCode)),
        MasterPswd = MasterPswd ?? throw new ArgumentNullException(nameof(MasterPswd)),
        ResetToken = ResetToken ?? throw new ArgumentNullException(nameof(ResetToken)),
        Created = Created  ?? throw new ArgumentNullException(nameof(Created))
    };

    /// <summary>
    /// Explicit conversion from '<see cref="Platform"/>' to '<see cref="PlatformDTO"/>'.
    /// </summary>
    /// <param name="model"></param>
    public override void FromModel(Platform model)
    {
        Id = model.Id;
        Name = model.Name;
        GuestCode = model.GuestCode;
        MemberCode = model.MemberCode;
        MasterPswd = model.MasterPswd;
        ResetToken = model.ResetToken;
        Created = model.Created;
    }

    /// <summary>
    /// Explicit conversion from '<see cref="Platform"/>' to '<see cref="PlatformDTO"/>' where the DTO's values are not overridden.
    /// </summary>
    /// <param name="model"></param>
    public override void FromModelNoOverride(Platform model)
    {
        Id ??= model.Id;
        Name ??= model.Name;
        GuestCode ??= model.GuestCode;
        MemberCode ??= model.MemberCode;
        MasterPswd ??= model.MasterPswd;
        ResetToken ??= model.ResetToken;
        Created ??= model.Created;
    }
}

/// <summary>
/// The 'CreatePlatform' model, used to create a new platform.
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
    [JsonPropertyName("username")]
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
/// The 'CreatePlatformSuccess' model, including the created 'PlatformDTO' and 'UserDTO'.
/// </summary>
public record CreatePlatformSuccess
{
    /// <summary>
    /// <see cref="PlatformDTO"/>
    /// </summary>
    [JsonPropertyName("platform")]
    public PlatformDTO Platform { get; init; }
    
    /// <summary>
    /// <see cref="UserDTO"/>
    /// </summary>
    [JsonPropertyName("user")]
    public UserDTO User { get; init; }

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }

    public CreatePlatformSuccess(PlatformDTO platform, UserDTO user) 
    {
        if (platform.Id is null) { throw new ArgumentNullException(nameof(platform.Id)); }
        if (platform.GuestCode is null) { throw new ArgumentNullException(nameof(platform.GuestCode)); }
        if (platform.MemberCode is null) { throw new ArgumentNullException(nameof(platform.MemberCode)); }
        if (platform.ResetToken is null) { throw new ArgumentNullException(nameof(platform.ResetToken)); }
        if (user.Username is null) { throw new ArgumentNullException(nameof(user.Username)); }
        if (user.Token is null) { throw new ArgumentNullException(nameof(user.Token)); }

        Platform = platform;
        User = user;
        Created = platform.Created ?? DateTime.Now;
    }
}