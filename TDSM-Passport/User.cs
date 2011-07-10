using System;

namespace Envoy.TDSM_Passport
{
    public class User
    {
        public string username = "";
        public string password = "";
        public string lastPlayerName = "";
     
        public User()
        {
        }
     
        public override bool Equals(Object other)
        {
            User otherUser = (User)other;
            return (username.Equals(otherUser.username) && password.Equals(otherUser.password)); 
        }
     
        public override int GetHashCode()
        {
            return username.GetHashCode() ^ password.GetHashCode();  
        }
     
    }
}