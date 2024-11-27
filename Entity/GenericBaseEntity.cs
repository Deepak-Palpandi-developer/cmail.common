using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cgmail.Common.Entity;
public abstract class GenericBaseEntity<T>
{
    [Key, Column(Order = 0)]
    public T Id { get; set; } = default!;

    [Required, Column(Order = 100)]
    public bool IsActive { get; set; } = true;
}

