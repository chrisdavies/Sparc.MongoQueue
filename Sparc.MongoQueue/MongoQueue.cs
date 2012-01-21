namespace Sparc.MongoQueue
{
    using System;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Driver.Builders;

    /// <summary>
    /// Manages a queuing mechanism using MongoDB.
    /// </summary>
    public class MongoQueue
    {
        private MongoDatabase db;

        /// <summary>
        /// Initializes a new instance of the MongoQueue class.
        /// </summary>
        /// <param name="db">The mongo database to use.</param>
        /// <param name="queueName">The name of the queue to be managed.
        /// (e.g. "Notifications", "BatchJobs", etc)
        /// </param>
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
            this.MaxProcessingTime = TimeSpan.FromMinutes(-30);
            this.CollectionName = "MongoQueue";
        }

        /// <summary>
        /// Gets the name of the queue being used.
        /// </summary>
        public string QueueName { get; private set; }

        /// <summary>
        /// Gets or sets the name of the Mongo collection to use.
        /// </summary>
        public string CollectionName { get; set; }

        /// <summary>
        /// Gets or sets the maximum processing time for queue items.
        /// The default is 30 minutes.  This should be a negative value.
        /// Setting this to TimeSpan.FromMinutes(-5) would mean that if an item
        /// had been dequeued 6 minutes ago, but was not closed, the queue
        /// would see that item as available for dequeuing again.
        /// </summary>
        public TimeSpan MaxProcessingTime { get; set; }

        /// <summary>
        /// Pushes an item on to the queue.
        /// </summary>
        /// <param name="data">The item to be pushed.</param>
        public void Push(BsonDocument data)
        {
            db.GetCollection(CollectionName).Save(new BsonDocument
            {
                { "name", this.QueueName },
                { "data", data },
                { "inProgress", false }
            });
        }

        /// <summary>
        /// Pops an item off of the queue.
        /// </summary>
        /// <param name="message">A custom message to store in the queued item.
        /// (e.g. "The foo app is processing this item.")
        /// </param>
        /// <returns>The item to process.</returns>
        public MongoQueueItem Pop(string message = null)
        {
            var collection = db.GetCollection(CollectionName);
            var query = Query.And(
                Query.EQ("name", this.QueueName),
                Query.Or(Query.EQ("inProgress", false), Query.LT("updated", DateTime.UtcNow.Add(MaxProcessingTime))));
            var item = collection.FindAndModify(
                query,
                SortBy.Null,
                Update.Set("inProgress", true).Set("machine", Environment.MachineName).Set("updated", DateTime.UtcNow).Set("message", message)).ModifiedDocument;

            return item == null ? null : new MongoQueueItem(collection, item);
        }
    }
}
