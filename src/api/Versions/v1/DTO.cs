// (c) 2024 @Maxylan
namespace Homie.Api.v1;

using Homie.Database.Models;
using Microsoft.AspNetCore.Mvc;

public interface iDTO { }

/// <summary>
/// "Base" Data Transfer Object, used to transfer data between the API and the database by defining conversions between the two.
/// </summary>
/// <typeparam name="M">"Add"</typeparam>
public abstract class DTO<M> : iDTO where M : class, IBaseModel<M>
{
    /// <summary>
    /// Explicit conversion from 'DTO' to 'Model' DB Model.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public abstract M ToModel();

    /// <summary>
    /// Explicit conversion from 'Model' to 'DTO'.
    /// </summary>
    /// <param name="model"></param>
    public abstract void FromModel(M model);

    /// <summary>
    /// Explicit conversion from 'Model' to 'DTO' where the DTO's values are not overridden.
    /// </summary>
    /// <param name="model"></param>
    public abstract void FromModelNoOverride(M model);
    
    public static implicit operator M(DTO<M> dto) => dto.ToModel();
    public static implicit operator DTO<M>(M model) => (DTO<M>) model.ToDataTransferObject();
}

