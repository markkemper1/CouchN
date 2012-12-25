using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using RestSharp;
using RestSharp.Contrib;

namespace CouchN
{
    public class CouchSession : IDisposable
    {
        private readonly Uri baseUri;
        private string db;
        private Dictionary<string, DesginDocuments> designDocuments = new Dictionary<string, DesginDocuments>();
        private Documents documents;
        private BulkDocuments bulkDocuments;
        private readonly RestClient client;
        private readonly Users users;

        public CouchSession(Uri baseUri, string db)
        {
            //JsConfig.ExcludeTypeInfo = true;
            //JsConfig.EmitCamelCaseNames = true;

            if (baseUri == null) throw new ArgumentNullException("baseUri");
            if (db == null) throw new ArgumentNullException("db");
            this.baseUri = baseUri;
            this.db = db;
            client = new RestClient(baseUri.ToString());

            if (!String.IsNullOrWhiteSpace(this.baseUri.UserInfo))
            {
                var username = this.baseUri.UserInfo.Substring(0, this.baseUri.UserInfo.IndexOf(':'));
                var password = this.baseUri.UserInfo.Substring(this.baseUri.UserInfo.IndexOf(':') + 1);
                Credential = new NetworkCredential(username, password);
                client.Authenticator = new HttpBasicAuthenticator(username, password);
            }

            documents = new Documents(this);
            bulkDocuments = new BulkDocuments(this);
            users = new Users(this);
        }

        public NetworkCredential Credential { get; set; }

        public Databases Db
        {
            get { return new Databases(this); }
        }

        internal RestClient Client { get { return client; } }

        public string DatabaseName { get { return this.db; } }

        public Documents Documents
        {
            get { return documents; }
        }

        public BulkDocuments Bulk { get { return bulkDocuments; } }
        /// <summary>
        /// Helper methods for the users database
        /// </summary>
        public Users Users { get { return this.users; } }

        /// <summary>
        ///     Set the default datbase
        /// </summary>
        /// <param name="db"></param>
        public void Use(string db)
        {
            this.db = db;
        }

        /// <summary>
        ///     Prefixs the current database to the path (unless the path starts with a '/')
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string Url(string path)
        {
            if (path != null && path.StartsWith("/"))
                return path;

            return DatabaseName + (String.IsNullOrWhiteSpace(path) ? null : "/" + path);
        }

        public Uri GetUri(string path, Dictionary<string, object> query)
        {
            var pathAndQuery = Url(path) + ToQueryString(query);
            return new Uri(baseUri, pathAndQuery);
        }

        private string ToQueryString(IEnumerable<KeyValuePair<string, object>> query)
        {

            if (query == null) return null;


            var sb = new StringBuilder();

            
            bool first = true;
            foreach (var kv in query)
            {
                sb.Append(first ? '?' : '&');
                sb.AppendFormat("{0}={1}", HttpUtility.UrlEncode(kv.Key), HttpUtility.UrlEncode(kv.Value.ToString()));
                first = false;
            }

            return sb.ToString();
        }

        /// <summary>
        ///     Shortcut for Documents.Get
        ///     Returns the document from couch based on the id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public T Get<T>(string id)
        {
            return this.Documents.Get<T>(id);
        }

        /// <summary>
        ///     Shortcut for Documents.Save
        ///     Saves a document to the database. 
        ///     can pass an optional id (but this is not recommended)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public Documents.DocumentInfo Save<T>(T doc, string id = null, string revision = null)
        {
            if (id != null)
                return this.Documents.Save(doc, id, revision);

            return this.Documents.Save(doc);
        }

        public Documents.DocumentInfo Save<T>(T doc, Documents.DocumentInfo info)
        {
            return this.Documents.Save(doc, info);
        }

        /// <summary>
        ///     Deletes a tracked document. (document must first be loaded inside this session)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="doc"></param>
        public void Delete<T>(T doc)
        {
            this.Documents.Delete(doc);
        }

        /// <summary>
        ///     Deletes a tracked document based on the id and revision
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="doc"></param>
        public void Delete(string id, string revision)
        {
            this.Documents.Delete(id, revision);
        }


        /// <summary>
        ///     Gets a unique id from couch
        /// </summary>
        /// <returns></returns>
        public string GetUuid()
        {
            return this.GetUuids(1).First();
        }

        /// <summary>
        ///     Gets a set of unique ids from couch
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public string[] GetUuids(int count = 1)
        {
            var query = new Dictionary<string, object>();
            query["count"] = count;
            var response = Get<GetUuidsResponse>("/_uuids", query);
            return response.uuids;
        }

        /* Basics */

        internal T Get<T>(string path, Dictionary<string, object> query = null)
        {
            var content = Get(path, query);

            if (content == null)
                return default(T);

            return content.DeserializeObject<T>();
        }

        internal string Get(string path, Dictionary<string, object> query = null)
        {
            var request = Request(path, query);
            var response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Created  &&
                response.StatusCode != HttpStatusCode.Accepted)
                return response.Content;

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            throw new ApplicationException("Failed: " + response.StatusCode + " - " + response.Content);
        }

        internal int Put(string path)
        {
            var request = PutRequest(path);
            var response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK &&
              response.StatusCode != HttpStatusCode.Created &&
                response.StatusCode != HttpStatusCode.Accepted)
                throw new ApplicationException("Failed: " + response.StatusCode + " - " + response.Content);

            return (int)response.StatusCode;
        }

        internal int Delete(string path, Dictionary<string, object> query = null)
        {
            var request = DeleteRequest(path, query);
            var response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK &&
                response.StatusCode != HttpStatusCode.NotFound &&
                response.StatusCode != HttpStatusCode.Accepted)
                throw new ApplicationException("Failed: " + response.StatusCode + " - " + response.Content);

            return (int)response.StatusCode;
        }

        private RestRequest Request(string path, Dictionary<string, object> query = null)
        {
            var request = new RestRequest(Url(path));
            request.RequestFormat = DataFormat.Json;
            request.AddQuery(query);
            return request;
        }

        internal RestRequest GetRequest(string path, Dictionary<string, object> query = null)
        {
            var request = Request(path, query);
            request.Method = Method.GET;
            return request;
        }

        internal RestRequest DeleteRequest(string path, Dictionary<string, object> query = null)
        {
            var request = Request(path, query);
            request.Method = Method.DELETE;
            return request;
        }

        internal RestRequest PostRequest(string path, Dictionary<string, object> query = null)
        {
            var request = Request(path, query);
            request.Method = Method.POST;
            return request;
        }

        internal RestRequest PutRequest(string path, Dictionary<string, object> query = null)
        {
            var request = Request(path, query);
            request.Method = Method.PUT;
            return request;
        }

        public virtual void Dispose()
        {

        }

        /// <summary>
        ///     Gets a design document helper for the given design document name (like 'tasks' for example)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public DesginDocuments Design(string name)
        {
            if (!designDocuments.ContainsKey(name))
                designDocuments[name] = new DesginDocuments(this, name);
            return designDocuments[name];
        }

        private class GetUuidsResponse
        {
            public string[] uuids { get; set; }
        }
    }
}