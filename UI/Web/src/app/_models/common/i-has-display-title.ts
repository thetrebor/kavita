export interface IHasDisplayTitle {
  /**
   * Pre-computed display designation, e.g. "Chapter 5", "Issue #5"
   */
  displayNumber: string;
  /**
   * Pre-computed full display title, e.g. "Chapter 5 - The Battle Begins"
   */
  displayTitle: string;
  /**
   * Pre-computed content subtitle from metadata
   */
  metaTitle: string;
}
