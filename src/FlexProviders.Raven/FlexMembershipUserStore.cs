using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FlexProviders.Membership;
using FlexProviders.Roles;
using Microsoft.Web.WebPages.OAuth;
using Raven.Client;

namespace FlexProviders.Raven
{
    public class FlexMembershipUserStore<TUser, TRole>
        : IFlexUserStore, IFlexRoleStore
        where TUser : class, IFlexMembershipUser, new()
        where TRole : class, IFlexRole<string>, new()
    {
        protected readonly IDocumentStore DocumentStore;

        public FlexMembershipUserStore(IDocumentStore documentStore)
        {
            DocumentStore = documentStore;
        }

        public IFlexMembershipUser GetUserByUsername(string username)
        {
            using (var session = DocumentStore.OpenSession())
                return session.Query<TUser>().SingleOrDefault(u => u.Username == username);
        }

        public IFlexMembershipUser Add(IFlexMembershipUser user)
        {
            if (string.IsNullOrWhiteSpace(user.Username))
                throw new ArgumentException("The user must have a valid username");
            using (var session = DocumentStore.OpenSession())
            {
                if (session.Query<TUser>().Any(u => u.Username == user.Username))
                    throw new UserAlreadyExists(user.Username);
                session.Store(user);
                session.SaveChanges();
                var users = session.Query<TUser>().ToArray();
            }
            return user;
        }

        public IFlexMembershipUser Save(IFlexMembershipUser user)
        {
            using (var session = DocumentStore.OpenSession())
            {
                var existingUser = session.Query<TUser>().SingleOrDefault(u => u.Username == user.Username);
                if (existingUser == null)
                    throw new UserNotFoundException(user.Username);

                session.Store(user);
                session.SaveChanges();
            }
            return user;
        }

        public bool DeleteOAuthAccount(string provider, string providerUserId)
        {
            using (var session = DocumentStore.OpenSession())
            {
                var user =
                    session.Query<TUser>()
                                 .SingleOrDefault(u => u.OAuthAccounts
                                                        .Any(
                                                            o =>
                                                            o.ProviderUserId == providerUserId && o.Provider == provider));

                if (user != null)
                {
                    var account =
                        user.OAuthAccounts.Single(o => o.Provider == provider && o.ProviderUserId == providerUserId);
                    user.OAuthAccounts.Remove(account);
                    session.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public IFlexMembershipUser GetUserByPasswordResetToken(string passwordResetToken)
        {
            using (var session = DocumentStore.OpenSession())
                return
                    session.Query<TUser>().SingleOrDefault(u => u.PasswordResetToken == passwordResetToken);
        }

        public IFlexMembershipUser GetUserByOAuthProvider(string provider, string providerUserId)
        {
            using (var session = DocumentStore.OpenSession())
                return
                    session.Query<TUser>().SingleOrDefault(u => u.OAuthAccounts.Any(r => r.Provider == provider && r.ProviderUserId == providerUserId));
        }

        public IFlexMembershipUser CreateOAuthAccount(string provider, string providerUserId, IFlexMembershipUser user)
        {
            using (var session = DocumentStore.OpenSession())
            {
                var account = new FlexOAuthAccount { Provider = provider, ProviderUserId = providerUserId };
                if (user.OAuthAccounts == null)
                {
                    user.OAuthAccounts = new Collection<FlexOAuthAccount>();
                }
                user.OAuthAccounts.Add(account);
                session.SaveChanges();
            }

            return user;
        }

        public IEnumerable<OAuthAccount> GetOAuthAccountsForUser(string username)
        {
            using (var session = DocumentStore.OpenSession())
            {
                return session
                    .Query<TUser>()
                    .Single(u => u.Username == username)
                    .OAuthAccounts
                    .ToArray()
                    .Select(o => new OAuthAccount(o.Provider, o.ProviderUserId));
            }
        }

        public void CreateRole(string roleName)
        {
            using (var session = DocumentStore.OpenSession())
            {
                if (session.Query<TRole>().Any(r => r.Name == roleName))
                    throw new RoleAlreadyExists(roleName);
                var role = new TRole { Name = roleName };
                session.Store(role);
                session.SaveChanges();
            }
        }

        public string[] GetRolesForUser(string username)
        {
            using (var session = DocumentStore.OpenSession())
            {
                return session.Query<TRole>().Where(r => r.Users.Any(name => name == username))
                                    .Select(r => r.Name).ToArray();
            }
        }

        public string[] GetUsersInRole(string roleName)
        {
            using (var session = DocumentStore.OpenSession())
            {
                var role = session.Query<TRole>().SingleOrDefault(r => r.Name == roleName);
                if (role != null)
                {
                    return role.Users.ToArray();
                }
            }
            return new string[0];
        }

        public string[] GetAllRoles()
        {
            using (var session = DocumentStore.OpenSession())
            {
                return session.Query<TRole>().Select(r => r.Name).ToArray();
            }
        }

        public void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            using (var session = DocumentStore.OpenSession())
            {
                foreach (var roleName in roleNames)
                {
                    var role = session.Query<TRole>().Single(r => r.Name == roleName);
                    var users = role.Users.Where(usernames.Contains).ToArray();
                    foreach (var user in users)
                    {
                        role.Users.Remove(user);
                    }
                }
                session.SaveChanges();
            }
        }

        public void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            using (var session = DocumentStore.OpenSession())
            {
                foreach (var roleName in roleNames)
                {
                    var role = session.Query<TRole>().Single(r => r.Name == roleName);
                    foreach (var username in usernames)
                    {
                        if (!role.Users.Contains(username))
                        {
                            role.Users.Add(username);
                        }
                    }
                }
                session.SaveChanges();
            }
        }

        public bool RoleExists(string roleName)
        {
            using (var session = DocumentStore.OpenSession())
            {
                return session.Query<TRole>().Any(r => r.Name == roleName);
            }
        }

        public bool DeleteRole(string roleName)
        {
            using (var session = DocumentStore.OpenSession())
            {
                var role = session.Query<TRole>().SingleOrDefault(r => r.Name == roleName);
                if (role != null)
                {
                    session.Delete(role);
                    session.SaveChanges();
                    return true;
                }
                return false;
            }
        }
    }
}