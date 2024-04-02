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

    /// <summary>
    /// Explicit conversion from '<see cref="UserDTO"/>' to '<see cref="User"/>' DB Model.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public override User ToModel() => new User()
    {
        /* Id = Id, */
        PlatformId = PlatformId ?? throw new ArgumentNullException(nameof(PlatformId)),
        Username = Username ?? throw new ArgumentNullException(nameof(Username)),
        FirstName = FirstName,
        LastName = LastName,
        Group = Group ?? throw new ArgumentNullException(nameof(Group)),
        Token = Token ?? throw new ArgumentNullException(nameof(Token)),
        Expires = Expires,
        Created = Created ?? throw new ArgumentNullException(nameof(Token)),
        Changed = Changed ?? DateTime.Now
    };

    /// <summary>
    /// Explicit conversion from '<see cref="User"/>' to '<see cref="UserDTO"/>'.
    /// </summary>
    /// <param name="model"></param>
    public override void FromModel(User model)
    {
        Id = model.Id;
        PlatformId = model.PlatformId;
        Username = model.Username;
        FirstName = model.FirstName;
        LastName = model.LastName;
        Group = model.Group;
        Token = model.Token;
        Expires = model.Expires;
        Created = model.Created;
        Changed = model.Changed;
    }

    /// <summary>
    /// Explicit conversion from '<see cref="User"/>' to '<see cref="UserDTO"/>' where the DTO's values are not overridden.
    /// </summary>
    /// <param name="model"></param>
    public override void FromModelNoOverride(User model)
    {
        Id ??= model.Id;
        PlatformId ??= model.PlatformId;
        Username ??= model.Username;
        FirstName ??= model.FirstName;
        LastName ??= model.LastName;
        Group ??= model.Group;
        Token ??= model.Token;
        Expires ??= model.Expires;
        Created ??= model.Created;
        Changed ??= model.Changed;
    }
}

/// <summary>
/// 
/// </summary>
public record NewUserJoinPlatform
{
    [JsonPropertyName("username")]
    [StringLength(63)]
    public string Username { get; set; } = null!;

    [JsonPropertyName("first_name")]
    [StringLength(63)]
    public string? FirstName { get; set; } = null;

    [JsonPropertyName("last_name")]
    [StringLength(63)]
    public string? LastName { get; set; } = null;
}

/// <summary>
/// 
/// </summary>
public record CreateUser : NewUserJoinPlatform
{
    /// <summary>
    /// Platform to register the new user on.
    /// </summary>
    [JsonPropertyName("platform_id")]
    public uint? PlatformId { get; set; }

    [JsonPropertyName("group")]
    public UserGroup? Group { get; set; } = null;

    /// <summary>ctor</summary>
    public CreateUser((uint, UserGroup)? platformDetails = null) {
        PlatformId = platformDetails?.Item1;
        Group = platformDetails?.Item2;
    }
    /// <summary>ctor</summary>
    public CreateUser(NewUserJoinPlatform newUserJoinPlatform, (uint, UserGroup)? platformDetails) { 
        Username = newUserJoinPlatform.Username;
        FirstName = newUserJoinPlatform.FirstName;
        LastName = newUserJoinPlatform.LastName;
        PlatformId = platformDetails?.Item1;
        Group = platformDetails?.Item2;
    }

    /// <summary>
    /// Explicit conversion from 'CreateUser' to 'UserDTO'.<br/>
    /// `null` values should be generated elsewhere.
    /// </summary>
    /// <param name="user"></param>
    public static implicit operator UserDTO(CreateUser user) => new UserDTO()
    {
        Username = user.Username,
        PlatformId = user.PlatformId,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Group = user.Group ?? UserGroup.Guest,
        Token = null,
        Expires = null,
        Created = DateTime.Now,
        Changed = DateTime.Now
    };
}