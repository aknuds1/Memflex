using System;
using System.Collections.Generic;
using FlexProviders.Membership;
using MongoDB.Bson;

namespace LogMeIn.Models
{
    public class User : IFlexMembershipUser<ObjectId>
    {
        public ObjectId Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public string PasswordResetToken { get; set; }
        public DateTime PasswordResetTokenExpiration { get; set; }
        public int FavoriteNumber { get; set; }
        public virtual ICollection<FlexOAuthAccount> OAuthAccounts { get; set; }
    }
}