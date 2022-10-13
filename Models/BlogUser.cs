using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace blog.Models
{
    public class BlogUser : IdentityUser
    {
        [MaxLength(100)]
        public string FullName { set; get; }

        [MaxLength(255)]
        public string Address { set; get; }

        [DataType(DataType.Date)]
        public DateTime? BirthDate { set; get; }
    }
}
