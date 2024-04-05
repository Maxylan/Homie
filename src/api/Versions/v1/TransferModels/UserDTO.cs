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

    [JsonIgnore] // [JsonPropertyName("token")]
    [StringLength(31)]
    public string? Token { get; set; }

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

    [JsonPropertyName("expires")]
    public DateTime? Expires { get; set; }

    [JsonPropertyName("created")]
    public DateTime? Created { get; set; }

    [JsonPropertyName("changed")]
    public DateTime? Changed { get; set; }

    [JsonPropertyName("last_seen")]
    public DateTime? LastSeen { get; set; }

    /// <summary>
    /// Explicit conversion from '<see cref="UserDTO"/>' to '<see cref="User"/>' DB Model.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public override User ToModel() => new User()
    {
        /* Id = Id, */
        Token = Token ?? throw new ArgumentNullException(nameof(Token)),
        PlatformId = PlatformId ?? throw new ArgumentNullException(nameof(PlatformId)),
        Username = Username ?? throw new ArgumentNullException(nameof(Username)),
        FirstName = FirstName,
        LastName = LastName,
        Group = Group ?? throw new ArgumentNullException(nameof(Group)),
        Expires = Expires,
        Created = Created ?? throw new ArgumentNullException(nameof(Token)),
        Changed = Changed ?? DateTime.Now,
        LastSeen = LastSeen ?? DateTime.Now
    };

    /// <summary>
    /// Explicit conversion from '<see cref="User"/>' to '<see cref="UserDTO"/>'.
    /// </summary>
    /// <param name="model"></param>
    public override void FromModel(User model)
    {
        Id = model.Id;
        Token = model.Token;
        PlatformId = model.PlatformId;
        Username = model.Username;
        FirstName = model.FirstName;
        LastName = model.LastName;
        Group = model.Group;
        Expires = model.Expires;
        Created = model.Created;
        Changed = model.Changed;
        LastSeen = model.LastSeen;
    }

    /// <summary>
    /// Explicit conversion from '<see cref="User"/>' to '<see cref="UserDTO"/>' where the DTO's values are not overridden.
    /// </summary>
    /// <param name="model"></param>
    public override void FromModelNoOverride(User model)
    {
        Id ??= model.Id;
        Token ??= model.Token;
        PlatformId ??= model.PlatformId;
        Username ??= model.Username;
        FirstName ??= model.FirstName;
        LastName ??= model.LastName;
        Group ??= model.Group;
        Expires ??= model.Expires;
        Created ??= model.Created;
        Changed ??= model.Changed;
        LastSeen ??= model.LastSeen;
    }

    /// <summary>
    /// Implicit conversion from 'UserDTO' to 'CompleteUserDTO', revealing the user's token.<br/>
    /// Essentially "unceonsoring" the 'UserDTO'.
    /// </summary>
    /// <param name="user"></param>
    public static implicit operator CompleteUserDTO(UserDTO user) => new CompleteUserDTO()
    {
        Id = user.Id ?? throw new ArgumentNullException(nameof(user.Id)),
        Token = user.Token ?? throw new ArgumentNullException(nameof(user.Token)),
        PlatformId = user.PlatformId ?? throw new ArgumentNullException(nameof(user.PlatformId)),
        Username = user.Username ?? throw new ArgumentNullException(nameof(user.Username)),
        FirstName = user.FirstName ?? throw new ArgumentNullException(nameof(user.FirstName)),
        LastName = user.LastName ?? throw new ArgumentNullException(nameof(user.LastName)),
        Group = user.Group ?? throw new ArgumentNullException(nameof(user.Group)),
        Expires = user.Expires,
        Created = user.Created ?? throw new ArgumentNullException(nameof(user.Created)),
        Changed = user.Changed ?? DateTime.Now,
        LastSeen = user.LastSeen ?? DateTime.Now
    };
}

/// <summary>
/// The 'UserDTO' - "uncensored" variant.
/// </summary>
public class CompleteUserDTO
{
    [JsonPropertyName("id")]
    public uint Id { get; set; }

    /// <summary>
    /// platform_id (ON DELETE CASCADE)
    /// </summary>
    [JsonPropertyName("platform_id")]
    public uint PlatformId { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; } = null!;

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = null!;

    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = null!;

    [JsonPropertyName("group")]
    public UserGroup Group { get; set; } = UserGroup.Guest;

    [JsonPropertyName("token")]
    public string Token { get; set; } = null!;

    [JsonPropertyName("expires")]
    public DateTime? Expires { get; set; }

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }

    [JsonPropertyName("changed")]
    public DateTime Changed { get; set; }

    [JsonPropertyName("last_seen")]
    public DateTime? LastSeen { get; set; }
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
    /// Implicit conversion from 'CreateUser' to 'UserDTO'.<br/>
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