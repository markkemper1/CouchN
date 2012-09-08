using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CouchN
{
    [DataContract]
    public class ViewQuery
    {
        [DataMember(Name = "key")]
        public string Key { get; set; }

        [DataMember(Name = "keys")]
        public string[] Keys { get; set; }

        [DataMember(Name = "startkey")]
        public string StartKey { get; set; }

        /// <summary>
        /// document id to start with (to allow pagination for duplicate startkeys)
        /// </summary>
        [DataMember(Name = "startkey_docid")]
        public string StartKeyDocId { get; set; }

        [DataMember(Name = "endkey")]
        public string EndKey { get; set; }

        /// <summary>
        /// last document id to include in the output (to allow pagination for duplicate endkeys)
        /// </summary>
        [DataMember(Name = "endkey_docid")]
        public string EndKEyDocId { get; set; }

        /// <summary>
        ///     Limit the number of documents in the output
        /// </summary>
        [DataMember(Name = "limit")]
        public int? Limit { get; set; }

        /// <summary>
        /// If stale=ok is set, CouchDB will not refresh the view even if it is stale, the benefit is a an improved query latency. If stale=update_after is set, CouchDB will update the view after the stale result is returned. update_after was added in version 1.1.0.
        /// </summary>
        [DataMember(Name = "stale")]
        public bool? Stale { get; set; }

        /// <summary>
        /// [Default: false] change the direction of search
        /// </summary>
        [DataMember(Name = "descending")]
        public bool? Descending { get; set; }

        /// <summary>
        /// [Default: 0] skip n number of documents
        /// </summary>
        [DataMember(Name = "skip")]
        public int? Skip { get; set; }

        /// <summary>
        /// [Default: false] the group option controls whether the reduce function reduces to a set of distinct keys or to a single result row.
        /// </summary>
        [DataMember(Name = "group")]
        public bool? Group { get; set; }

        /// <summary>
        /// the group and group_level options control whether the reduce function reduces to a set of distinct keys or to a single result row. group_level lets you specify how many items of the key array are used in grouping; group=true is effectively the same as group_level=999 (for an arbitrarily high value of 999.) Don't specify both group and group_level; the second one given will override the first.
        /// </summary>
        [DataMember(Name = "group_level")]
        public int? GroupLevel { get; set; }

        /// <summary>
        /// [Default: true]  use the reduce function of the view. It defaults to true, if a reduce function is defined and to false otherwise.
        /// </summary>
        [DataMember(Name = "reduce")]
        public bool? Reduce { get; set; }

        /// <summary>
        /// [Default: false]  automatically fetch and include the document which emitted each view entry
        /// </summary>
        [DataMember(Name = "include_docs")]
        public bool? IncludeDocs { get; set; }

        /// <summary>
        /// [Default: true]  Controls whether the endkey is included in the result. It defaults to true.
        /// </summary>
        [DataMember(Name = "inclusive_end")]
        public bool? InclusiveEnd { get; set; }

        /// <summary>
        ///  [Default: false] Response includes an update_seq value indicating which sequence id of the database the view reflects
        /// </summary>
        [DataMember(Name = "update_seq")]
        public bool? UpdateSeq { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var query = new Dictionary<string, object>();
            if (Key != null) query["key"] = Key.Serialize();
            if (Keys != null) query["keys"] = Keys.Serialize();
            if (StartKey != null) query["startkey"] = StartKey.Serialize();
            if (EndKey != null) query["endkey"] = EndKey.Serialize();

            if (StartKeyDocId != null) query["startkey_docid"] = StartKeyDocId;
            if (EndKEyDocId != null) query["endkey_docid"] = EndKEyDocId;
            if (Limit != null) query["limit"] = Limit;
            if (Stale != null) query["stale"] = Stale;
            if (Descending != null) query["descending"] = Descending;
            if (Skip != null) query["skip"] = Skip;
            if (Group != null) query["group"] = Group;
            if (GroupLevel != null) query["group_level"] = GroupLevel;
            if (Reduce != null) query["reduce"] = Reduce;
            if (IncludeDocs != null) query["include_docs"] = IncludeDocs;
            if (InclusiveEnd != null) query["inclusive_end"] = InclusiveEnd;
            if (UpdateSeq != null) query["update_seq"] = UpdateSeq;
            return query;
        }
    }
}