namespace Kavita.Models.Entities.Interfaces;

/// <summary>
/// Provides localized, display-ready naming fields computed at API time.
/// </summary>
public interface IHasDisplayTitle
{
    /// <summary>
    /// The entity's type-specific designation, localized.
    /// Chapter: "Chapter 5", "Issue #5", "Book 5"
    /// Volume: "Volume 2", "Volume 1-4", "Band 2"
    /// ReadingListItem: "Chapter 5", "Issue #5", "Volume 2"
    /// For specials: cleaned special title. For loose-leaf with no number: ""
    /// </summary>
    string DisplayNumber { get; set; }

    /// <summary>
    /// Full composed display title, context-aware, localized.
    /// "Chapter 5 - The Battle Begins", "Volume 2 - Chapter 5", "Bonus"
    /// The "best" single string when you only have one slot.
    /// </summary>
    string DisplayTitle { get; set; }

    /// <summary>
    /// Content subtitle from metadata. For volumes: first chapter's title or volume name.
    /// For chapters: the chapter's TitleName. For reading list items: same as DisplayTitle.
    /// Empty string when not applicable (e.g., Image libraries).
    /// </summary>
    string MetaTitle { get; set; }
}
