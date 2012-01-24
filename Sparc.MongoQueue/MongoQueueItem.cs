namespace Sparc.MongoQueue
{
    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Driver.Builders;

    /// <summary>
    /// Represents an item on a MongoQueue.
    /// </summary>
    public class MongoQueueItem : BsonDocument
    {
        protected internal MongoCollection Collection { get; set; }

        /// <summary>
        /// Closes the current item and removes it completely from the db.
        /// </summary>
        public virtual void Close()
        {
            Collection.Remove(Query.EQ("_id", this["_id"]));
        }
    }
}
