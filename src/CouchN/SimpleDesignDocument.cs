using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CouchN
{
    [DataContract]
    public class SimpleDesignDocument
    {
        public SimpleDesignDocument()
        {
            this.Views = new Dictionary<string, View>();
            this.Updates = new Dictionary<string, string>();
            this.Shows = new Dictionary<string, string>();
            this.Rewrites = new List<object>();
            this.Lists = new Dictionary<string, string>();
        }

        [DataMember(Name = "views")]
        public Dictionary<string, View> Views { get; set; }

        [DataMember(Name = "updates")]
        public Dictionary<string, string> Updates { get; set; }

        [DataMember(Name = "shows")]
        public Dictionary<string, string> Shows { get; set; }

        [DataMember(Name = "rewrites")]
        public List<object> Rewrites { get; set; }

        [DataMember(Name = "lists")]
        public Dictionary<string, string> Lists { get; set; }

        [DataMember(Name = "validate_doc_update")]
        public string ValidateDocUpdate { get; set; }

        [DataContract]
        public class View
        {
            [DataMember(Name = "map")]
            public string Map { get; set; }

            [DataMember(Name = "reduce")]
            public string Reduce { get; set; }
        }
    }
}