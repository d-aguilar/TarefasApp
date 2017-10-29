using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using TarefaFunction.Colecoes;

namespace TarefaFunction.Funcoes
{
    public static class CriacaoTarefa
    {
        [FunctionName("CriarTarefa")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tarefas/criar")]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                Task<HttpResponseMessage> result;

                switch (req.Method.Method)
                {
                    case "POST":
                        result = ExecutarPostAsync(req, log);
                        break;
                    default:
                        result = Comum.GetBadRequest(req, log);
                        break;
                }

                return await result;
            }
            catch (System.Exception ex)
            {
                log.Error("Ocorreu um erro.", ex);
                throw;
            }
        }

        private static async Task<HttpResponseMessage> ExecutarPostAsync(HttpRequestMessage req, TraceWriter log)
        {
            HttpResponseMessage response;
            var jsonContent = await req.Content.ReadAsStringAsync().ConfigureAwait(false);
            var doc = BsonSerializer.Deserialize<BsonDocument>(jsonContent);
            IMongoCollection<BsonDocument> collection = TarefaCollection.Instance;

            var id = ObjectId.Empty;

            await collection.InsertOneAsync(doc).ConfigureAwait(false);

            id = (ObjectId)doc["_id"];

            response = req.CreateResponse(HttpStatusCode.Created, $"Tarefa criada. Código da tarefa: {id}");

            return response;

        }
    }
}
