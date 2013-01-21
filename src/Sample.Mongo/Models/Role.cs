using System.Collections.Generic;
using FlexProviders.Roles;
using MongoDB.Bson;

namespace LogMeIn.Models
{
    public class Role : IFlexRole<ObjectId>
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the users.
        /// </summary>
        /// <value>
        /// The users.
        /// </value>
        public ICollection<ObjectId> Users { get; set; }
    }
}