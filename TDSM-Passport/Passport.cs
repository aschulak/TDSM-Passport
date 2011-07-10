using System;
using System.IO;

namespace Envoy.TDSM_Passport
{
    public class Passport
    {
        private User user;
        private string token;
     
        private Passport(User user, string token)
        {
            this.user = user;
            this.token = token;
        }
     
        public static Passport createPassport(User user)
        {
            return new Passport(user, createRandomString(64));
        }

        public User getUser()
        {
            return user;
        }

        public override string ToString()
        {
            return token;
        }
     
        public override bool Equals(Object other)
        {
            Passport otherPassport = (Passport)other;
            return token.Equals(otherPassport.token);
        }
     
        public override int GetHashCode()
        {
            return token.GetHashCode();
        }

        //
        // PRIVATE
        //

        private static string createRandomString(int passwordLength)
        {
            string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789!@$?_-";
            char[] chars = new char[passwordLength];
            Random rd = new Random();

            for (int i = 0; i < passwordLength; i++) {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }

    }
 
}