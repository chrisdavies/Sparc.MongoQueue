namespace Sparc.MongoQueue.Tests
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MongoDB.Driver;
    using MongoDB.Bson;
    using Should;
    using MongoDB.Driver.Builders;

    /// <summary>
    /// These require a local instance of MongoDB.  This will create a database
    /// called "MongoQueueTests", if it does not already exist.
    /// </summary>
    [TestClass]
    public class MongoQueueTests
    {
        private MongoCollection collection;
        private MongoDatabase db;

        public MongoQueueTests()
        {
            var server = MongoServer.Create("mongodb://localhost");
            this.db = server.GetDatabase("MongoQueueTests");
            this.collection = db.GetCollection(new MongoQueue(db, "Init").CollectionName);
        }

        [TestInitialize]
        public void Before_each_test()
        {
            this.collection.RemoveAll();
        }

        [TestMethod]
        public void Pop_gets_items_from_the_proper_queue_with_their_data()
        {
            var q1 = new MongoQueue(db, "Q1");
            var q2 = new MongoQueue(db, "Q2");

            q2.Push(new BsonDocument { { "hello", "1" } });
            q1.Push(new BsonDocument { { "goodbye", "2" } });
            q2.Push(new BsonDocument { { "hello", "3" } });

            var item = q1.Pop();
            item.ShouldNotBeNull();
            item["goodbye"].AsString.ShouldEqual("2");
            q1.Pop().ShouldBeNull();
        }
        
        [TestMethod]
        public void Pop_gets_expired_items()
        {
            var q1 = new MongoQueue(db, "Q1");
            q1.Push(new BsonDocument { { "goodbye", "2" } });
            var item = q1.Pop();
            item.ShouldNotBeNull();
            q1.Pop().ShouldBeNull();

            collection.Update(Query.NE("name", BsonNull.Value), Update.Set("updated", DateTime.UtcNow.AddMinutes(-31)));
            q1.Pop().ShouldNotBeNull();
        }
        
        [TestMethod]
        public void Pop_sets_metadata()
        {
            var q = new MongoQueue(db, "Q1");
            q.Push(new BsonDocument());
            q.Pop("Hello World");
            var doc = collection.FindOneAs<BsonDocument>();
            doc["name"].AsString.ShouldEqual("Q1");
            doc["machine"].AsString.ShouldEqual(Environment.MachineName);
            doc["message"].AsString.ShouldEqual("Hello World");
        }
                
        [TestMethod]
        public void Update_refreshes_timestamp_and_message()
        {
            var q = new MongoQueue(db, "Q1");
            q.Push(new BsonDocument());
            var item = q.Pop();

            var doc = collection.FindOneAs<BsonDocument>();
            doc["updated"] = DateTime.UtcNow.AddHours(-1);
            collection.Save(doc);
            
            var now = DateTime.UtcNow;
            item.Update("Did stuff");

            doc = collection.FindOneAs<BsonDocument>();
            doc["updated"].AsDateTime.ShouldBeInRange(now.AddSeconds(-2), now.AddSeconds(2));
            doc["message"].AsString.ShouldEqual("Did stuff");
        }

        [TestMethod]
        public void Close_removes_the_item()
        {
            var q = new MongoQueue(db, "Q1");
            q.Push(new BsonDocument());
            var item = q.Pop();
            item.Close();
            collection.Count().ShouldEqual(0);
        }
    }
}
