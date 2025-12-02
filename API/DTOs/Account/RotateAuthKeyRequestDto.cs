using System;
using System.ComponentModel.DataAnnotations;
using API.Helpers;

namespace API.DTOs.Account;

public sealed record RotateAuthKeyRequestDto
{
    [MinLength(AuthKeyHelper.MinKeyLength)]
    [MaxLength(AuthKeyHelper.MaxKeyLength)]
    [Required]
    public int KeyLength { get; set; }

    public required string Name { get; set; }
    public DateTime? ExpiresUtc { get; set; }
}
