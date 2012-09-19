using System;
using System.Collections.Generic;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;

namespace FlexProviders
{
    public class FlexMemebershipProvider : IFlexMembershipProvider, 
                                           IFlexOAuthProvider,
                                           IOpenAuthDataProvider 
    {
        private readonly IFlexUserStore _userStore;
        private readonly IFlexOAuthDataStore _oAuthDataStore;
        private readonly IApplicationEnvironment _applicationEnvironment;
        private readonly ISecurityEncoder _encoder = new DefaultSecurityEncoder();

        public FlexMemebershipProvider(
            IFlexUserStore userStore, 
            IFlexOAuthDataStore oAuthDataStore,
            IApplicationEnvironment applicationEnvironment)            
        {         
            _userStore = userStore;
            _oAuthDataStore = oAuthDataStore;
            _applicationEnvironment = applicationEnvironment;
        }

        public bool Login(string username, string password)
        {
            var user = _userStore.GetUserByUsername(username);
            var encodedPassword = _encoder.Encode(password, user.Salt);
            var flag = encodedPassword.Equals(user.Password);
            if (flag)
            {
                _applicationEnvironment.IssueAuthTicket(username, true);
                return true;
            }
            return false;
        }

        public void Logout()
        {
            _applicationEnvironment.RevokeAuthTicket();
        }

        public void CreateAccount(IFlexMembershipUser user)
        {
            var existingUser = _userStore.GetUserByUsername(user.Username);
            if (existingUser != null)
            {
                throw new MembershipCreateUserException("Cannot register with a duplicate username");
            }

            user.Salt = user.Salt ?? _encoder.GenerateSalt();
            user.Password = _encoder.Encode(user.Password, user.Salt);
            user.IsLocal = true;
            _userStore.Add(user);
        }

        public bool HasLocalAccount(string userName)
        {
            var user = _userStore.GetUserByUsername(userName);
            return user.IsLocal;
        }

        public bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            var user = _userStore.GetUserByUsername(username);
            var encodedPassword = _encoder.Encode(oldPassword, user.Salt);
            var flag = encodedPassword.Equals(user.Password);
            if (flag)
            {
                user.Password = _encoder.Encode(newPassword, user.Salt);
                _userStore.Save(user);
            }
            return false;
        }

        public void CreateOAuthAccount(string provider, string providerUserId, string username)
        {
            _oAuthDataStore.CreateOAuthAccount(provider, providerUserId, username);
        }

        public string GetUserNameFromOpenAuth(string provider, string providerUserId)
        {
            var user = _oAuthDataStore.GetUserByOAuthProvider(provider, providerUserId);
            return user.Username;
        }

        public bool DissassociateOAuthAccount(string provider, string providerUserId)
        {
            return _oAuthDataStore.DeleteOAuthAccount(provider, providerUserId);
        }

        public AuthenticationClientData GetOAuthClientData(string providerName)
        {
            return _authenticationClients[providerName];
        }

        public void RegisterClient(IAuthenticationClient client,
            string displayName, IDictionary<string, object> extraData)
        {
            var clientData = new AuthenticationClientData(client, displayName, extraData);
            _authenticationClients.Add(client.ProviderName, clientData);
        }

        public ICollection<AuthenticationClientData> RegisteredClientData
        {
            get { return _authenticationClients.Values; }
        }

        public IEnumerable<OAuthAccount> GetOAuthAccountsFromUserName(string username)
        {
            return _oAuthDataStore.GetOAuthAccountsForUser(username);
        }

        public void RequestOAuthAuthentication(string provider, string returnUrl)
        {
            var client = _authenticationClients[provider];
            _applicationEnvironment.RequestAuthentication(client.AuthenticationClient, this, returnUrl);
        }

        public AuthenticationResult VerifyOAuthAuthentication(string returnUrl)
        {
            var providerName = _applicationEnvironment.GetOAuthPoviderName();
            if (String.IsNullOrEmpty(providerName))
            {
                return AuthenticationResult.Failed;
            }

            var client = _authenticationClients[providerName];
            return _applicationEnvironment.VerifyAuthentication(client.AuthenticationClient, this, returnUrl);
        }

        public bool OAuthLogin(string provider, string providerUserId, bool persistCookie)
        {
            var oauthProvider = _authenticationClients[provider];
            var context = _applicationEnvironment.AcquireContext();
            var securityManager = new OpenAuthSecurityManager(context, oauthProvider.AuthenticationClient, this);
            return securityManager.Login(providerUserId, persistCookie);
        }

        private static readonly Dictionary<string, AuthenticationClientData> _authenticationClients =
            new Dictionary<string, AuthenticationClientData>(StringComparer.OrdinalIgnoreCase);        
    }
}