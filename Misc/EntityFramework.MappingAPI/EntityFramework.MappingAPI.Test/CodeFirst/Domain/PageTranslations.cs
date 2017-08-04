
namespace EntityFramework.MappingAPI.Test.CodeFirst.Domain
{
    public class PageTranslations
    {
        public int PageId { get; set; }

        public virtual Page Page { get; set; }

        public string Language { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }
    }
}