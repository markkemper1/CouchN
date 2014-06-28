using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CouchN
{
    public class DesginDocuments
    {
        private readonly CouchSession session;
        private readonly string name;
        private readonly string basePath;
        private Documents Documents { get; set; }

        public DesginDocuments(CouchSession session, string name)
        {
            this.session = session;
            this.Documents = session.Documents;
            this.name = name;
            this.basePath = "_design/" + name;
        }

        public T Get<T>()
        {
            return Documents.Get<T>(basePath);
        }

        public void Put<T>(T doc)
        {
            this.Documents.Save(doc, basePath);
        }

        public void SetDocument(string content)
        {
            var existing = session.Documents.Get<JObject>(basePath);
            if (existing != null)
            {
                var info = session.Documents.GetInfo(existing);
                session.Documents.Save(JObject.Parse(content), basePath, info.Revision);
            }
            else
                session.Documents.Save(JObject.Parse(content), basePath, null);
        }

        public ViewResult<VALUE, object> View<VALUE>(string viewName, ViewQuery query = null, bool track = false)
        {
            return View<VALUE, object>(viewName, query);
        }

        public ViewResult<VALUE, DOC> View<VALUE, DOC>(string viewName, ViewQuery query = null, bool track = false) where DOC : new()
        {
            query = query ?? new ViewQuery();

            if (track && !query.IncludeDocs.HasValue)
                query.IncludeDocs = true;

            string path = basePath + "/_view/" + viewName;
            var config = Documents.GetConfiguration<DOC>();
            using (var client = new WebClient())
            {
                using (var stream = client.OpenRead(session.GetUri(path, query.ToDictionary())))
                {
                    using (var textReader = new StreamReader(stream, Encoding.UTF8))
                    {
                        using (var jsonReader = new JsonTextReader(textReader))
                        {
                            var serializer = new JsonSerializer();
                            serializer.NullValueHandling = NullValueHandling.Ignore;

                            if (typeof (DOC) != typeof (Object))
                                serializer.Converters.Add(new JsonCreationConverter<DOC>(config, track, Documents));

                            return serializer.Deserialize<ViewDocsResultRaw<VALUE, DOC>>(jsonReader).ToViewResult();
                        }
                    }
                }
            }
        }

        private class JsonCreationConverter<T> : JsonConverter where T : new()
        {
            private readonly DocumentConfig<T> config;
            private readonly bool track;
            private readonly Documents documents;


            public JsonCreationConverter(DocumentConfig<T> config, bool track, Documents documents)
            {
                this.config = config;
                this.track = track;
                this.documents = documents;
            }

            /// <summary>
            /// Create an instance of objectType, based properties in the JSON object
            /// </summary>
            /// <param name="objectType">type of object expected</param>
            /// <param name="jObject">
            /// contents of JSON object that will be deserialized
            /// </param>
            /// <returns></returns>
            protected T Create(Type objectType, JObject jObject)
            {
                var foo = new T();

                var id = jObject["_id"].Value<string>();
                var rev = jObject["_rev"].Value<string>();

                if (config != null && config.SetInfo != null)
                {
                    config.SetInfo(new Documents.DocumentInfo(id,rev), foo);
                }

                if (track) this.documents.Attach(foo, id, rev);


                return foo;
            }

            public override bool CanConvert(Type objectType)
            {
                return typeof(T).IsAssignableFrom(objectType);
            }

            public override object ReadJson(JsonReader reader,
                                            Type objectType,
                                             object existingValue,
                                             JsonSerializer serializer)
            {
                // Load JObject from stream
                JObject jObject = JObject.Load(reader);

                // Create target object based on JObject
                T target = Create(objectType, jObject);

                // Populate the object properties
                serializer.Populate(jObject.CreateReader(), target);

                return target;
            }

            public override void WriteJson(JsonWriter writer,
                                           object value,
                                           JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }


        public ViewResult<object, DOC> ViewDocs<DOC>(string viewName, ViewQuery query = null, bool track = false) where DOC : new()
        {

            query = query ?? new ViewQuery();
            query.IncludeDocs = true;
            return View<object, DOC>(viewName, query, track);

        }

        public RESPONSE Update<DOC, RESPONSE>(string handler, DOC document, string key = null)
        {
            var info = Documents.GetInfo(document);

            object payload = Documents.AddInfoToObject(document, info);

            //If we aren't tracking and no key is supplied then create a key
            key = key ?? (info == null ? this.session.GetUuid() : info.Id);

            var path = basePath + "/_update/" + handler + "/" + key;

            var request = session.PutRequest(path);

            request.AddJson(payload);

            var response = session.Client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Created)
            {
                var revHeader = response.Headers.FirstOrDefault(x => x.Name == "X-Couch-Update-NewRev");
                if (revHeader != null)
                    Documents.Attach(document, key, revHeader.Value.ToString());

                return response.Content.DeserializeObject<RESPONSE>();
            }

            throw new ApplicationException("Failed: " + response.StatusCode + " - " + response.Content);
        }


        public Tuple<RESPONSE, string> Update<RESPONSE>(string handler, string documentId = null, object payload = null)
        {
            bool hasDocId = !String.IsNullOrWhiteSpace(documentId);

            var path = basePath + "/_update/" + handler + (hasDocId ? "/" + documentId : null);

            var request = hasDocId ? session.PutRequest(path) : session.PostRequest(path);

            if (payload != null)
                request.AddJson(payload.Serialize());

            var response = session.Client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Created)
            {
                string revision = null;
                var revHeader = response.Headers.FirstOrDefault(x => x.Name == "X-Couch-Update-NewRev");

                if (revHeader != null)
                    revision = revHeader.Value.ToString();

                return Tuple.Create(response.Content.DeserializeObject<RESPONSE>(), revision);
            }

            throw new ApplicationException("Failed: " + response.StatusCode + " - " + response.Content);
        }


        [DataContract]
        public class ViewResult<VALUE, DOC>
        {
            [DataMember(Name = "total_rows")]
            public int Total { get; set; }

            [DataMember(Name = "offset")]
            public int Offset { get; set; }

            [DataMember(Name = "rows")]
            public RowInfo[] Rows { get; set; }

            public VALUE[] Values { get; set; }

            public DOC[] Documents { get; set; }

            [DataContract]
            public class RowInfo
            {
                [DataMember(Name = "id")]
                public string Id { get; set; }

                [DataMember(Name = "key")]
                public object Key { get; set; }
            }
        }

        [DataContract]
        public class ViewDocsResultRaw<VALUE, DOC>
        {
            [DataMember(Name = "total_rows")]
            public int Total { get; set; }

            [DataMember(Name = "offset")]
            public int Offset { get; set; }

            [DataMember(Name = "rows")]
            public RowInfo<VALUE, DOC>[] Rows { get; set; }

            [DataContract]
            public class RowInfo<VALUE, DOC>
            {
                [DataMember(Name = "id")]
                public string Id { get; set; }

                [DataMember(Name = "key")]
                public object Key { get; set; }

                [DataMember(Name = "doc")]
                public DOC Document { get; set; }

                [DataMember(Name = "value")]
                public VALUE Value { get; set; }
            }

            public ViewResult<VALUE, DOC> ToViewResult()
            {
                return new ViewResult<VALUE, DOC>()
                {
                    Total = this.Total,
                    Offset = this.Offset,
                    Rows = this.Rows.Select(x => new ViewResult<VALUE, DOC>.RowInfo { Id = x.Id, Key = x.Key }).ToArray(),
                    Documents = this.Rows.Select(x => x.Document).ToArray(),
                    Values = this.Rows.Select(x => x.Value).ToArray()
                };
            }
        }

    }
}