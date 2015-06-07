using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Estream.Cart42.Web.Domain
{
    public class Upload
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public int? ProductId { get; set; }

        [DefaultValue(UploadType.ProductImage)]
        public UploadType Type { get; set; }

        [DefaultValue(1)]
        public int SortOrder { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        public virtual ICollection<ProductSku> Skus { get; set; }
    }

    public enum UploadType
    {
        ProductImage = 1,
        TemplateImage = 2
    }
}