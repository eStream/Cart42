using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Configuration;
using System.Web.Hosting;
using CsvHelper;
using Estream.Cart42.Web.Areas.Admin.Controllers;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Glimpse.AspNet.Tab;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;

namespace Estream.Cart42.Web.Migrations
{
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<DAL.DataContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "Estream.Cart42.Web.DAL.DataContext";
        }

        protected override void Seed(DAL.DataContext context)
        {
            #region Import translations

            if (context.Translations.None())
            {
                DataImportController.ImportTranslationsCsv(context, new CsvReader(File.OpenText(
                    HostingEnvironment.MapPath("~/Content/DataInitializer/Translations/Translations.csv"))),
                    TranslationArea.Frontend);

                DataImportController.ImportTranslationsCsv(context, new CsvReader(File.OpenText(
                    HostingEnvironment.MapPath("~/Content/DataInitializer/Translations/TranslationsAdmin.csv"))),
                    TranslationArea.Backend);
            }

            Thread.CurrentThread.CurrentUICulture = new CultureInfo(
                WebConfigurationManager.AppSettings["DefaultCulture"]);

            #endregion

            #region Create user roles

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            if (!roleManager.RoleExists(User.ADMIN_ROLE))
                roleManager.Create(new IdentityRole(User.ADMIN_ROLE));

            if (!roleManager.RoleExists(User.CUSTOMER_ROLE))
                roleManager.Create(new IdentityRole(User.CUSTOMER_ROLE));

            if (!roleManager.RoleExists(User.OPERATOR_ROLE))
                roleManager.Create(new IdentityRole(User.OPERATOR_ROLE));

            #endregion

            #region Create users

            if (context.Users.None())
            {
                var userManager = new UserManager<User>(new UserStore<User>(context));

                // Create admin user
                var admin = new User
                            {
                                FirstName = "Admin",
                                LastName = "Admin",
                                Company = "Admin",
                                PhoneNumber = "1-800-ADMIN",
                                UserName = WebConfigurationManager.AppSettings["DefaultUserEmail"],
                                Email = WebConfigurationManager.AppSettings["DefaultUserEmail"],
                            };
                IdentityResult result = userManager.Create(admin, WebConfigurationManager.AppSettings["DefaultUserPassword"]);
                if (!result.Succeeded)
                    throw new Exception(string.Join("\n", result.Errors));
                userManager.AddToRole(admin.Id, User.ADMIN_ROLE);
            }

            #endregion

            #region Create EmailTemplates

            if (context.EmailTemplates.None())
            {
                var templateTypes = new List<EmailTemplate>
                                    {
                                        new EmailTemplate
                                        {
                                            Subject = "Order Successful".TA(),
                                            Body = "Your order has been received successfully".TA(),
                                            Type = EmailTemplateType.OrderCompleted
                                        }
                                    };

                templateTypes.ForEach(t => context.EmailTemplates.AddOrUpdate(t));
                context.SaveChanges();
            }

            #endregion

            #region Import countries and states

            if (context.Countries.None())
            {
                string countryJson =
                    File.ReadAllText(HostingEnvironment.MapPath("~/Content/DataInitializer/Regional/Countries.json"));
                dynamic json = JsonConvert.DeserializeObject(countryJson);

                foreach (dynamic item in json)
                {
                    var country = new Country {Code = item.code, Name = item.name, IsActive = true};
                    context.Countries.Add(country);

                    if (item.filename != null)
                    {
                        dynamic regionJson =
                            File.ReadAllText(
                                HostingEnvironment.MapPath("~/Content/DataInitializer/Regional/" + item.filename +
                                                           ".json"));
                        dynamic json2 = JsonConvert.DeserializeObject(regionJson);
                        foreach (dynamic item2 in json2)
                        {
                            var region = new Region {Country = country, Code = item2.code, Name = item2.name};
                            context.Regions.Add(region);
                        }
                    }
                }

                context.SaveChanges();
            }

            #endregion

            #region Create default shipping and tax zones

            if (context.TaxZones.None())
            {
                var taxZone = new TaxZone {Name = "Default".TA(), IsActive = true};
                context.TaxZones.Add(taxZone);
            }

            if (context.ShippingZones.None())
            {
                var shippingZone = new ShippingZone { Name = "Default".TA(), IsActive = true };
                context.ShippingZones.Add(shippingZone);
            }

            #endregion

            #region Create default content pages

            if (context.ContentPages.None())
            {
                var page = new ContentPage
                                {
                                    Title = "About Us".TA(),
                                    Content =
                                        "Edit this page contents from the \"Edit Content Pages\" section in the admin tool".TA(),
                                    HeaderPosition = 1,
                                    FooterPosition = 1
                                };
                context.ContentPages.Add(page);
                page = new ContentPage
                {
                    Title = "Contact Us".TA(),
                    Content =
                        "Edit this page contents form the \"Edit Content Pages\" section in the admin tool".TA(),
                    HeaderPosition = 2,
                    FooterPosition = 2
                };
                context.ContentPages.Add(page);
                page = new ContentPage
                {
                    Title = "Terms & Conditions".TA(),
                    Content =
                        "Edit this page contents form the \"Edit Content Pages\" section in the admin tool".TA(),
                    HeaderPosition = 3,
                    FooterPosition = 3
                };
                context.ContentPages.Add(page);
                page = new ContentPage
                {
                    Title = "Privacy Policy".TA(),
                    Content =
                        "Edit this page contents form the \"Edit Content Pages\" section in the admin tool".TA(),
                    HeaderPosition = 4,
                    FooterPosition = 4
                };
                context.ContentPages.Add(page);
                page = new ContentPage
                {
                    Title = "Orders and Returns".TA(),
                    Content =
                        "Edit this page contents form the \"Edit Content Pages\" section in the admin tool".TA(),
                    HeaderPosition = 5,
                    FooterPosition = 5
                };
                context.ContentPages.Add(page);
                context.SaveChanges();
            }

            #endregion

            #region Create default news blog

            if (context.Blogs.None())
            {
                User admin = context.Users.OrderByDescending(u => u.DateRegistered).FirstOrDefault();

                var blog = new Blog
                           {
                               Title = "News".T()
                           };
                context.Blogs.Add(blog);

                if (admin != null)
                {
                    var blogPost = new BlogPost
                                   {
                                       UserId = admin.Id,
                                       Blog = blog,
                                       Title = "Store created".T(),
                                       Content = "You can use the blog functionality to post updates for your users...".T()

                                   };
                    context.BlogPosts.Add(blogPost);
                }

                context.SaveChanges();
            }

            #endregion

            #region Payment methods

            if (context.PaymentMethods.None())
            {
                var paymentMethods = new List<PaymentMethod>
                                     {
                                         new PaymentMethod
                                         {
                                             Name = "Bank Deposit".TA(),
                                             Settings =
                                                 "Bank Name: ACME Bank\nBank Branch: New York\nAccount Name: John Smith\nAccount Number: XXXXXXXXXXX\n\nType any special instructions in here.",
                                             Type = PaymentMethodType.Manual,
                                             IsActive = true
                                         },
                                         new PaymentMethod
                                         {
                                             Name = "Cash on Delivery".TA(),
                                             Type = PaymentMethodType.Manual,
                                             IsActive = true
                                         },
                                         new PaymentMethod
                                         {
                                             Name = "Check".TA(),
                                             Countries =
                                                 new Collection<Country>(
                                                 context.Countries.Where(c => c.Code == "US").ToArray()),
                                             Type = PaymentMethodType.Manual,
                                             IsActive = true
                                         },
                                         new PaymentMethod
                                         {
                                             Name = "Money Order".TA(),
                                             Type = PaymentMethodType.Manual,
                                             IsActive = true
                                         },
                                         new PaymentMethod
                                         {
                                             Name = "Pay in Store".TA(),
                                             Type = PaymentMethodType.Manual,
                                             IsActive = true
                                         },
                                         new PaymentMethod
                                         {
                                             Name = "ePay.bg".TA(),
                                             ClassName = "EPayBgButton",
                                             Countries =
                                                 new Collection<Country>(
                                                 context.Countries.Where(c => c.Code == "BG").ToArray()),
                                             Type = PaymentMethodType.Hosted,
                                             IsActive = true
                                         },
                                         new PaymentMethod
                                         {
                                             Name = "Paypal Website Payments (Standard)".TA(),
                                             ClassName = "PayPalStandard",
                                             Type = PaymentMethodType.Hosted,
                                             IsActive = true
                                         },
                                     };
                paymentMethods.ForEach(c => context.PaymentMethods.AddOrUpdate(c));
                context.SaveChanges();
            }

            #endregion
        }
    }
}