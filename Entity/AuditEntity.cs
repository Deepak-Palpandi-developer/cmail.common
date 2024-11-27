using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cgmail.Common.Entity;


public abstract class AuditEntity : BaseEntity
{
    [Column(Order = 101)]
    public bool IsDeleted { get; set; }

    [Required, Column(Order = 103)]
    public DateTimeOffset CreatedAt { get; set; }

    [StringLength(30), Column(Order = 104)]
    public string CreatedIp { get; set; } = string.Empty;

    [Column(Order = 106)]
    public DateTimeOffset? UpdatedAt { get; set; }

    [StringLength(30), Column(Order = 107)]
    public string? UpdatedIp { get; set; }

    [Column(Order = 109)]
    public DateTimeOffset? DeletedAt { get; set; }

    [StringLength(30), Column(Order = 110)]
    public string? DeletedIp { get; set; }
}