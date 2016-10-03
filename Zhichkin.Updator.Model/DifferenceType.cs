namespace Zhichkin.Updator.Model
{
    public enum DifferenceType
    {
        /// <summary>
        /// No difference
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
