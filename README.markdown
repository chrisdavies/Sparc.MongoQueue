# Sparc.MongoQueue
This library provides queuing logic on top of MongoDB.  Queued items are given a maximum processing time (defualting to 30 minutes).  If a dequeued item is not processed (or at least updated) within the specified amount of time, it is requeued.

# Installing
Sparc.Mvc is available on NuGet here: http://nuget.org/packages/Sparc.MongoQueue

# Usage


    // In the enqueuing app:
    var q = new MongoQueue(myMongoDatabase, "Notifications");
    q.Push(new BsonDocument { { "Foo", "Bar" } });
    
    // In the dequeueing app/process:
    var q = new MongoQueue(myMongoDatabase, "Notifications");
    var item = q.Pop();
    // Optionally update the queued item.
    item.Update("some status message");
    // Removes the item from the queue.
    item.Close();
    

# License (MIT)
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.