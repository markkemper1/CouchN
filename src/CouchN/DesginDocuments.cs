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

        public ViewResult<VALUE, DOC> View<VALUE, DOC>(string viewName, ViewQuery query = null, bool track = false)
        {
            query = query ?? new ViewQuery();

            if (track && !query.IncludeDocs.HasValue)
                query.IncludeDocs = true;

            string path = basePath + "/_view/" + viewName;

            if (!track)
            {
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
                                return serializer.Deserialize<ViewDocsResultRaw<VALUE, DOC>>(jsonReader).ToViewResult();
                            }
                        }
                    }
                }
            }
            else
            {
                var jObjectResult = View<VALUE, JObject>(viewName, query, false);

                var final = new ViewResult<VALUE, DOC>();
                final.Offset = jObjectResult.Offset;
                final.Total = jObjectResult.Total;
                final.Rows = new ViewResult<VALUE, DOC>.RowInfo[jObjectResult.Rows.Length];
                final.Documents = new DOC[jObjectResult.Rows.Length];
                final.Values = jObjectResult.Values;

                var config = Documents.GetConfiguration<DOC>();

                for (int i = 0; i < jObjectResult.Rows.Length; i++)
                {
                    final.Rows[i] = new ViewResult<VALUE, DOC>.RowInfo()
                    {
                        Id = jObjectResult.Rows[i].Id,
                        Key = jObjectResult.Rows[i].Key
                    };

                    var jDoc = jObjectResult.Documents[i];
                    var hasDoc = jDoc != null;
                    var doc = hasDoc ? jDoc.ToObject<DOC>() : default(DOC);

                    final.Documents[i] = doc;

                    if (hasDoc)
                    {
                        if (config != null && config.SetInfo != null)
                            config.SetInfo(new Documents.DocumentInfo(final.Rows[i].Id, jDoc["_rev"].ToString()), doc);

                        this.Documents.Attach(doc, jDoc["_id"].Value<string>(), jDoc["_rev"].Value<string>());
                    }
                }

                return final;
            }
            //var stopWatch = new Stopwatch();

            //stopWatch.Start();

            //var responseContent = session.Get(path, query.ToDictionary());

            //stopWatch.Stop();
            //Console.WriteLine("Get Request took: {0}", stopWatch.ElapsedMilliseconds);

            //if (responseContent == null)
            //    throw new Exception("Design view not found: " + path);

            //var result = JObject.Parse(responseContent);

            //var final = responseContent.DeserializeObject<ViewResult<VALUE, DOC>>();

            //final.Documents = new DOC[final.Rows.Length];
            //final.Values = new VALUE[final.Rows.Length];

            //for (var i = 0; i < final.Rows.Length; i++)
            //{
            //    var document = result["rows"][i]["doc"];
            //    var value = result["rows"][i]["value"];

            //    final.Documents[i] = document != null ? document.ToString().DeserializeObject<DOC>() : default(DOC);

            //    var typedDocument = final.Documents[i];

            //    if (typedDocument != null)
            //    {
            //        var config = Documents.GetConfiguration<DOC>();

            //        if (config != null && config.SetInfo != null)
            //            config.SetInfo(new Documents.DocumentInfo(final.Rows[i].Id, document["_rev"].ToString()), typedDocument);
            //    }

            //    final.Values[i] = value != null ? value.ToObject<VALUE>() : default(VALUE);

            //    if (track)
            //        this.Documents.Attach(final.Documents[i], result["rows"][i]["doc"]["_id"].ToString(), result["rows"][i]["doc"]["_rev"].ToString());
            //}

            //return final;
        }



        public ViewResult<object, DOC> ViewDocs<DOC>(string viewName, ViewQuery query = null, bool track = false)
        {

            query = query ?? new ViewQuery();
            query.IncludeDocs = true;
            return View<object, DOC>(viewName, query, track);

            //query = query ?? new ViewQuery();
            //query.IncludeDocs = true;

            //string path = basePath + "/_view/" + viewName;

            //using (var client = new WebClient())
            //{
            //    using (var stream = client.OpenRead(session.GetUri(path, query.ToDictionary())))
            //    {
            //        using (var textReader = new StreamReader(stream, Encoding.UTF8))
            //        {
            //            using (var jsonReader = new JsonTextReader(textReader))
            //            {
            //                var serializer = new JsonSerializer();
            //                serializer.NullValueHandling = NullValueHandling.Ignore;
            //              return  serializer.Deserialize<ViewDocsResultRaw<object,DOC>>(jsonReader).ToViewResult();
            //            }
            //        }
            //    }
            //}



            //var responseContent = session.Get(path, query.ToDictionary());

            //    stopWatch.Stop();

            //    Console.WriteLine("Get Request took: {0}", stopWatch.ElapsedMilliseconds);

            //    if (responseContent == null)
            //        throw new Exception("Design view not found: " + path);

            //    stopWatch.Reset();
            //    stopWatch.Start();
            //    var resultRaw = responseContent.DeserializeObject<ViewDocsResultRaw<DOC>>();

            //    Console.WriteLine("Deserialization Took: {0}", stopWatch.ElapsedMilliseconds);
            //    return resultRaw;
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