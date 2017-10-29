using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using TarefaFunction.Colecoes;
using System.Collections.Generic;

namespace TarefaFunction.Funcoes
{
    public static class Tarefa
    {
        [FunctionName("Tarefa")]
        public static Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "put", "delete", Route = "tarefas/{idTarefa?}")]HttpRequestMessage req, TraceWriter log, string idTarefa)
        {
            try
            {
                log.Info("Executando requisição.");
                Task<HttpResponseMessage> result;
                var tarefasCollections = TarefaCollection.Instance;

                switch (req.Method.Method)
                {
                    case "GET":
                        result = ExecutarGetAsync(req, idTarefa, log, tarefasCollections);
                        break;
                    case "PUT":
                        result = ExecutarPutAsync(req, idTarefa, log, tarefasCollections);
                        break;
                    case "DELETE":
                        result = ExecutarDeleteAsync(req, idTarefa, log, tarefasCollections);
                        break;
                    default:
                        result = Comum.GetBadRequest(req, log);
                        break;
                }

                log.Info("Término requisição.");

                return result;
            }
            catch (System.Exception ex)
            {
                log.Error("Ocorreu um erro.", ex);
                throw;
            }
        }

        private static async Task<HttpResponseMessage> ExecutarGetAsync(HttpRequestMessage req, string idTarefa, TraceWriter log, IMongoCollection<BsonDocument> collection)
        {
            HttpResponseMessage response;
            List<BsonDocument> resultado;

            var nomeTarefa = Comum.GetQueryString(req, "nome");
            
            if(!string.IsNullOrWhiteSpace(idTarefa))
            {
                var filter = Builders<BsonDocument>.Filter.Eq<ObjectId>("_id", ObjectId.Parse(idTarefa));
                resultado = await collection.Find(filter).ToListAsync().ConfigureAwait(false);

                if (resultado.Count() > 0)
                {
                    response = req.CreateResponse(HttpStatusCode.OK, resultado[0].ToString());
                }
                else
                {
                    response = req.CreateResponse(HttpStatusCode.NotFound, $"Tarefa não encontrada. Código da tarefa: {idTarefa}");
                }
            }
            else if(!string.IsNullOrEmpty(nomeTarefa))
            {
                var filter = Builders<BsonDocument>.Filter.Regex("Nome", new BsonRegularExpression(nomeTarefa));
                resultado = await collection.Find(filter).ToListAsync().ConfigureAwait(false);
                response = req.CreateResponse(HttpStatusCode.OK, resultado.ToJson());
            }
            else
            {
                resultado = await collection.Find(Builders<BsonDocument>.Filter.Empty).ToListAsync().ConfigureAwait(false);
                response = req.CreateResponse(HttpStatusCode.OK, resultado.ToJson());
            }

            return response;
        }

        private static async Task<HttpResponseMessage> ExecutarDeleteAsync(HttpRequestMessage req, string idTarefa, TraceWriter log, IMongoCollection<BsonDocument> collection)
        {
            HttpResponseMessage response;
            var filter = Builders<BsonDocument>.Filter.Eq<ObjectId>("_id", ObjectId.Parse(idTarefa));
            var results = await collection.FindOneAndDeleteAsync(filter).ConfigureAwait(false);
            if (results.Count() > 0)
            {
                response = req.CreateResponse(HttpStatusCode.OK, "Tarefa removida com sucesso.");
            }
            else
            {
                response = req.CreateResponse(HttpStatusCode.NotFound, $"Tarefa não pode ser removida. Código da tarefa: {idTarefa}");
            }

            return response;
        }

        private static async Task<HttpResponseMessage> ExecutarPutAsync(HttpRequestMessage req, string idTarefa, TraceWriter log, IMongoCollection<BsonDocument> collection)
        {
            HttpResponseMessage response;
            var filtro = Builders<BsonDocument>.Filter.Eq<ObjectId>("_id", ObjectId.Parse(idTarefa));
            var tarefaJson = await req.Content.ReadAsStringAsync();
            var tarefaAlterada = Comum.ConverterJsonToBson(tarefaJson);

            UpdateDefinition<BsonDocument> update = null;
            foreach (var propriedade in tarefaAlterada)
            {
                if (update == null)
                {
                    update = Builders<BsonDocument>.Update.Set(propriedade.Name, propriedade.Value);
                }
                else
                {
                    update = update.Set(propriedade.Name, propriedade.Value);
                }
            }

            var resultadoUpdate = await collection.UpdateOneAsync(filtro, update).ConfigureAwait(false);

            if (resultadoUpdate.ModifiedCount == 1)
            {
                response =  req.CreateResponse(HttpStatusCode.OK, $"Tarefa alterada com sucesso. Código da tarefa: {idTarefa}");
            }
            else
            {
                response = req.CreateResponse(HttpStatusCode.NotFound, $"Tarefa não pode ser atualizada. Código da tarefa: {idTarefa}");
            }

            return response;
        }

    }
}
