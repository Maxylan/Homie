// (c) 2024 @Maxylan
// Created myself.
namespace Homie.Database.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public interface IBaseModel<T> where T : class
{
    // public abstract static Action<EntityTypeBuilder<T>> Configuration<T>() where T : class, IBaseModel;
    /// <summary>
    /// The configuration for the entity.
    /// </summary>
    /// <returns></returns>
    public abstract static Action<EntityTypeBuilder<T>> Configuration();
}