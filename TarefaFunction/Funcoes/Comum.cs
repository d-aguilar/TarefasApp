using Microsoft.Azure.WebJobs.Host;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TarefaFunction.Funcoes
{
    public static class Comum
    {
        public static async Task<HttpResponseMessage> GetBadRequest(HttpRequestMessage req, TraceWriter log)
        {
            HttpResponseMessage response;
            response = req.CreateResponse(HttpStatusCode.MethodNotAllowed, $"Método não permitido.");

            return response;
        }

        public static BsonDocument ConverterJsonToBson(string json)
        {
            try
            {
                var bsonDocumento = BsonSerializer.Deserialize<BsonDocument>(json);
                return bsonDocumento;
            }
            catch (System.FormatException ex)
            {
                var msg = $"Json inválido: {json}";
                throw ex;
            }
        }

        public static string GetQueryString(this HttpRequestMessage request, string key)
        {
            // IEnumerable<KeyValuePair<string,string>> - right!
            var queryStrings = request.GetQueryNameValuePairs();
            if (queryStrings == null)
                return null;

            var match = queryStrings.FirstOrDefault(kv => string.Compare(kv.Key, key, true) == 0);
            if (string.IsNullOrEmpty(match.Value))
                return null;

            return match.Value;
        }

    }
}
