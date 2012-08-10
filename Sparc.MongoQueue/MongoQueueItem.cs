namespace Sparc.MongoQueue
{
    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Driver.Builders;
    using System;

    public class MongoQueueItem : BsonDocument
    {
        protected internal MongoCollection Collection { get; set; }

        public virtual void Close()
        {
            this.Collection.Remove(Query.EQ("_id", this["_id"]));
        }

        public virtual void Reschedule(DateTime nextRun)
        {
            Schedule schedule = new Schedule
            {
                NextRun = nextRun,
                Repeat = Repeat.Custom
            };

            this["MongoQueue"].ToBsonDocument().Set("Schedule", schedule.ToBsonDocument());
            this.Collection.Save(this);
        }
    }
}
