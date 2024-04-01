// (c) 2024 @Maxylan
// Created myself.
namespace Homie.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public interface IBaseModel<T> where T : class
{
    /// <summary>PK</summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public uint? Id { get; set; }

    // public abstract static Action<EntityTypeBuilder<T>> Configuration<T>() where T : class, IBaseModel;
    /// <summary>
    /// The configuration for the entity.
    /// </summary>
    /// <returns></returns>
    public abstract static Action<EntityTypeBuilder<T>> Configuration();

    // public abstract DTO<O> ToDataTransferObject<O>() where O : class, IBaseModel<O>;
    public abstract object ToDataTransferObject();
}