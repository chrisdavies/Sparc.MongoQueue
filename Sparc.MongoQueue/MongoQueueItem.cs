namespace Sparc.MongoQueue
{
    using System;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Driver.Builders;
    using System.Collections.Generic;

    /// <summary>
    /// Represents an item on a MongoQueue.
    /// </summary>
    public class MongoQueueItem : BsonDocument
    {
        private MongoCollection collection;
        private QueryComplete me;

        internal MongoQueueItem(MongoCollection collection, BsonDocument item)
            : base(item["data"] as IEnumerable<BsonElement>)
        {
            me = Query.EQ("_id", item["_id"]);
            this.collection = collection;
        }

        /// <summary>
        /// Updates the current queued item, posting the specified message
        /// and updating the item's timestamp.
        /// </summary>
        /// <param name="message">The message to post.</param>
        public void Update(string message)
        {
            var update = MongoDB.Driver.Builders.Update
                    .Set("message", message)
                    .Set("updated", DateTime.UtcNow);

            collection.Update(me, update);
        }

        /// <summary>
        /// Closes the current item and removes it completely from the db.
        /// </summary>
        public void Close()
        {
            collection.Remove(me);
        }
    }
}
