using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using Envoy.TDSM_Vault;

namespace Envoy.TDSM_Passport
{
    public class UserList : VaultObject
    {
        private List<User> users;
        private XmlSerializer serializer;

        public UserList() : base("Passport")
        {
            users = new List<User>();
            serializer = new XmlSerializer(typeof(List<User>));
        }

        public List<User> getUsers()
        {
            return users;
        }

        //
        // FROM VaultObject
        //
        
        public override void fromXml(String xml)
        {
            StringReader stringReader = new StringReader(xml);
            users = (List<User>)serializer.Deserialize(stringReader);
            stringReader.Close();
        }

        public override string toXml()
        {
            StringWriter stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, users);
            String xml = stringWriter.ToString();
            stringWriter.Close();
            return xml;
        }

    }

}