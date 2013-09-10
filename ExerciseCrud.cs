using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;

// Core
using MongoDB.Bson;
using MongoDB.Driver;

// Optional
using MongoDB.Driver.Builders;
using MongoDB.Driver.GridFS;
using MongoDB.Driver.Linq;


namespace MongoDbTest
{
    [TestFixture]
    public class ExerciseCrud
    {
        // Check out MongoLab.com to set up a free test environment https://mongolab.com/welcome/ 
        // or download the bits and spin up your own server http://www.mongodb.org/downloads
        const string ConnectionString = "mongodb://<dbuser>:<dbpassword>@<instance>.mongolab.com:<port>/<database>";

        private MongoDatabase _db;

        [TestFixtureSetUp]
        public void Setup()
        {
            _db = ConnectAndGetDatabase();
            _db.ShouldNotBe(null);
        }

        [Test]
        public void CanAddOneRandomRecord()
        {
            var collection = _db.GetCollection<SquatchTestObject>("TestObjects");
            var item = new SquatchTestObject()
                {
                    Message = "This is a random test: " + Guid.NewGuid().ToString(),
                    V = 1
                };

            var result = collection.Insert(item);
            item.Id.ShouldNotBe(ObjectId.Empty);
        }

        [Test]
        public void CanCreateUpdateAndDeleteAllInOneMassiveMethod()
        {
            string updateMessage = "This is an update";

            var collection = _db.GetCollection<SquatchTestObject>("TestObjects");
            var item = new SquatchTestObject()
            {
                Message = "This is a random test: " + Guid.NewGuid().ToString(),
                V = 1
            };

            // Add
            collection.Insert(item);

            // Find
            var query = Query<SquatchTestObject>.EQ(e => e.Id, item.Id);
            var entity = collection.FindOne(query);
            entity.Message.ShouldBe(item.Message);
            
            // Update
            var changeMessageUpdate = Update<SquatchTestObject>.Set(e => e.Message, updateMessage).Set(e => e.V, 2);
            collection.Update(query, changeMessageUpdate);

            // Check update was written
            var secondFind = collection.FindOne(query);
            secondFind.Message.ShouldBe(updateMessage);
            secondFind.V.ShouldBe(2);

            // Delete
            collection.Remove(query);
            var gone = collection.FindOne(query);
            Assert.IsNull(gone);
        }


        #region -- Helpers --
        
        private MongoDatabase ConnectAndGetDatabase()
        {
            var client = new MongoClient(ConnectionString);

            var server = client.GetServer();

            server.ShouldNotBe(null);
            //server.Connect();

            return server.GetDatabase("sandbox-testing");
        } 

        #endregion
    }

    public class SquatchTestObject
    {
        public int V { get; set; }
        public ObjectId Id { get; set; }
        public string Message { get; set; }
    }
}
