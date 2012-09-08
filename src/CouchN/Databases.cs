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
            var response = session.Put("");

            if (response != 201)
                throw new ApplicationException("Error creating database, does it already exist? Status code: " + response);
        }

        public void Delete()
        {
            session.Delete("", new Dictionary<string, object>());
        }
    }
}