using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using System.Collections.Generic;
using System.Linq;

namespace ServerlessBlog.Application
{
    public interface IBlogDataProvider
    {
        Task<IEnumerable<Post>> GetAllPosts();
        Task<IEnumerable<Post>> GetPostsById(IEnumerable<string> keys);
        Task<IEnumerable<string>> GetSortedPostIds();
        Task<Post> GetPost(string id);
        Task SavePostAsync(Post post);
        Task DeletePostAsync(string postId);
    }

    public class BlogDataProvider : IBlogDataProvider
    {
        private readonly IDynamoDBContext _dynamoDbContext;

        public BlogDataProvider()
        {
            var dynamoDbClient = new AmazonDynamoDBClient(RegionEndpoint.USEast1); // TODO: don't hard code region
            _dynamoDbContext = new DynamoDBContext(dynamoDbClient);
        }

        public async Task<IEnumerable<Post>> GetAllPosts()
        {
            var table = _dynamoDbContext.GetTargetTable<Post>();
            var scan = table.Scan(new ScanOperationConfig
            {
                IndexName = "All-Posts-Index",
                AttributesToGet = new List<string> {"PostId", "Title"},
                Select = SelectValues.SpecificAttributes
            });
            var result = await scan.GetNextSetAsync();
            return _dynamoDbContext.FromDocuments<Post>(result);
        }

        public async Task<IEnumerable<string>> GetSortedPostIds()
        {
            var table = _dynamoDbContext.GetTargetTable<Post>();
            var scan = table.Scan(new ScanOperationConfig
            {
                AttributesToGet = new List<string> {"PostId", "CreateDate"},
                Select = SelectValues.SpecificAttributes
            });
            var result = await scan.GetRemainingAsync();
            return _dynamoDbContext
                .FromDocuments<Post>(result)
                .OrderByDescending(x => x.CreateDate)
                .Select(x => x.PostId);
        }

        public async Task<IEnumerable<Post>> GetPostsById(IEnumerable<string> keys)
        {
            var batchGet = _dynamoDbContext.CreateBatchGet<Post>();
            foreach (var key in keys)
            {
                batchGet.AddKey(key);
            }

            await batchGet.ExecuteAsync();
            return batchGet.Results;
        }

        public Task<Post> GetPost(string id)
        {
            return _dynamoDbContext.LoadAsync<Post>(id);
        }

        public Task SavePostAsync(Post post)
        {
            return _dynamoDbContext.SaveAsync(post);
        }

        public Task DeletePostAsync(string postId)
        {
            return _dynamoDbContext.DeleteAsync<Post>(postId);
        }
    }
}
