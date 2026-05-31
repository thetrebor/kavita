using System;
using System.Collections.Generic;
using Kavita.Models.Entities.Enums;

namespace Kavita.Models.DTOs.KavitaPlus.Account;

/// <summary>
/// Represents information around a user's tokens and their status
/// </summary>
public sealed record UserTokenInfo
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public List<TokenValidityInfo> Tokens { get; set; }
}

public sealed record TokenValidityInfo
{
    public ScrobbleProvider Provider { get; set; }
    public DateTime ValidUntilUtc { get; set; }
}
