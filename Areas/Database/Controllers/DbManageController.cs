using blog.Areas.Database.Models;
using blog.Data;
using blog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace blog.Areas.Database.Controllers
{
    [Area("Database"), Route("/database/[action]")]
    public class DbManageController : Controller
    {
        private readonly BlogDbContext dbContext;
        private readonly UserManager<BlogUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public DbManageController(BlogDbContext dbContext, UserManager<BlogUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult DelDb()
        {
            return View();
        }

        [TempData]
        public string Status { get; set; }
        [HttpPost]
        public async Task<IActionResult> DelDbAsync()
        {
            var success = await dbContext.Database.EnsureDeletedAsync();
            Status = success ? "Delete Database successfully" : "Delete Database fail";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> CreateDb()
        {
            await dbContext.Database.MigrateAsync();
            Status = "Create Database successfully";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> SeedDataAsync()
        {
            var roleNames = typeof(RoleName).GetFields().ToList();
            foreach (var roleName in roleNames)
            {
                var aRoleName = (string)roleName.GetRawConstantValue();
                var isFound = await roleManager.FindByNameAsync(aRoleName);
                if (isFound == null)
                {
                    await roleManager.CreateAsync(new IdentityRole(aRoleName));
                }
            }
            var adminUser = await userManager.FindByEmailAsync("admin");
            if (adminUser == null)
            {
                adminUser = new BlogUser()
                {
                    UserName = "admin",
                    Email = "vyb1910730@student.ctu.edu.vn",
                    EmailConfirmed = true,
                };

                await userManager.CreateAsync(adminUser, "b1910730");
                await userManager.AddToRoleAsync(adminUser, RoleName.Administrator);
            }
            Status = "Having seed Database";
            return RedirectToAction("Index");
        }

    }
}
