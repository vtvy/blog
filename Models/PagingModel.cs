namespace blog.Models
{
    public class PagingModel
    {
        public int CurPage { get; set; }
        public int PageNumber { get; set; }

        public Func<int?, string> GenerateUrl { get; set; }

    }

}