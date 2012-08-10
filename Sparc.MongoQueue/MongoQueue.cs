namespace Sparc.MongoQueue
{
    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Driver.Builders;
    using System;

    public class MongoQueue
    {
        private MongoDatabase db;

        public string QueueName { get; private set; }

        public string CollectionName { get; set; }

        public TimeSpan MaxProcessingTime { get; set; }

        public MongoQueue(MongoDatabase db, string queueName)
        {
            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentNullException("queueName");
            }

            if (db == null)
            {
                throw new ArgumentNullException("db");
            }

            this.db = db;
            this.QueueName = queueName;
            this.MaxProcessingTime = TimeSpan.FromMinutes(30.0);
            this.CollectionName = "MongoQueue";
        }

        public void Push(BsonDocument data, Schedule schedule = null)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            schedule = schedule ?? new Schedule { Repeat = Repeat.None, NextRun = DateTime.UtcNow };

            var meta = new BsonDocument();
            meta["QueueName"] = this.QueueName;
            meta["Schedule"] = schedule.ToBsonDocument();
            data["MongoQueue"] = meta;
            this.db.GetCollection(this.CollectionName).Save(data);
        }

        public MongoQueueItem Pop()
        {
            var collection = this.db.GetCollection(this.CollectionName);
            var query = Query.And(
                Query.EQ("MongoQueue.QueueName", this.QueueName),
                Query.Or(
                    Query.NotExists("MongoQueue.Schedule"),
                    Query.LTE("MongoQueue.Schedule.NextRun", DateTime.UtcNow)));
            var result = collection.FindAndModify(
                query, 
                SortBy.Null, 
                Update.Set("MongoQueue.Machine", Environment.MachineName).Set("MongoQueue.Schedule.NextRun", DateTime.UtcNow.Add(this.MaxProcessingTime)))
                .GetModifiedDocumentAs<MongoQueueItem>();
            
            if (result != null)
            {
                result.Collection = collection;
            }

            return result;
        }
    }
}
