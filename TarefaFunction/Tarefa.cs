using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;

namespace TarefaFunction
{
    public static class Tarefa
    {
        [FunctionName("Tarefa")]
        public static Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "HttpTriggerCSharp/name/{idTarefa}")]HttpRequestMessage req, string idTarefa, TraceWriter log)
        {
            try
            {
                Task<HttpResponseMessage> result;

                log.Info("C# HTTP trigger function processed a request.");
                var tarefasCollections = TarefaCollection.Instance;

                result = ExecutarGet(req, idTarefa, log, tarefasCollections);
                // Fetching the name from the path parameter in the request URL

                return result;
            }
            catch (System.Exception ex)
            {
                log.Error("Um erro ocorreu.", ex);
                throw;
            }
        }

        private static async Task<HttpResponseMessage> ExecutarGet(HttpRequestMessage req, string idTarefa, TraceWriter log, IMongoCollection<BsonDocument> collection)
        {
            HttpResponseMessage response;
            var filter = Builders<BsonDocument>.Filter.Eq("idTarefa", idTarefa);
            var results = await collection.Find(filter).ToListAsync().ConfigureAwait(false);
            if (results.Count > 0)
            {
                response = req.CreateResponse(HttpStatusCode.OK, results[0].ToString());
            }

            response =  req.CreateResponse(HttpStatusCode.NotFound, $"Tarefa não encontrada. Código da tarefa: {idTarefa}");

            return response;
        }
    }
}
