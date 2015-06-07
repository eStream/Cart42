using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using AutoMapper;

namespace Estream.Cart42.Web.Domain
{
    public class Blog
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; }

        public bool ApproveComments { get; set; }

        [IgnoreMap]
        public virtual ICollection<BlogPost> BlogPosts { get; set; }
    }
}