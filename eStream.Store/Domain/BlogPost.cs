using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using AutoMapper;

namespace Estream.Cart42.Web.Domain
{
    public class BlogPost
    {
        public BlogPost()
        {
            PublishDate = DateTime.Now;
        }

        [Key]
        public int Id { get; set; }

        public int BlogId { get; set; }

        public string UserId { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public bool AllowComments { get; set; }

        public DateTime PublishDate { get; set; }

        [ForeignKey("BlogId")]
        public virtual Blog Blog { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [IgnoreMap]
        public virtual ICollection<BlogPostComment> Comments{ get; set; }
    }
}