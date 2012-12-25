using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace CouchN
{
    public class BulkDocuments
    {
        private readonly CouchSession session;

        public BulkDocuments(CouchSession session)
        {
            this.session = session;
        }

        public AllDocsResult All(ViewQuery query = null)
        {
            query = query ?? new ViewQuery();

            var path = "_all_docs";

            var responseContent = session.Get(path, query.ToDictionary());
            var final = responseContent.DeserializeObject<AllDocsResult>();

            return final;
        }

        public BulkResponse[] Update(JObject[] documents)
        {
            var docWrapper = new { docs = documents};

            var path = "_bulk_docs";
            var request = session.PostRequest(path);
            request.AddJson(docWrapper);
            var response = this.session.Client.Execute(request);

            if (response.StatusCode != HttpStatusCode.Created
              && response.StatusCode != HttpStatusCode.Accepted
              && response.StatusCode != HttpStatusCode.OK
              )
                throw new ApplicationException("Failed: " + response.StatusCode + " - " + response.Content);

            return response.Content.DeserializeObject<BulkResponse[]>();
        }

        public BulkResponse[] Delete(object[] documents)
        {
            var docWrapper = new { docs = JArray.FromObject(documents) };

            foreach (var item in docWrapper.docs)
            {
                item["_deleted"] = true;
            }

            var path = "_bulk_docs";

            var request = session.PostRequest(path);

            request.AddJson(docWrapper);

            var response = this.session.Client.Execute(request);

            if (response.StatusCode != HttpStatusCode.Created
              && response.StatusCode != HttpStatusCode.Accepted
              && response.StatusCode != HttpStatusCode.OK
              )
                throw new ApplicationException("Failed: " + response.StatusCode + " - " + response.Content);

            return response.Content.DeserializeObject<BulkResponse[]>();       
        }
    }


    [DataContract]
    public class BulkResponse
    {
        [DataMember(Name = "error")]
        public string Error { get; set; }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "rev")]
        public string Rev { get; set; }
    }


    [DataContract]
    public class AllDocsResult
    {
        private DocumentHelper documents;

        [DataMember(Name = "total_rows")]
        public int Total { get; set; }

        [DataMember(Name = "offset")]
        public int Offset { get; set; }

        [DataMember(Name = "rows")]
        public RowInfo[] Rows { get; set; }

        public DocumentHelper Documents
        {
            get
            {
                if(documents==null)
                    documents = new DocumentHelper(this.Rows);
                return documents;
            }
        }

        [DataContract]
        public class RowInfo
        {
            [DataMember(Name = "id")]
            public string Id { get; set; }

            [DataMember(Name = "key")]
            public object Key { get; set; }

            [DataMember(Name = "value")]
            public RowInfoValue Value { get; set; }

            [DataMember(Name = "doc")]
            public JObject Document { get; set; }
        }


        [DataContract]
        public class RowInfoValue
        {
            [DataMember(Name = "rev")]
            public string Revision { get; set; }
        }

        public class DocumentHelper : IEnumerable<JObject>
        {
            private readonly RowInfo[]  rows;

            public DocumentHelper(RowInfo[] rows)
            {
                if (rows == null) throw new ArgumentNullException("rows");
                this.rows = rows;
            }

            public JObject this[int index]
            {
                get { return rows[index].Document; }
            }

            public IEnumerator<JObject> GetEnumerator()
            {
                return this.rows.Select(item => item.Document).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
