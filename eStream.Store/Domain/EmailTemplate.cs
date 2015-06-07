using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;

namespace Estream.Cart42.Web.Domain
{
    public class EmailTemplate
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Type")]
        public EmailTemplateType Type { get; set; }

        [MaxLength(500)]
        [Display(Name = "Subject")]
        public string Subject { get; set; }

        [DataType(DataType.MultilineText)]
        [UIHint("RichTextEditor"), AllowHtml]
        [Display(Name = "Body")]
        public string Body { get; set; }
    }

    public enum EmailTemplateType
    {
        [Display(Name = "Order Completed")] OrderCompleted = 1
    }

    public class EmailTemplateValidator : AbstractValidator<EmailTemplate>
    {
        public EmailTemplateValidator()
        {
            RuleFor(c => c.Subject).NotEmpty();
        }
    }
}