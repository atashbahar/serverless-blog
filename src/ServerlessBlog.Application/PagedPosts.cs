using System.Collections.Generic;

namespace ServerlessBlog.Application
{
    public class PagedPosts
    {
        public IEnumerable<Post> Posts { get; set; }
        public string NexPageToken { get; set; }
    }
}