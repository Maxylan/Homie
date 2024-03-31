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
    
}

