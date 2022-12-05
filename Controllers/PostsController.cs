using blog.Areas.Database.Models;
using blog.Data;
using blog.Models;
using blog.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace blog.Controllers
{
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Editor)]
    public class PostsController : Controller
    {
        private readonly BlogDbContext _context;
        private readonly UserManager<BlogUser> _userManager;

        public PostsController(BlogDbContext context, UserManager<BlogUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [TempData]
        public string Status { set; get; }

        // GET: Posts
        [Route("/posts")]
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int curPage, int pagesize)
        {
            var posts = _context.Posts.Include(p => p.Author)
                                .OrderByDescending(post => post.DateUpdated);

            if (pagesize < 1) pagesize = 10;

            int postNumber = await posts.CountAsync();

            int pageNumber = (int)Math.Ceiling((double)postNumber / pagesize);

            if (curPage > pageNumber) curPage = pageNumber;
            if (curPage < 1) curPage = 1;



            PagingModel pagingModel = new()
            {
                CurPage = curPage,
                PageNumber = pageNumber,
                GenerateUrl = (p) => Url.Action("Index", new
                {
                    p,
                    pagesize
                })
            };
            ViewBag.postIndex = (curPage - 1) * pagesize;
            ViewBag.pagingModel = pagingModel;
            ViewBag.postNumber = postNumber;
            var pagingPosts = await posts.Skip((curPage - 1) * pagesize)
                    .Take(pagesize)
                    .Include(p => p.PostCategories)
                    .ThenInclude(post => post.Category)
                    .ToListAsync();

            return View(pagingPosts);
        }

        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Posts/Create
        public async Task<IActionResult> CreateAsync()
        {
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            return View();

        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published,CategoryIDs")] CreatePostModel post)
        {

            
            
            post.Slug ??= BlogUtilities.GenerateSlug(post.Title);

            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug))
            {
                ModelState.AddModelError("Slug", "Nhập chuỗi Url khác");
                return View(post);
            }

            if (ModelState.IsValid)
            {

                BlogUser user = await _userManager.GetUserAsync(this.User);
                post.DateCreated = post.DateUpdated = DateTime.Now;
                post.AuthorId = user.Id;
                _context.Add(post);

                if (post.CategoryIDs != null)
                {
                    foreach (int CategoryID in post.CategoryIDs)
                    {
                        _context.Add(new PostCategory { CategoryID = CategoryID, Post = post });
                    }
                }




                await _context.SaveChangesAsync();
                Status = "Create a blog post successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        // GET: Posts/Edit/5
        [Route("/posts/{id?}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                            .Include(post => post.PostCategories)
                            .FirstOrDefaultAsync(post => post.PostId == id);

            if (post == null)
            {
                return NotFound();
            }

            var editPost = new CreatePostModel()
            {
                PostId = post.PostId,
                Title = post.Title,
                Description = post.Description,
                Content = post.Content,
                Slug = post.Slug,
                Published = post.Published,
                CategoryIDs = post.PostCategories.Select(post => post.CategoryID).ToArray()
            };


            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");

            return View(editPost);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,Description,Slug,Content,Published,CategoryIDs")] CreatePostModel post)
        {
            if (id != post.PostId)
            {
                return NotFound();
            }

            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");

            post.Slug ??= BlogUtilities.GenerateSlug(post.Title);

            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug && post.PostId != id))
            {
                ModelState.AddModelError("Slug", "Nhập chuỗi Url khác");
                return View(post);
            }
            if (ModelState.IsValid)
            {

                var updatingPost = await _context.Posts
                        .Include(post => post.PostCategories)
                        .FirstOrDefaultAsync(post => post.PostId == id);

                if (updatingPost == null)
                {
                    return NotFound();
                }

                updatingPost.Title = post.Title;
                updatingPost.Description = post.Description;
                updatingPost.Content = post.Content;
                updatingPost.Slug = post.Slug;
                updatingPost.DateUpdated = DateTime.Now;


                post.CategoryIDs ??= Array.Empty<int>();

                var oldCateIds = updatingPost.PostCategories.Select(c => c.CategoryID).ToArray();
                var newCateIds = post.CategoryIDs;

                var removeCatePosts = from postCate in updatingPost.PostCategories
                                      where (!newCateIds.Contains(postCate.CategoryID))
                                      select postCate;
                _context.PostCategories.RemoveRange(removeCatePosts);

                var addCateIds = from CateId in newCateIds
                                 where !oldCateIds.Contains(CateId)
                                 select CateId;

                foreach (var CateId in addCateIds)
                {
                    _context.PostCategories.Add(new PostCategory()
                    {
                        PostID = id,
                        CategoryID = CateId
                    });
                }


                _context.Update(updatingPost);
                await _context.SaveChangesAsync();

                Status = "Update a post successfully!";
                return RedirectToAction(nameof(Index));
            }
            Status = "Update fail!";
            return View(post);
        }

        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Posts == null)
            {
                return Problem("Entity set 'BlogDbContext.Posts'  is null.");
            }
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }

            await _context.SaveChangesAsync();
            Status = "Success delete the post: " + post.Title;

            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.PostId == id);
        }

        [HttpPost]
        public async Task<IActionResult> UploadAsync(IFormFile file)
        {
            string imgPath;
            if (file.Length > 0)
            {
                string stroredPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "wwwroot/files"));
                if (!Directory.Exists(stroredPath))
                {
                    Directory.CreateDirectory(stroredPath);
                }
                imgPath = DateTime.Now.ToString("yyyyMMddTHHmmss") + file.FileName;
                string fullPath = Path.Combine(stroredPath, imgPath);
                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                imgPath = $@"https://localhost:7224/files/${imgPath}";
                return Ok(new { imgPath});
            }
            imgPath = "No_image_available.svg.png";
            return Ok(new { imgPath });

        }
    }
}