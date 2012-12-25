using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CouchN
{
    public static class SetupHelper
    {
        public static void SetDesignDocumentsFromAssembly<T>(CouchSession session, Dictionary<string, string> databaseOverrides = null)
        {
            SetDesignDocumentsFromAssembly(session, typeof(T).Assembly, databaseOverrides);
        }

        public static void SetDesignDocumentsFromAssembly(CouchSession session, Assembly assembly, Dictionary<string, string> databaseOverrides = null)
        {
            databaseOverrides = databaseOverrides ?? new Dictionary<string, string>();

            var designDocs = GetDesignDocuments(assembly);

            if (session.Db.Get() == null)
                session.Db.Create();

            var defaultDb = session.DatabaseName;

            foreach (var databaseOverride in databaseOverrides)
            {
                using (session.Switch(databaseOverride.Value))
                {
                    if (session.Db.Get() == null)
                        session.Db.Create();
                }
            }

            foreach (var doc in designDocs)
            {       
                using (session.Switch(databaseOverrides.ContainsKey(doc.Key) ? databaseOverrides[doc.Key] : defaultDb))
                {
                    session.Design(doc.Key).SetDocument(doc.Value);
                }
            }
        }

        //Gets all the design documents from the assembly, following naming convention: _design-[name].json
        private static Dictionary<string, string> GetDesignDocuments(Assembly assembly)
        {
            var items = new Dictionary<string, string>();

            foreach (var name in assembly.GetManifestResourceNames())
            {
                var _designPos = name.IndexOf("_design.json");
                if (_designPos < 0)
                    continue;

                var contents = GetFile(assembly, name);

                var split = name.Replace("_design.json", "").Split('.');
                var path = split.Last();
                items.Add(path, contents);
            }

            return items;
        }

        private static string GetFile(Assembly assembly, string name)
        {
            using (var stream = assembly.GetManifestResourceStream(name))
            using (var reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }
    }
}
