using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace CouchN
{
    [DataContract]
    public class ChangesQuery
    {
        /// <summary>
        ///   [Default: 0]  Start the results from the change immediately after the given sequence number.
        /// </summary>
        [DataMember(Name = "since")]
        public int? Since { get; set; }

        /// <summary>
        ///    [Default: none]  Limit number of result rows to the specified value (note that using 0 here has the same effect as 1: get a single result row).
        /// </summary>
        [DataMember(Name = "limit")]
        public int? Limit { get; set; }

        /// <summary>
        /// [Default: false] change the direction of search
        /// </summary>
        [DataMember(Name = "descending")]
        public bool? Descending { get; set; }

        /// <summary>
        /// [Default: normal]    Select the type of feed.
        /// </summary>
        [DataMember(Name = "feed")]
        public FeedType? Feed { get; set; }

        /// <summary>
        /// [Default: 60000]    Period in milliseconds after which an empty line is sent in the results. Only applicable for longpoll or continuous feeds. Overrides any timeout to keep the feed alive indefinitely.
        /// </summary>
        [DataMember(Name = "heartbeat")]
        public int? HeartBeat { get; set; }

        /// <summary>
        /// [Default: 60000]    Maximum period in milliseconds to wait for a change before the response is sent, even if there are no results. Only applicable for longpoll or continuous feeds. Note that 60000 is also the default maximum timeout to prevent undetected dead connections.*
        /// </summary>
        [DataMember(Name = "timeout")]
        public int? Timeout { get; set; }

        /// <summary>
        /// [Default: none]    Reference a filter function from a design document to selectively get updates. See the section in the book for more information.
        /// </summary>
        [DataMember(Name = "filter")]
        public string Filter { get; set; }

        /// <summary>
        /// [Default: false]  automatically fetch and include the document which emitted each view entry
        /// </summary>
        [DataMember(Name = "include_docs")]
        public bool? IncludeDocs { get; set; }

        /// <summary>
        /// [Default: main_only]  Specifies how many revisions are returned in the changes array. The default, main_only, will only return the current "winning" revision; all_docs will return all leaf revisions (including conflicts and deleted former conflicts.)
        /// </summary>
        [DataMember(Name = "style")]
        public ResultStyleType? style { get; set; }


        public Dictionary<string, object> ToDictionary()
        {
            var query = new Dictionary<string, object>();

            if (Since != null) query["since"] = Since;
            if (Limit != null) query["limit"] = Limit;
            if (Descending.HasValue) query["descending"] = Descending.Value ? "true" : "false";
            if (Feed.HasValue) query["feed"] = Feed.Value.ToString();
            if (HeartBeat != null) query["heartbeat"] = HeartBeat;
            if (Timeout != null) query["timeout"] = Timeout;
            if (Filter != null) query["filter"] = Filter;
            if (IncludeDocs.HasValue) query["include_docs"] = IncludeDocs.Value ? "true" : "false";
            if (style.HasValue) query["style"] = style.Value.ToString();


            return query;
        }

        public enum ResultStyleType
        {
            all_docs,
            main_only
        }

        public enum FeedType
        {
            normal,
            longpoll,
            continuous
        }

    }
}