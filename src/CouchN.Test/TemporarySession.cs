using System;

namespace CouchN.Test
{
     public class TemporarySession : CouchSession
    {
         public TemporarySession(Uri server = null)
             : base(server ?? new Uri("http://127.0.0.1:5984"), "test_" + Guid.NewGuid().ToString().Replace("-", "").ToLower() )
         {
             this.Db.Create();
         }

         public override void Dispose()
         {
             this.Db.Delete();
         }
    }
}
