using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using Envoy.TDSM_Vault;

namespace Envoy.TDSM_Passport
{
    // Data stored by PassportManager
    public class PassportManagerData
    {
        public Dictionary<User, Passport> passportsByUser;
        public Dictionary<Passport, User> usersByPassport;
        public Dictionary<string, Passport> passportsByPlayerName;
        private UserList userList;
        private Vault vault;

        public PassportManagerData(String pluginPath)
        {
            vault = VaultFactory.getVault();
            userList = new UserList();
            passportsByUser = new Dictionary<User, Passport>();
            passportsByPlayerName = new Dictionary<string, Passport>();
            usersByPassport = new Dictionary<Passport, User>();
        }

        public void addUser(User user)
        {
            userList.getUsers().Add(user);
            // save for good measure, though this probably makes the save timer moot
            save();
        }

        public List<User> getUsers()
        {
            return userList.getUsers();
        }

        public void save()
        {
            vault.store(userList);
        }

        public void load()
        {
            vault.getVaultObject(userList);
        }
     
    }
    
}