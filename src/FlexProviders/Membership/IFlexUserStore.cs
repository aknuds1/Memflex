using System.Collections.Generic;
using Microsoft.Web.WebPages.OAuth;

namespace FlexProviders.Membership
{
    public interface IFlexUserStore
    {        
        /// <summary>
        /// Add a user to the store.
        /// </summary>
        /// <param name="user">User to add.</param>
        /// <returns>The user.</returns>
        /// <exception cref="UserAlreadyExists">A user by that username already exists.</exception>
        IFlexMembershipUser Add(IFlexMembershipUser user);        
        /// <summary>
        /// Save modifications to a user.
        /// </summary>
        /// <param name="user">The user to save.</param>
        /// <returns>The user.</returns>
        /// <exception cref="UserNotFoundException">No user by that username exists in the store.</exception>
        IFlexMembershipUser Save(IFlexMembershipUser user);
        IFlexMembershipUser CreateOAuthAccount(string provider, string providerUserId, IFlexMembershipUser user);    
        /// <summary>
        /// Get a user by username.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <returns>The corresponding user or null.</returns
        IFlexMembershipUser GetUserByUsername(string username);
        IFlexMembershipUser GetUserByOAuthProvider(string provider, string providerUserId);        
        IEnumerable<OAuthAccount> GetOAuthAccountsForUser(string username);
        bool DeleteOAuthAccount(string provider, string providerUserId);
        IFlexMembershipUser GetUserByPasswordResetToken(string passwordResetToken);
    }    
}