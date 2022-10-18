using System.ComponentModel.DataAnnotations;

namespace blog.Models
{
    public class CreatePostModel : Post
    {
        [Display(Name = "Categories")]
        public int[] CategoryIDs { get; set; }
    }
}