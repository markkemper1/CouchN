using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace CouchN
{
    public class AttachmentList : Dictionary<string,Attachment>
    {
    }

   
    [DataContract]
    public class Attachment
    {
        [DataMember(Name = "content_type")]
        public string ContentType { get; set; }

        [DataMember(Name = "data")]
        public string Data { get; set; }

        [DataMember(Name = "revpos")]
        public int? Revpos { get; set; }

        [DataMember(Name = "digest")]
        public string Digest { get; set; }

        [DataMember(Name = "length")]
        public int? Length { get; set; }

        [DataMember(Name = "stub")]
        public bool? Stub { get; set; }

        [DataMember(Name = "follows")]
        public bool? Follows { get; set; }

        //public string content_type { get { return this["content_type"].Value<string>(); } }
        //public int revpos { get { return this["revpos"].Value<int>(); } }
        //public string digest { get { return this["digest"].Value<string>(); } }
        //public int length { get { return this["length"].Value<int>(); } }
        //public bool stub { get { return this["stub"].Value<bool>(); } }
    }
}
