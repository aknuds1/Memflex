using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using FlexProviders.Membership;
using FlexProviders.Roles;
using Microsoft.Web.WebPages.OAuth;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;

namespace FlexProviders.Mongo
{
    public class FlexMembershipUserStore<TUser, TRole> : IFlexUserStore, IFlexRoleStore
        where TUser : class, IFlexMembershipUser<ObjectId>, new()
        where TRole : class, IFlexRole<ObjectId>, new()
    {
        protected readonly MongoCollection<TRole> RoleCollection;
        protected readonly MongoCollection<TUser> UserCollection;

        public FlexMembershipUserStore(MongoCollection<TUser> userCollection, MongoCollection<TRole> roleCollection)
        {
            UserCollection = userCollection;
            RoleCollection = roleCollection;
        }

        public void CreateRole(string roleName)
        {
            var role = new TRole { Name = roleName };
            RoleCollection.Save(role);
        }

        public string[] GetRolesForUser(string username)
        {
            var user = UserCollection.AsQueryable().SingleOrDefault(u => u.Username == username);
            if (user == null)
                return new string[] {};
            var names = RoleCollection.Find(new QueryDocument("Users", user.Id))
                .Select(r => r.Name).ToArray();
            return names;
        }

        public string[] GetUsersInRole(string roleName)
        {
            var role = RoleCollection.AsQueryable().SingleOrDefault(r => r.Name == roleName);
            if (role != null)
            {
                return UserCollection.AsQueryable().Where(u => role.Users.Contains(u.Id)).Select(u => u.Username).ToArray();
            }

            return new string[] { };
        }

        public string[] GetAllRoles()
        {
            return RoleCollection.AsQueryable().Select(r => r.Name).ToArray();
        }

        public void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            var users = UserCollection.AsQueryable().Where(u => usernames.Contains(u.Username));
            foreach (string roleName in roleNames)
            {
                var role = RoleCollection.AsQueryable().Single(r => r.Name == roleName);
                foreach (var uid in role.Users.Where(uid => users.Any(u => u.Id == uid)))
                    role.Users.Remove(uid);
                RoleCollection.Save(role);
            }
        }

        public void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            var uids = UserCollection.AsQueryable().Where(u => usernames.Contains(u.Username)).Select(
                u => u.Id);
            foreach (string roleName in roleNames)
            {
                TRole role = RoleCollection.AsQueryable().Single(r => r.Name == roleName);
                foreach (var uid in uids)
                {
                    if (!role.Users.Contains(uid))
                    {
                        role.Users.Add(uid);
                    }
                }
                RoleCollection.Save(role);
            }
        }

        public bool RoleExists(string roleName)
        {
            return RoleCollection.AsQueryable().Any(r => r.Name == roleName);
        }

        public bool DeleteRole(string roleName)
        {
            TRole role = RoleCollection.AsQueryable().SingleOrDefault(r => r.Name == roleName);
            if (role != null)
            {
                RoleCollection.Remove(Query<TRole>.EQ(r => r.Name, roleName));
                return true;
            }
            return false;
        }

        public IFlexMembershipUser CreateOAuthAccount(string provider, string providerUserId, IFlexMembershipUser user)
        {
            var account = new FlexOAuthAccount { Provider = provider, ProviderUserId = providerUserId };
            if (user.OAuthAccounts == null)
            {
                user.OAuthAccounts = new Collection<FlexOAuthAccount>();
            }
            user.OAuthAccounts.Add(account);
            UserCollection.Save(user);

            return user;
        }

        public IFlexMembershipUser GetUserByUsername(string username)
        {
            return UserCollection.AsQueryable().SingleOrDefault(u => u.Username == username);
        }

        public IFlexMembershipUser Add(IFlexMembershipUser user)
        {
            UserCollection.Save(user);
            return user;
        }

        public IFlexMembershipUser Save(IFlexMembershipUser user)
        {
            TUser existingUser = UserCollection.AsQueryable().SingleOrDefault(u => u.Username == user.Username);
            foreach (PropertyInfo property in user.GetType().GetProperties().Where(p => p.CanWrite))
                property.SetValue(existingUser, property.GetValue(user));

            UserCollection.Save(existingUser);
            return user;
        }

        public bool DeleteOAuthAccount(string provider, string providerUserId)
        {
            TUser user =
                UserCollection.AsQueryable().SingleOrDefault(
                    u => u.OAuthAccounts.Any(o => o.ProviderUserId == providerUserId && o.Provider == provider));

            if (user != null)
            {
                if (user.OAuthAccounts.Count() > 1 || !string.IsNullOrEmpty(user.Password))
                {
                    FlexOAuthAccount account =
                        user.OAuthAccounts.Single(o => o.Provider == provider && o.ProviderUserId == providerUserId);
                    user.OAuthAccounts.Remove(account);
                    UserCollection.Save(user);
                    return true;
                }
            }
            return false;
        }

        public IFlexMembershipUser GetUserByPasswordResetToken(string passwordResetToken)
        {
            return UserCollection.AsQueryable().SingleOrDefault(u => u.PasswordResetToken == passwordResetToken);
        }

        public IFlexMembershipUser GetUserByOAuthProvider(string provider, string providerUserId)
        {
            return
                UserCollection.AsQueryable().SingleOrDefault(
                    u => u.OAuthAccounts.Any(r => r.Provider == provider && r.ProviderUserId == providerUserId));
        }

        public IEnumerable<OAuthAccount> GetOAuthAccountsForUser(string username)
        {
            return UserCollection.AsQueryable()
                .Single(u => u.Username == username)
                .OAuthAccounts.ToArray()
                .Select(o => new OAuthAccount(o.Provider, o.ProviderUserId));
        }

        public IFlexMembershipUser CreateOAuthAccount(string provider, string providerUserId, string username)
        {
            TUser user = UserCollection.AsQueryable().SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                user = new TUser { Username = username };
                UserCollection.Save(user);
            }
            var account = new FlexOAuthAccount { Provider = provider, ProviderUserId = providerUserId };
            if (user.OAuthAccounts == null)
            {
                user.OAuthAccounts = new Collection<FlexOAuthAccount>();
            }
            user.OAuthAccounts.Add(account);
            UserCollection.Save(user);

            return user;
        }
    }
}