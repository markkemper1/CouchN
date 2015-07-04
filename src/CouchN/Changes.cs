using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json.Linq;

namespace CouchN
{
    public class Changes
    {
        private readonly CouchSession session;
        private readonly string basePath;

        public Changes(CouchSession session)
        {
            this.session = session;
            this.basePath = "_changes";
        }


        public ChangesResult Poll(ChangesQuery query = null)
        {
            query = query ?? new ChangesQuery();
            query.Feed = ChangesQuery.FeedType.normal;

            return Get(query);
        }

        public ChangesResult LongPoll(ChangesQuery query = null)
        {
            query = query ?? new ChangesQuery();
            query.Feed = ChangesQuery.FeedType.longpoll;

            return Get(query);
        }

        private ChangesResult Get(ChangesQuery query)
        {
            string path = "_changes";

            var responseContent = session.Get(path, query.ToDictionary());

            return responseContent.DeserializeObject<ChangesResult>();
        }

        public void Stream(Action<string> onLine, ChangesQuery query = null, Action<KeyValuePair<string,string>> onHeader  = null)
        {
            if (onLine == null) throw new ArgumentNullException("onLine");

            query = query ?? new ChangesQuery();
            query.Feed = ChangesQuery.FeedType.continuous;

            var sb = new StringBuilder();
            sb.AppendFormat("GET /{0}/_changes{1} HTTP/1.1\r\n", session.DatabaseName, session.ToQueryString(query.ToDictionary()));
            sb.AppendFormat("Host: {0}\r\n", session.baseUri.Host);
            sb.AppendFormat("Connection: keep-alive\r\n");
            sb.AppendFormat("\r\n");

            using (var client = new TcpClient())
            {
                client.ReceiveTimeout = query.Timeout ?? 60000;
                client.NoDelay = true;
                client.Connect(session.baseUri.Host, session.baseUri.Port);

                using (var stream = client.GetStream())
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(sb.ToString());
                        writer.Flush();

                        using (var reader = new StreamReader(stream))
                        {
                            bool headersRead = false;
                            bool dataLengthRead = false;

                            while (!reader.EndOfStream)
                            {
                                var line = reader.ReadLine();

                                
                                if (line.StartsWith("HTTP/") && !headersRead)
                                {
                                    continue;
                                    }

                                if (line == "" && !headersRead)
                                {
                                    headersRead = true;
                                    continue;
                                }

                                if(String.IsNullOrWhiteSpace(line))
                                    continue;

                                if (!headersRead)
                                {
                                    var split = line.Split(':');

                                    if (split.Length > 1)
                                    {
                                        if (onHeader != null)
                                            onHeader(new KeyValuePair<string, string>(split[0], split[1]));
                                        continue;
                                    }
                                }

                                if (!dataLengthRead)
                                {
                                    dataLengthRead = true;
                                    continue;
                                }

                                onLine(line);

                                dataLengthRead = false;
                            }
                        }
                    }
                }
            }
        }
    }

    [DataContract]
    public class ChangesResult
    {
        [DataMember(Name = "results")]
        public ChangeRow[] Results { get; set; }

        [DataMember(Name = "last_seq")]
        public int LastSequence { get; set; }

        [DataContract]
        public class ChangeRow
        {
            [DataMember(Name = "seq")]
            public int Sequence { get; set; }

            [DataMember(Name = "id")]
            public string Id { get; set; }

            [DataMember(Name = "changes")]
            public Change[] Changes { get; set; }

            [DataMember(Name = "doc")]
            public JObject Document { get; set; }

            [DataContract]
            public class Change
            {
                [DataMember(Name = "rev")]
                public string Revision { get; set; }
            }
        }
    }
}