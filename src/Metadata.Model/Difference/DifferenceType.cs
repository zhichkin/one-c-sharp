namespace Zhichkin.Metadata.Model
{
    public enum DifferenceType
    {
        /// <summary>
        /// No difference (used just as a root of child differences)
        /// </summary>
        None,
        /// <summary>
        /// New item to add
        /// </summary>
        Insert,
        /// <summary>
        /// Some properties has been changed
        /// </summary>
        Update,
        /// <summary>
        /// Target item to be deleted
        /// </summary>
        Delete
    }
}
