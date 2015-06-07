using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Estream.Cart42.Web.Domain
{
    public class BlogPostComment
    {
        [Key]
        public int Id { get; set; }

        public int BlogPostId { get; set; }

        public string UserId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Message { get; set; }

        public bool IsApproved { get; set; }

        public DateTime DatePosted { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("BlogPostId")]
        public virtual BlogPost BlogPost { get; set; }
    }
}