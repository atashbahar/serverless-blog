using System;
using Amazon.DynamoDBv2.DataModel;

namespace ServerlessBlog.Application
{
    [DynamoDBTable("serverless-blog-posts")]
    public class Post
    {
        [DynamoDBHashKey]
        public string PostId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime? CreateDate { get; set; }

        public string CreateDateString => CreateDate?.ToString("dd MMM yyyy") ?? "";
    }
}
