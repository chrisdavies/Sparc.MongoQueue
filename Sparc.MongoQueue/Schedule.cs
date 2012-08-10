namespace Sparc.MongoQueue
{
    using System;

    public class Schedule
    {
        public Repeat Repeat { get; set; }

        public DateTime NextRun { get; set; }
    }
}