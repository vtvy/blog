using blog.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;

namespace blog.Areas.Identity.Models.UserViewModels
{
    public class AddUserRoleModel
    {
        public BlogUser User { get; set; }

        [DisplayName("Các role gán cho user")]
        public string[] RoleNames { get; set; }

        public List<IdentityRoleClaim<string>> ClaimsInRole { get; set; }
        public List<IdentityUserClaim<string>> ClaimsInUserClaim { get; set; }

    }
}