using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TarefaFunction.Colecoes
{
    public sealed class TarefaCollection
    {
        private static volatile IMongoCollection<BsonDocument> instance;
        private static object syncRoot = new Object();

        private TarefaCollection() { }

        public static IMongoCollection<BsonDocument> Instance
        {
            get
            {
                    
if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            string connectionString = System.Environment.GetEnvironmentVariable("DbTarefas");
                            var client = new MongoClient(connectionString);
                            var db = client.GetDatabase("pos-graduacao");
                            instance = db.GetCollection<BsonDocument>("tarefas");
                        }
                    }
                }
                return instance;
            }
        }
    }
}
