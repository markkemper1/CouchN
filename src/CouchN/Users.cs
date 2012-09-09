using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace CouchN
{
    public class Users
    {
        private readonly CouchSession session;

        public Users(CouchSession session)
        {
            this.session = session;
        }

        /// <summary>
        ///     Gets a user by username
        /// </summary>
        public T Get<T>(string username)
        {
            var db = session.DatabaseName;
            session.Use("_users");
            var result = session.Get<T>("org.couchdb.user:" + username);
            session.Use(db);
            return result;
        }

        /// <summary>
        ///    Saves a user
        /// </summary>
        public Documents.DocumentInfo Save<T>(T doc)
        {
            var jsonObject = JObject.Parse(doc.Serialize());

            if (jsonObject["roles"] == null  && jsonObject["Roles"] != null)
            {
                jsonObject["roles"] = jsonObject["Roles"];
            }

            if (jsonObject["roles"] == null)
            {
                var roles = new JArray();
                jsonObject["roles"] = roles;
            }

            if(jsonObject["name"] == null && jsonObject["Name"] != null)
            {
                jsonObject["name"] = jsonObject["Name"].ToString();
                jsonObject.Remove("Name");
            }

            if (jsonObject["name"] == null)
            {
                throw new ArgumentException("You must have a property called Name on your user object");
            }

            jsonObject["type"] = "user";

            var db = session.DatabaseName;
            session.Use("_users");

            var info = session.Documents.GetInfo(doc);

            var result = session.Save(jsonObject, 
                "org.couchdb.user:" + jsonObject["name"], 
                info != null ? info.Revision : null);

            session.Use(db);
            return result;
        }

        public void SetPasword(string username, string password)
        {
            var user = this.Get<JObject>(username);

            if(user == null)
                throw new ArgumentException("The user with username: " + username + " could not be found");

            user["password"] = password;

            Save(user);
        }

        /// <summary>
        ///     Deletes a user by username
        /// </summary>
        public void Delete(string username)
        {
            var db = session.DatabaseName;
            session.Use("_users");
            session.Delete(username);
            session.Use(db);
        }


        public AuthenticationResponse<T> Authenticate<T>(string name, string password)
        {
            var dict = new Dictionary<string, object>()
                           {
                               {"name", name},
                               {"password", password}
                           };

            var request = session.PostRequest("/_session", dict);

            var response = session.Client.Execute(request);

            if(response.StatusCode != HttpStatusCode.OK)
                return new AuthenticationResponse<T>{ OK = false};

            return new AuthenticationResponse<T>()
                       {
                           OK = true,
                           Token = response.Cookies[0].Value,
                           User = response.Content.DeserializeObject<T>()
                       };
        }
    }

    public class AuthenticationResponse<T>
    {
        public bool OK { get; set; }

        public string Token { get; set; }

        public T User { get; set; }
    }

}
