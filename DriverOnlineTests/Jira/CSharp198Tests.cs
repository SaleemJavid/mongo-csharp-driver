﻿/* Copyright 2010-2011 10gen Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using NUnit.Framework;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace MongoDB.DriverOnlineTests.Jira.CSharp198 {
    [TestFixture]
    public class CSharp198Tests {
        public class Id {
            public int AccountId;
            public int Index;
        }

        public class IdWithExtraField : Id {
            public int Extra;
        }

        public class Foo {
            public Id Id;
            public string Name;
        }

        private MongoServer server;
        private MongoDatabase database;
        private MongoCollection<Foo> collection;

        [TestFixtureSetUp]
        public void TestFixtureSetup() {
            server = MongoServer.Create("mongodb://localhost/?safe=true");
            database = server["onlinetests"];
            collection = database.GetCollection<Foo>("csharp198");
        }

        [Test]
        public void TestSave() {
            collection.RemoveAll();
            var foo1 = new Foo {
                Id = new Id { AccountId = 1, Index = 2 },
                Name = "foo1"
            };
            collection.Save(foo1);

            var foo1Rehydrated = collection.FindOne(Query.EQ("_id", BsonDocumentWrapper.Create(foo1.Id)));
            Assert.IsInstanceOf<Foo>(foo1Rehydrated);
            Assert.IsInstanceOf<Id>(foo1Rehydrated.Id);
            Assert.AreEqual(1, foo1Rehydrated.Id.AccountId);
            Assert.AreEqual(2, foo1Rehydrated.Id.Index);
            Assert.AreEqual("foo1", foo1Rehydrated.Name);

            var foo2 = new Foo {
                Id = new IdWithExtraField { AccountId = 3, Index = 4, Extra = 5 },
                Name = "foo2"
            };
            collection.Save(foo2);

            var foo2Rehydrated = collection.FindOne(Query.EQ("_id", BsonDocumentWrapper.Create(foo2.Id)));
            Assert.IsInstanceOf<Foo>(foo2Rehydrated);
            Assert.IsInstanceOf<IdWithExtraField>(foo2Rehydrated.Id);
            Assert.AreEqual(3, foo2Rehydrated.Id.AccountId);
            Assert.AreEqual(4, foo2Rehydrated.Id.Index);
            Assert.AreEqual(5, ((IdWithExtraField) foo2Rehydrated.Id).Extra);
            Assert.AreEqual("foo2", foo2Rehydrated.Name);
        }
    }
}
