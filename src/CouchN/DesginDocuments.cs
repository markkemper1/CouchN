using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
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

        public void Get<T>(T doc)
        {
            this.Documents.Get<T>(basePath);
        }

        public void Put<T>(T doc, bool track = true)
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

        public ViewResult<object, DOC> ViewDocs<DOC>(string viewName, ViewQuery query = null, bool track = false)
        {
            query = query ?? new ViewQuery();
            query.IncludeDocs = true;
            return View<object, DOC>(viewName, query, track);
        }

        public ViewResult<VALUE, DOC> View<VALUE, DOC>(string viewName, ViewQuery query = null, bool track = false)
        {
            query = query ?? new ViewQuery();

            if (track && !query.IncludeDocs.HasValue)
                query.IncludeDocs = true;

            string path = basePath + "/_view/" + viewName;
            var responseContent = session.Get(path, query.ToDictionary());

            if (responseContent == null)
                throw new Exception("Design view not found: " + path);

            var result = JObject.Parse(responseContent);

            var final = responseContent.DeserializeObject<ViewResult<VALUE, DOC>>();

            final.Documents = new DOC[final.Rows.Length];
            final.Values = new VALUE[final.Rows.Length];

            for (var i = 0; i < final.Rows.Length; i++)
            {
                var document = result["rows"][i]["doc"];
                var value = result["rows"][i]["value"];

                final.Documents[i] = document != null ? document.ToString().DeserializeObject<DOC>() : default(DOC);

                var typedDocument = final.Documents[i];

                if(typedDocument != null)
                {
                    var config = Documents.GetConfiguration<DOC>();

                    if(config != null && config.SetInfo != null)
                        config.SetInfo(new Documents.DocumentInfo(final.Rows[i].Id, document["_rev"].ToString()), typedDocument);
                }

                final.Values[i] = value != null ?  value.ToString().DeserializeObject<VALUE>() : default(VALUE);

                if (track)
                    this.Documents.Attach(final.Documents[i], result["rows"][i]["doc"]["_id"].ToString(), result["rows"][i]["doc"]["_rev"].ToString());
            }

            return final;
        }

        public RESPONSE Update<DOC, RESPONSE>(string handler, DOC document, string key = null)
        {
            var info = Documents.GetInfo(document);

            object payload = Documents.AddInfoToObject(document, info);

            //If we aren't tracking and no key is supplied then create a key
            key = (key == null && info == null) ? this.session.GetUuid() : info.Id;

            var path = basePath + "/_update/" + handler + "/" + key;

            var request = session.PutRequest(path);

            request.AddJson(payload);

            var response = session.Client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Created)
            {
                var rev = response.Headers.First(x => x.Name == "X-Couch-Update-NewRev").Value;
                Documents.Attach(document, key, rev.ToString());

                return response.Content.DeserializeObject<RESPONSE>();
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

    }
}