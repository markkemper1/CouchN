using System;
using System.Collections.Generic;

namespace CouchN
{
    public class Databases
    {
        private readonly CouchSession session;

        public Databases(CouchSession session)
        {
            this.session = session;
        }

        public Database Get()
        {
            return session.Get<Database>("");
        }

        public void Create()
        {
            try
            {
                var response = session.Put("");
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error creating database, does it already exist? Status code: ", ex);
            }
        }

        public void Delete()
        {
            session.Delete("", new Dictionary<string, object>());
        }
    }
}