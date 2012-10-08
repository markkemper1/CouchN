using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Web;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace CouchN
{
    public class Documents
    {
        private readonly CouchSession session;
        private readonly Dictionary<object, DocumentInfo> tracking = new Dictionary<object, DocumentInfo>();

        private static Dictionary<Type, object> configuration = new Dictionary<Type, object>(); 

        public Documents(CouchSession session)
        {
            this.session = session;
        }

        public static void SetConfiguration<T>(DocumentConfig<T> config)
        {
            var type = typeof (T);
            configuration[type] = config;
        }

        public static DocumentConfig<T> GetConfiguration<T>()
        {
            var type = typeof(T);
            return (DocumentConfig<T>) (configuration.ContainsKey(type) ? configuration[type] : null);
        }

        public static DocumentConfig<T> Configure<T>(
            Action<DocumentInfo, T> setInfo = null, 
            Func<T, string> createId=null,
            Func<string,string> resolveId=null ,
            string name = null)
        {
            var config = new DocumentConfig<T>
                             {
                                 TypeName = name, 
                                 SetInfo = setInfo, 
                                 ResolveId =  resolveId,
                                 CreateId = createId,
                             };
           
            SetConfiguration(config);

            return config;
        }

        public T Get<T>(string docId)
        {
            if (docId == null) throw new ArgumentNullException("docId");

            var config = GetOrCreateConfig<T>();

            if (config.ResolveId != null)
                docId = config.ResolveId(docId);

            var resultContent = session.Get(docId);

            if (resultContent == null)
                return default(T);

            var json = resultContent.DeserializeObject<JObject>();

            var result = json.ToObject<T>();

            var document = resultContent.DeserializeObject<GetResponse>();

            var info =  new DocumentInfo(document.Id, document.Rev);            
            tracking[(object) result] = info;
           
            
            if(config.SetInfo != null)
                config.SetInfo(info, result);

            return result;
        }

    
        public DocumentInfo Save<T>(T document, DocumentInfo info)
        {
            return Put(document, info.Id, info.Revision);
        }

        public DocumentInfo Save<T>(T document, string id, string revision = null)
        {
            if (id == null) throw new ArgumentNullException("id");

            if (revision == null && tracking.ContainsKey(document))
            {
                var info = tracking[document];
                //Set revision if we are trying to update an existing tracked doc
                revision = info.Id == id ? info.Revision : null;
            }

            return Put(document, id, revision);
        }

        public DocumentInfo Save<T>(T document)
        {
            if (!tracking.ContainsKey(document))
            {
                return Create(document);
            }

            var tag = tracking[document];

            return Put(document, tag.Id, tag.Revision);
        }

        public void Delete<T>(T document)
        {
            if (!tracking.ContainsKey(document))
            {
                throw new ApplicationException("The document is not tracked, this method can only be used for tracked documents.");
            }
            var tag = tracking[document];

            var config = GetOrCreateConfig<T>();

            if (config.UniqueConstraint != null)
            {
                var key = config.UniqueConstraint(document);
                var uniqueKey = "unique__" + config.TypeName + "__" + key;
                var existing = session.Get<UniqueConstraint>(uniqueKey);

                if (existing != null)
                {
                    session.Delete(existing._id, existing._rev);
                }
            }

            Delete(tag.Id, tag.Revision);
        }

        public void Delete(string id, string revision)
        {
            if (id == null) throw new ArgumentNullException("id");
            var query = new Dictionary<string, object>();
            query["rev"] = revision;
            session.Delete(id, query);
        }

        public void Attach(object document, string id, string rev)
        {
            if (id == null) throw new ArgumentNullException("id");
            this.tracking[document] = new DocumentInfo(id, rev);
        }

        public DocumentInfo GetInfo(object document)
        {
            if (document == null) throw new ArgumentNullException("document");
            if (tracking.ContainsKey(document))
                return tracking[document];
            return null;
        }

        public object AddInfoToObject<T>(T document)
        {
            return AddInfoToObject<T>(document, GetInfo(document));
        }

        public object AddInfoToObject<T>(T document, DocumentInfo info)
        {
            var jsonObject = JObject.Parse(document.Serialize());

            var config = GetOrCreateConfig<T>();

            if(jsonObject["type"] == null)
                jsonObject["type"] = config.TypeName;

            if (info == null)
                return jsonObject;

            jsonObject["_id"] = info.Id;
    
            if(info.Revision != null)
                jsonObject["_rev"] = info.Revision;

            return jsonObject;
        }

        private static DocumentConfig<T> GetOrCreateConfig<T>()
        {
            var type = typeof(T);
            if (configuration.ContainsKey(type))
                return (DocumentConfig<T>)configuration[type];

            lock (configuration)
            {
                if (!configuration.ContainsKey(type))
                    configuration[type] = DocumentConfig<T>.Default();
            }
            return (DocumentConfig<T>)configuration[type];
        }

        private DocumentInfo Create<T>(T document)
        {
            var config = GetOrCreateConfig<T>();
            var id = config.CreateId != null ? config.CreateId(document) : session.GetUuid();
            return Create<T>(document, id);
        }

        private DocumentInfo Create<T>(T document, string docId)
        {
            if (docId == null) throw new ArgumentNullException("docId");
            return Put<T>(document, docId, null);
        }
        /* Constraints 
     * 
     *  When updating, if current holder of constraint is this doc then OK,
     *      if not current holder, create one,
     *      if not holder , then throw exception
     *  
     *  When creating new doc, see above
     */ 

        private DocumentInfo Put<T>(T document, string id, string revision)
        {
            if (id == null) throw new ArgumentNullException("id");

            var config = GetOrCreateConfig<T>();

            if(config.UniqueConstraint != null)
            {
                var key = config.UniqueConstraint(document);
                var uniqueKey = "unique__" + config.TypeName + "__" + key;
                var existing = session.Get<UniqueConstraint>(uniqueKey);

                if(existing != null )
                {
                    var existingDoc = session.Get<JsonObject>(existing.HolderId);

                    if (existingDoc != null && existing.HolderId != id)
                        throw new ArgumentException("Unique constraint violated. Document: " + existing.HolderId + " is aready holder the key: " + uniqueKey);

                    existing.HolderId = id;
                    Save(existing);
                }

                if(existing == null)
                {
                    existing = new UniqueConstraint() { _id = uniqueKey, HolderId = id};
                    var constraintInfo = Put<UniqueConstraint>(existing, uniqueKey, null);
                    existing._id = constraintInfo.Id;
                }
            }


            //we have a new doc
            if (revision == null && config.CreateId != null)
            {
                id = config.CreateId(document);
            }

            var request = session.PutRequest(id);

            object payload = document;

            payload = this.AddInfoToObject(document, new DocumentInfo(id, revision));

            request.AddJson(payload);

            var response = session.Client.Execute(request);

            if (response.StatusCode != HttpStatusCode.Created && response.StatusCode != HttpStatusCode.Accepted)
                throw new ApplicationException("Failed: " + response.StatusCode + " - " + response.Content);

            var result = response.Content.DeserializeObject<DocumentResponse>();

            var info = new DocumentInfo(result.Id, result.Rev);


            tracking[(object)document] = info;



            if (config.SetInfo != null)
                config.SetInfo(info, document);

            return info;
        }

        public class DocumentInfo
        {
            public string Id;
            public string Revision;

            public DocumentInfo(string id, string revision)
            {
                Id = id;
                Revision = revision;
            }
        }


        [DataContract]
        public class GetResponse
        {
            [DataMember(Name = "_id")]
            public string Id { get; set; }
            [DataMember(Name = "_rev")]
            public string Rev { get; set; }
        }

        [DataContract]
        public class DocumentResponse
        {
            [DataMember(Name = "ok")]
            public bool OK { get; set; }

            [DataMember(Name = "id")]
            public string Id { get; set; }
            [DataMember(Name = "rev")]
            public string Rev { get; set; }
        }

    }

    public class UniqueConstraint
    {
        public string _id { get; set; }
        public string _rev { get; set; }
        public string HolderId { get; set; }
    }

    public class DocumentConfig<T>
    {
        private string typeName;

        public string TypeName
        {
            get { return typeName ?? typeof(T).Name; }
            set { typeName = value; }
        }

        public Action<Documents.DocumentInfo, T> SetInfo;

        public Func<T, string> CreateId;

        public Func<string, string> ResolveId;

        /// <summary>
        ///     Why isn't this a list? because this is a slow / crapping method of implementing unique constraints in couch.
        ///     If you need more then either maintain them your self OR use a RDBMS.
        /// </summary>
        public Func<T, string> UniqueConstraint { get; set; }

        public static DocumentConfig<T> Default()
        {
            return new DocumentConfig<T>();
        }
    }
}
