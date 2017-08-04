using System;
using System.Collections.Generic;

namespace EntityFramework.MappingAPI.Test.CodeFirst.Domain
{
    public class Page
    {
        public int PageId { get; set; }

        public string Content { get; set; }

        public string Title { get; set; }

        public int? ParentId { get; set; }

        public virtual Page Parent { get; set; }

        public virtual ICollection<PageTranslations> Translations { get; set; } 

        public Guid CreatedById { get; set; }
        public virtual TestUser CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        public Guid? ModifiedById { get; set; }
        public virtual TestUser ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}