using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Web;
using Estream.Cart42.Web.Domain;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Estream.Cart42.Web.DAL
{
    public class DataContext : DbContext
    {
        public DataContext() : base("Cart42")
        {
        }

        public DataContext(string connectionString) : base(connectionString)
        {
            Database.Connection.ConnectionString = connectionString; 
        }

        public IDbSet<Country> Countries { get; set; }
        public IDbSet<Region> Regions { get; set; }
        public IDbSet<Setting> Settings { get; set; }
        public IDbSet<User> Users { get; set; }
        public IDbSet<Category> Categories { get; set; }
        public IDbSet<Product> Products { get; set; }
        public IDbSet<ProductSku> ProductSkus { get; set; }
        public IDbSet<ProductSection> ProductSections { get; set; }
        public IDbSet<OptionCategory> OptionCategories { get; set; }
        public IDbSet<Option> Options { get; set; }
        public IDbSet<Upload> Uploads { get; set; }
        public IDbSet<ContentPage> ContentPages { get; set; }
        public IDbSet<ShoppingCart> ShoppingCarts { get; set; }
        public IDbSet<ShoppingCartItem> ShoppingCartItems { get; set; }
        public IDbSet<ShippingZone> ShippingZones { get; set; }
        public IDbSet<ShippingMethod> ShippingMethods { get; set; }
        public IDbSet<TaxClass> TaxClasses { get; set; }
        public IDbSet<TaxZone> TaxZones { get; set; }
        public IDbSet<TaxRate> TaxRates { get; set; }
        public IDbSet<TaxClassRate> TaxClassRates { get; set; }
        public IDbSet<EmailTemplate> EmailTemplates { get; set; }
        public IDbSet<PaymentMethod> PaymentMethods { get; set; }
        public IDbSet<Payment> Payments { get; set; }
        public IDbSet<Address> Addresses { get; set; }
        public IDbSet<Order> Orders { get; set; }
        public IDbSet<OrderItem> OrderItems { get; set; }
        public IDbSet<Shipment> Shipments { get; set; }
        public IDbSet<ShipmentItem> ShipmentItems { get; set; }
        public IDbSet<Return> Returns { get; set; }
        public IDbSet<ReturnItem> ReturnItems { get; set; }
        public IDbSet<Translation> Translations { get; set; }
        public IDbSet<WorkProcess> WorkProcesses { get; set; }
        public IDbSet<TemplateSetting> TemplateSettings { get; set; }
        public IDbSet<Blog> Blogs { get; set; }
        public IDbSet<BlogPost> BlogPosts { get; set; }
        public IDbSet<BlogPostComment> BlogPostComments { get; set; }
        public IDbSet<Visitor> Visitors { get; set; }
        public IDbSet<NewsletterSubscription> NewsletterSubscriptions { get; set; } 

        public static DataContext Current
        {
            get
            {
                if (HttpContext.Current != null && HttpContext.Current.Items["DataContext"] != null)
                    return HttpContext.Current.Items["DataContext"] as DataContext;

                DataContext entities = Create();
                if (HttpContext.Current != null)
                    HttpContext.Current.Items["DataContext"] = entities;
                return entities;
            }
        }

        public static DataContext Create()
        {
            return new DataContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            modelBuilder.Entity<IdentityUserLogin>().HasKey<string>(l => l.UserId);
            modelBuilder.Entity<IdentityRole>().HasKey<string>(r => r.Id);
            modelBuilder.Entity<IdentityUserRole>().HasKey(r => new {r.RoleId, r.UserId});

            modelBuilder.Entity<Order>()
                .HasRequired(o => o.BillingAddress)
                .WithMany(a => a.Orders_Billing)
                .HasForeignKey(o => o.BillingAddressId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Order>()
                .HasRequired(o => o.ShippingAddress)
                .WithMany(a => a.Orders_Shipping)
                .HasForeignKey(o => o.ShippingAddressId)
                .WillCascadeOnDelete(false);
        }

        public static void DisposeCurrent()
        {
            if (HttpContext.Current.Items["DataContext"] != null)
            {
                var entities = (DataContext) HttpContext.Current.Items["DataContext"];
                entities.Dispose();
            }
        }
    }
}