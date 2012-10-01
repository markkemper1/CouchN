using System.Runtime.Serialization;

namespace CouchN
{
    [DataContract]
    public class Database
    {
        //{"db_name":"test_e11bc30ca5dd494aace274a98ec808c0",
        //"doc_count":0,
        //"doc_del_count":0,
        //"update_seq":0,
        //"purge_seq":0,
        //"compact_running":false,
        //"disk_size":79,
        //"data_size":0,
        //"instance_start_time":"1346806651591181",
        //"disk_format_version":6,
        //"committed_update_seq":0}

        [DataMember(Name="db_name")]
        public string Name { get; set; }

        [DataMember(Name="doc_count")]
        public long DocCount { get; set; }

        [DataMember(Name="doc_del_count")]
        public long DocDeleteCount { get; set; }

        [DataMember(Name="update_seq")]
        public string UpdateSeq { get; set; }

        [DataMember(Name="purge_seq")]
        public long PurgeSeq { get; set; }

        [DataMember(Name="compact_running")]
        public bool IsCompactRunning { get; set; }
        
        [DataMember(Name="disk_size")]
        public int DiskSize { get; set; }

        [DataMember(Name="disk_format_version")]
        public int DiskFormatVersion { get; set; }
        
        [DataMember(Name="committed_update_seq")]
        public long CommittedUpdateSeq { get; set; }
    }
}
