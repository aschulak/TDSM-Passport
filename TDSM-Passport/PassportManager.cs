using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Terraria_Server;

namespace Envoy.TDSM_Passport
{
    /**
    * Manages singleton reference of PassportManager for all plugins.
    */
    public static class PassportManagerFactory
    {
        private static PassportManager passportManager;
     
        /**
         * Gets the global singleton instance of the PassportManager.
         */
        public static PassportManager getPassportManager()
        {
            if (passportManager == null) {    
                passportManager = new PassportManager();   
            }
         
            return passportManager;
        }
    }
 
    public class PassportManager
    {        
        private PassportManagerData passportManagerData;
     
        public PassportManager()
        {                
            setupPassportManagerData();
        }
     
        //
        // API
        //

        /**
         * Creates the user in the account system if the user does not already exist.
         * Also effectively logs the user in.
         */
        public Passport createUser(Player player, string username, string password)
        {
            // check for existing user
            if (userExists(username)) {
                throw new UserExistsException();
            }

            User user = new User();
            user.username = username;
            user.password = password;
            user.lastPlayerName = player.Name;
            user.lastLoginDate = System.DateTime.Now.ToString();

            passportManagerData.addUser(user);

            Passport passport = createPassport(user);
            passportManagerData.passportsByUser[user] = passport;
            passportManagerData.passportsByPlayerName[user.lastPlayerName] = passport;
            passportManagerData.usersByPassport[passport] = user;

            Log("[" + player.Name + "] created user <" + username + ">");
            return passport;
        }
 
        /**
         * Logs the user into the account system.
         */
        public Passport loginUser(Player player, string username, string password)
        {            
            User user = getUser(username);
         
            if (user == null) {
                Log("No user found: <" + username + ">");
                throw new UserNotFoundException();
            }
                 
            if (user.password != password) {
                Log("Password doesn't match");
                throw new AuthenticationException();
            }                        
                     
            string lastPlayerName = player.Name;
            if (!user.lastPlayerName.Equals(lastPlayerName)) {
                Log("<" + username + "> was [" + user.lastPlayerName + "] and is now [" + lastPlayerName + "]");
                user.lastPlayerName = lastPlayerName;
            }
         
            Passport passport = null;
            passportManagerData.passportsByUser.TryGetValue(user, out passport);
            if (passport != null) {
                Log("Got passport:" + passport);
                Log("WARN: <" + username + ">[" + player.Name + "] attempting to login to account already logged in.");
                throw new UserAlreadyLoggedInException();
            } else {
                passport = createPassport(user);
                passportManagerData.passportsByUser[user] = passport;
                passportManagerData.passportsByPlayerName[user.lastPlayerName] = passport;
                passportManagerData.usersByPassport[passport] = user;
                Log("Created new passport:" + passport);
            }
         
            return passport;
        }

        /**
         * Logs the player out of the account system.
         */
        public void logout(Player player)
        {
            Passport passport = getPassport(player);
            logout(passport);
        }

        /**
         * Logs the player associated with the passport out of the account system.
         */
        public void logout(Passport passport)
        {
            // user may have already logged out
            if (passport != null) {
                User user = getUser(passport);          
                if (user != null) {
                    logoutUser(user, passport);     
                } else {
                    Log("User not found for logout");
                    throw new UserNotLoggedInException();
                }            
            } else {
                Log("No passport for user");
                throw new UserNotLoggedInException();
            }
        }
     
        /**
      * Returns an active Passport if one exists.
      */
        public Passport getPassport(Player player)
        {
            Passport passport = null;
            passportManagerData.passportsByPlayerName.TryGetValue(player.Name, out passport);
            return passport;
        }
     
        /**
         * Returns an active Passport if one exists.
         */
        public Passport getPassport(User user)
        {            
            Passport passport = null;
            passportManagerData.passportsByUser.TryGetValue(user, out passport);
            return passport;
        }

        //
        // INTERNAL and PRIVATE
        //
 
        internal void save()
        {
            Log("Saving data...");
            passportManagerData.save();
        }

        private void setupPassportManagerData()
        {            
            passportManagerData = new PassportManagerData(PassportPlugin.PLUGIN_FOLDER);
            try {
                passportManagerData.load();
            } catch (Exception e) {
                // this should only error on initial startup since there is no file
            }
        }
             
        private void logoutUser(User user, Passport passport)
        {
            Log("Logging out user <" + user.username + ">[" + user.lastPlayerName + "]");
            passportManagerData.usersByPassport[passport] = null;
            passportManagerData.passportsByUser[user] = null;
            passportManagerData.passportsByPlayerName[user.lastPlayerName] = null;
        }
     
        // using a list to make XmlSerialization easier
        private User getUser(string username)
        {            
            foreach (User user in passportManagerData.getUsers()) {
                if (user.username.Equals(username)) {    
                    return user;
                }
            }        
            return null;     
        }

        private User getUser(Passport passport)
        {        
            User user = null;
            passportManagerData.usersByPassport.TryGetValue(passport, out user);
            return user;
        }
     
        private bool userExists(String username)
        {
            return getUser(username) != null;
        }
     
        private Passport createPassport(User user)
        {
            return Passport.createPassport(user);
        }
     
        private void Log(string message)
        {
            Program.tConsole.WriteLine("[PassportManager] " + message);   
        }
             
    }

    //
    // EXCEPTIONS
    //

    public class UserExistsException : Exception
    {
    }

    public class UserNotFoundException : Exception
    {
    }

    public class AuthenticationException : Exception
    {
    }

    public class PlayerAlreadyLoggedInException : Exception
    {
    }

    public class UserAlreadyLoggedInException : Exception
    {
    }

    public class UserNotLoggedInException : Exception
    {
    }
}