using System.ComponentModel.DataAnnotations;

namespace Cgmail.Common.Dto;

public abstract class GenericBaseDto<T>
{
    [Required]
    public T Id { get; set; } = default!;

    [Required] public bool IsActive { get; set; }
}
