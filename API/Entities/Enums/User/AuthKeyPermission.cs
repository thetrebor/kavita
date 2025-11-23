using System;
using System.ComponentModel;

namespace API.Entities.Enums.User;

[Flags]
public enum AuthKeyPermission
{
    None = 0,

    /// <summary>
    /// Ability to resolve images
    /// </summary>
    [Description("Image")]
    Image = 1 << 0,      // 1

    /// <summary>
    /// Ability to download files
    /// </summary>
    [Description("Download")]
    Download = 1 << 1,   // 2

    // Future permissions
    // Upload = 1 << 2,  // 4
    // Delete = 1 << 3,  // 8

    /// <summary>
    /// All current permissions
    /// </summary>
    [Description("All")]
    All = Image | Download  // 3
}
