using Amazon.DynamoDBv2.DataModel;
using System;
using System.ComponentModel.DataAnnotations;

namespace EasyDynamo.Tests.Fakes
{
    public class FakeEntity
    {
        public const string IndexName = "GSI_FakeEntity_Title_LastModified";

        public int Id { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey(IndexName)]
        public string Title { get; set; }
        
        public string Content { get; set; }

        [DynamoDBGlobalSecondaryIndexRangeKey(IndexName)]
        public DateTime LastModified { get; set; }

        public string IgnoreMe { get; set; }

        [Required]
        public string PropertyWithRequiredAttribute { get; set; }
    }
}
