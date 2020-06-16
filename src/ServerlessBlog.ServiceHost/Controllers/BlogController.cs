using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServerlessBlog.Application;
using ServerlessBlog.ServiceHost.Models;

namespace ServerlessBlog.ServiceHost.Controllers
{
    public class BlogController : Controller
    {
        private readonly IBlogDataProvider _db;

        public BlogController(IBlogDataProvider db)
        {
            _db = db;
        }

        public async Task<ActionResult> Posts(int page = 1, int pageSize = 5)
        {
            var allPostIds = await _db.GetSortedPostIds();

            var postIds = allPostIds
                .Skip((page - 1)*pageSize)
                .Take(pageSize)
                .ToList();

            if (page > 1 && !postIds.Any())
                return RedirectToAction("Posts");

            var posts = await _db.GetPostsById(postIds);

            var model = new PagedList<Post>
            {
                Items = posts.OrderByDescending(x => x.PostId),
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = allPostIds.Count()
            };

            return View(model);
        }

        public async Task<ActionResult> Post(string id)
        {
            var post = await _db.GetPost(id);

            if (post == null)
                return RedirectToAction("PageNotFound", "Blog");

            return View(post);
        }

        public ActionResult PageNotFound()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public ActionResult Error()
        {
            return View();
        }
    }
}
