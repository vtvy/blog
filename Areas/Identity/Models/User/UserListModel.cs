using blog.Models;

namespace blog.Areas.Identity.Models.UserViewModels
{
    public class UserListModel
    {
        public int totalUsers { get; set; }
        public int countPages { get; set; }

        public int ITEMS_PER_PAGE { get; set; } = 10;

        public int currentPage { get; set; }

        public List<UserAndRole> users { get; set; }

    }

    public class UserAndRole : BlogUser
    {
        public string RoleNames { get; set; }
    }


}