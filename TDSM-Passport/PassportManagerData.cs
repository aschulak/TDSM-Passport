using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Passport
{    
    /**
     * Data stored by PassportManager
     */
    public class APassportManagerData
    {
        private String pluginFolderPath;
        public List<User> users;
        public Dictionary<User, Passport> passportsByUser;
        public Dictionary<Passport, User> usersByPassport;
        public Dictionary<string, Passport> passportsByPlayerName;

        public APassportManagerData(String pluginPath)
        {
            this.pluginFolderPath = pluginPath + Path.DirectorySeparatorChar + "passportdata.xml";
            users = new List<User>();
            passportsByUser = new Dictionary<User, Passport>();
            passportsByPlayerName = new Dictionary<string, Passport>();
            usersByPassport = new Dictionary<Passport, User>();
        }

        /**
         * Save user information to disk.
         */
        public void save()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<User>));
            TextWriter textWriter = new StreamWriter(@pluginFolderPath);
            serializer.Serialize(textWriter, users);
            textWriter.Close();
        }

        /**
         * Load user information from disk.
         */
        public void load()
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(List<User>));
            TextReader textReader = new StreamReader(@pluginFolderPath);
            users = (List<User>)deserializer.Deserialize(textReader);
            textReader.Close();
        }
     
    }
}