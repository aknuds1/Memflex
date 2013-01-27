using System.Collections.Generic;

namespace FlexProviders.Roles
{
    /// <summary>
    /// Represent a role.
    /// </summary>
    /// <typeparam name="T">The user type.</typeparam>
    public interface IFlexRole<T>
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the users.
        /// </summary>
        /// <value>
        /// The users.
        /// </value>
        ICollection<T> Users { get; set; }
    }
}