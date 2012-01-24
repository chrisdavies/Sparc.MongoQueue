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
        private QueryComplete mongoQuery;

        internal MongoQueueItem(MongoCollection collection, BsonDocument item)
            : base(item["Data"] as IEnumerable<BsonElement>)
        {
            mongoQuery = Query.EQ("_id", item["_id"]);
            this.Collection = collection;
        }

        protected MongoCollection Collection { get; set; }

        /// <summary>
        /// Closes the current item and removes it completely from the db.
        /// </summary>
        public virtual void Close()
        {
            Collection.Remove(mongoQuery);
        }
    }
}
