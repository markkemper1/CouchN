using System;

namespace CouchN
{
    public class SwitchDatabase : IDisposable
    {
        private readonly CouchSession session;
        private readonly string database;

        public SwitchDatabase(CouchSession session, string database)
        {
            this.session = session;
            this.database = database;
            this.session.PushDatabase();
            this.session.Use(database);
        }

        public void Dispose()
        {
            this.session.PopAndUseDatabase();
        }
    }
}
