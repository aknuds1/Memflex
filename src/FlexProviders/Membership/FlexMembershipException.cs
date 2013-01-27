using System;
using System.Runtime.Serialization;

namespace FlexProviders.Membership
{
    [Serializable]
    public class FlexMembershipException : Exception
    {
        public FlexMembershipStatus StatusCode { get; set; }

        public FlexMembershipException()
        {
        }

        public FlexMembershipException(string message)
            : base(message)
        {
        }

        public FlexMembershipException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public FlexMembershipException(FlexMembershipStatus statusCode)
        {
            this.StatusCode = statusCode;
        }

        protected FlexMembershipException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    ///  User not found.
    /// </summary>
    public class UserNotFoundException : FlexMembershipException
    {
        public UserNotFoundException(string username)
            : base(string.Format("No user by username '{0}' found", username))
        {
        }
    }

    /// <summary>
    /// A conflicting user was detected.
    /// </summary>
    public class UserAlreadyExists : FlexMembershipException
    {
        public UserAlreadyExists(string username)
            : base(string.Format("User '{0}' already exists", username))
        {
        }
    }

    public class RoleAlreadyExists : FlexMembershipException
    {
        public RoleAlreadyExists(string name)
            : base(string.Format("Role '{0}' already exists", name))
        {
        }
    }
}