using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using ServerlessBlog.Application;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace ServerlessBlog.Processor
{
    public class ContentProcessor
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IBlogDataProvider _dataProvider;

        public ContentProcessor(IAmazonS3 s3Client, IBlogDataProvider dataProvider)
        {
            _s3Client = s3Client;
            _dataProvider = dataProvider;
        }

        public ContentProcessor()
        {
            _s3Client = new AmazonS3Client();
            _dataProvider = new BlogDataProvider();
        }

        public async Task Process(S3Event s3Event, ILambdaContext context)
        {
            if (s3Event.Records == null || s3Event.Records.Count == 0)
            {
                context.Logger.LogLine("event records is empty");
                return;
            }

            var eventRecord = s3Event.Records[0];
            var s3Entity = eventRecord.S3;

            if(s3Entity == null)
            {
                context.Logger.LogLine("S3 entity for first event record is null");
                return;
            }

            var eventName = eventRecord.EventName;
            var s3ObjectKey = s3Entity.Object.Key;
            var postId = Path.GetFileNameWithoutExtension(s3ObjectKey);
            context.Logger.LogLine($"event name: {eventName}, key: {s3ObjectKey}");

            try
            {
                if (eventName == EventType.ObjectRemovedDelete)
                {
                    await _dataProvider.DeletePostAsync(postId);
                    context.Logger.LogLine($"removed article {postId}");
                }
                else if (eventName == EventType.ObjectCreatedPost || eventName == EventType.ObjectCreatedPut)
                {
                    var response = await ReadObjectDataAsync(s3Entity.Bucket.Name, s3ObjectKey);

                    var post = await MarkdownProcessor.GetPost(response);
                    post.PostId = postId;

                    await _dataProvider.SavePostAsync(post);

                    context.Logger.LogLine("saved article to dynamoDB");
                }
                else
                {
                    throw new Exception($"{eventName} cannot be processed");
                }
            }
            catch (Exception e)
            {
                context.Logger.LogLine($"ERROR: {e.Message}");
                context.Logger.LogLine(e.StackTrace);
                throw;
            }
        }

        private async Task<string> ReadObjectDataAsync(string bucketName, string key)
        {
            using var response = await _s3Client.GetObjectAsync(bucketName, key);
            await using var responseStream = response.ResponseStream;
            using var reader = new StreamReader(responseStream);
            return await reader.ReadToEndAsync();
        }
    }
}
