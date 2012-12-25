using System.Collections.Generic;
using RestSharp;

namespace CouchN
{
    public static class RestClientExtensions
    {
        public static void AddQuery(this RestRequest request, IDictionary<string, object> query)
        {
            if (query != null)
            {
                foreach (var kv in query)
                    request.AddParameter(kv.Key, kv.Value);
            }
        }

         public static void AddJson<T>(this RestRequest request, T value)
         {
             request.AddParameter("application/json", value.Serialize(), ParameterType.RequestBody);
         }
    }
}
