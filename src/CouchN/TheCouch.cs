using System;
using System.Collections.Generic;
using System.Reflection;

namespace CouchN
{
    public class TheCouch
    {
        public static CouchSession CreateSession(string database, Uri server = null)
        {
            server = server ?? new Uri("http://127.0.0.1:5984");
            return new CouchSession(server, database);
        }

        /// <summary>
        ///     a) Creates the database if it doesn't exist
        ///     b) Load Design documents from assembly containing the type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataabseName"></param>
        /// <param name="couchUri"></param>
        public static void SetupDatabaseAndDesignDocuments<T>(string dataabseName, Uri couchUri)
        {
            SetupDatabaseAndDesignDocuments(dataabseName, couchUri, typeof(T).Assembly);
        }

        public static void SetupDatabaseAndDesignDocuments(string dataabseName, Uri couchUri, Assembly assembly)
        {
            using (var session = TheCouch.CreateSession(dataabseName, couchUri))
                SetupHelper.SetDesignDocumentsFromAssembly(session, assembly);
        }

        
    }
}