using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Estream.Cart42.Web.Domain;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;

namespace Estream.Cart42.Web.DAL
{
    public class DataInitializer : DropCreateDatabaseIfModelChanges<DataContext>
    {
        protected override void Seed(DataContext context)
        {
            #region Delete old photos

            foreach (var file in Directory.GetFiles(HostingEnvironment.MapPath("~/Storage/"), "*.*"))
            {
                if (file.Contains("web.config")) continue;
                File.Delete(file);
            }

            #endregion

            #region Import countries and states

            string countryJson =
                File.ReadAllText(HostingEnvironment.MapPath("~/Content/DataInitializer/Regional/Countries.json"));
            dynamic json = JsonConvert.DeserializeObject(countryJson);

            foreach (dynamic item in json)
            {
                var country = new Country {Code = item.code, Name = item.name};
                country.IsActive = country.Code == "US" || country.Code == "BG";
                context.Countries.Add(country);

                if (item.filename != null)
                {
                    dynamic regionJson =
                        File.ReadAllText(
                            HostingEnvironment.MapPath("~/Content/DataInitializer/Regional/" + item.filename + ".json"));
                    dynamic json2 = JsonConvert.DeserializeObject(regionJson);
                    foreach (dynamic item2 in json2)
                    {
                        var region = new Region {Country = country, Code = item2.code, Name = item2.name};
                        context.Regions.Add(region);
                    }
                }
            }

            try
            {
                context.SaveChanges();
            }
            catch (Exception err)
            {
                throw err;
            }

            #endregion

            #region Import taxes

            string vatJson =
                File.ReadAllText(HostingEnvironment.MapPath("~/Content/DataInitializer/TaxRates/vat.json"));
            dynamic vats = JsonConvert.DeserializeObject(vatJson);

            foreach (dynamic vat in vats)
            {
                var countryCode = (string) vat.countryCode;
                Country country = context.Countries.First(c => c.Code == countryCode);
                var taxZone = new TaxZone
                              {
                                  Name = country.Name,
                                  IsActive = true,
                                  Countries = new Collection<Country> {country}
                              };

                var taxRate = new TaxRate
                              {
                                  Name = "VAT",
                                  TaxZone = taxZone,
                                  Amount = vat.percentage,
                                  IsActive = true
                              };

                context.TaxZones.Add(taxZone);
                context.TaxRates.Add(taxRate);
            }

            var calTaxZone = new TaxZone
                             {
                                 Name = "California Tax",
                                 Countries = new Collection<Country> {context.Countries.First(c => c.Code == "US")},
                                 Regions = new Collection<Region> {context.Regions.First(r => r.Name == "California")},
                                 IsActive = true
                             };
            var calTaxRate = new TaxRate
                             {
                                 Name = "Sales Tax",
                                 TaxZone = calTaxZone,
                                 Amount = 7.5m,
                                 IsActive = true
                             };

            context.TaxZones.Add(calTaxZone);
            context.TaxRates.Add(calTaxRate);

            context.SaveChanges();

            #endregion

            #region Create shipping options

            var defShipZone = new ShippingZone
                              {
                                  Name = "Default",
                                  IsActive = true
                              };
            context.ShippingZones.Add(defShipZone);

            var worldWideShippingUPS = new ShippingMethod
                                       {
                                           ShippingZone = defShipZone,
                                           Name = "UPS Worldwide",
                                           Type = ShippingMethodType.Flat,
                                           FlatRateAmount = 500
                                       };
            context.ShippingMethods.Add(worldWideShippingUPS);

            var bgShipZone = new ShippingZone
                             {
                                 Name = "Bulgaria",
                                 Countries = new Collection<Country> {context.Countries.First(c => c.Code == "BG")},
                                 IsActive = true
                             };
            context.ShippingZones.Add(bgShipZone);

            var freeBgShipping = new ShippingMethod
                                 {
                                     ShippingZone = bgShipZone,
                                     Name = "Free shipping",
                                     Type = ShippingMethodType.Free,
                                     FreeShippingMinTotal = 2000
                                 };
            var econtShipping = new ShippingMethod
                                {
                                    ShippingZone = bgShipZone,
                                    Name = "Econt",
                                    Type = ShippingMethodType.Flat,
                                    FlatRateAmount = 10
                                };
            context.ShippingMethods.Add(econtShipping);
            context.ShippingMethods.Add(freeBgShipping);

            context.SaveChanges();

            #endregion

            #region Payment methods

            var paymentMethods = new List<PaymentMethod>
                                 {
                                     new PaymentMethod
                                     {
                                         Name = "Bank Deposit",
                                         Settings =
                                             "Bank Name: ACME Bank\nBank Branch: New York\nAccount Name: John Smith\nAccount Number: XXXXXXXXXXX\n\nType any special instructions in here.",
                                         Type = PaymentMethodType.Manual,
                                         IsActive = true
                                     },
                                     new PaymentMethod
                                     {
                                         Name = "Cash on Delivery",
                                         Type = PaymentMethodType.Manual,
                                         IsActive = true
                                     },
                                     new PaymentMethod
                                     {
                                         Name = "Check",
                                         Countries =
                                             new Collection<Country>(
                                             context.Countries.Where(c => c.Code == "US").ToArray()),
                                         Type = PaymentMethodType.Manual,
                                         IsActive = true
                                     },
                                     new PaymentMethod
                                     {
                                         Name = "Money Order",
                                         Type = PaymentMethodType.Manual,
                                         IsActive = true
                                     },
                                     new PaymentMethod
                                     {
                                         Name = "Pay in Store",
                                         Type = PaymentMethodType.Manual,
                                         IsActive = true
                                     },
                                     new PaymentMethod
                                     {
                                         Name = "ePay.bg",
                                         ClassName = "EPayBgButton",
                                         Countries =
                                             new Collection<Country>(
                                             context.Countries.Where(c => c.Code == "BG").ToArray()),
                                         Type = PaymentMethodType.Hosted,
                                         IsActive = true
                                     },
                                     new PaymentMethod
                                     {
                                         Name = "Paypal Website Payments (Standard)",
                                         ClassName = "PayPalStandard",
                                         Type = PaymentMethodType.Hosted,
                                         IsActive = true
                                     },
                                 };
            paymentMethods.ForEach(c => context.PaymentMethods.AddOrUpdate(c));
            context.SaveChanges();

            #endregion

            #region Create users

            var userManager = new UserManager<User>(new UserStore<User>(context));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            // Create admin role
            roleManager.Create(new IdentityRole(User.ADMIN_ROLE));
            roleManager.Create(new IdentityRole(User.CUSTOMER_ROLE));

            // Create admin user
            var admin = new User
                        {
                            FirstName = "Admin",
                            LastName = "Adminov",
                            Company = "Admin Inc",
                            PhoneNumber = "1-800-ADMIN",
                            UserName = "admin@test.com",
                            Email = "admin@test.com",
                        };
            IdentityResult result = userManager.Create(admin, "123pass");
            if (!result.Succeeded)
                throw new Exception(string.Join("\n", result.Errors));
            userManager.AddToRole(admin.Id, User.ADMIN_ROLE);

            // Create regular user
            var user = new User
                       {
                           FirstName = "Tester",
                           LastName = "Testerov",
                           Company = "Test Ltd",
                           PhoneNumber = "1-800-USER",
                           UserName = "user@test.com",
                           Email = "user@test.com"
                       };
            userManager.Create(user, "123pass");
            userManager.AddToRole(user.Id, User.CUSTOMER_ROLE);

            #endregion

            #region Create EmailTemplates

            var templateTypes = new List<EmailTemplate>
                                {
                                    new EmailTemplate
                                    {
                                        Subject = "Order Successful",
                                        Body = "Your order has been received successfully",
                                        Type = EmailTemplateType.OrderCompleted
                                    }
                                };

            templateTypes.ForEach(t => context.EmailTemplates.AddOrUpdate(t));
            context.SaveChanges();

            #endregion

            context.SaveChanges();
            
            // Temporary until better solution is added to EF Code First
            context.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('Orders', RESEED, 101)");

            base.Seed(context);
        }
    }
}