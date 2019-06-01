using System;

namespace EasyDynamo.Tests.Fakes
{
    public class FakeEntity
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public DateTime LastModified { get; set; }

        public string IgnoreMe { get; set; }
    }
}
