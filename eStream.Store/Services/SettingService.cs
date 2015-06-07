using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using WebGrease.Css.Extensions;

namespace Estream.Cart42.Web.Services
{
    public class SettingService : ISettingService
    {
        private readonly DataContext db;

        private static readonly ConcurrentDictionary<string, object> _settingsCache =
            new ConcurrentDictionary<string, object>();

        public SettingService(DataContext db)
        {
            this.db = db;

            if (_settingsCache.IsEmpty)
            {
                lock (_settingsCache)
                {
                    if (_settingsCache.IsEmpty)
                    {
                        //TODO: Expand json strings to objects
                        db.Settings.ToArray().ForEach(
                            s => _settingsCache.TryAdd(s.Key, s.Value));
                    }
                }
            }
        }

        public T Get<T>(SettingField key)
        {
            object value;
            _settingsCache.TryGetValue(key.ToString(), out value);
            if (value == null || (value is string && (string) value == string.Empty))
                return (T)key.GetAttributeOfType<DefaultValueAttribute>().Value;
            return (T)Convert.ChangeType(value, Type.GetTypeCode(typeof(T)));
        }

        public void Set<T>(SettingField key, T value)
        {
            Setting setting = db.Settings.Find(key.ToString());
            if (setting == null)
            {
                setting = new Setting { Key = key.ToString() };
                db.Settings.Add(setting);
            }

            //TODO: Check for custom types and serialize if necessary
            setting.Value = Convert.ToString(value);
            db.SaveChanges();

            // Update local cache
            _settingsCache.AddOrUpdate(key.ToString(), value, (oldKey, oldvalue) => value);
        }
    }
}

public enum SettingField
{
    #region General

    [Category("General")]
    [Name("Store name")]
    [Description("Enter your store name")]
    [Type(typeof(string))]
    [DefaultValue("My Store")]
    StoreName,

    [Category("General")]
    [Name("Down for maintenance")]
    [Description("Use this option to take the store offline")]
    [Type(typeof(bool))]
    [DefaultValue(false)]
    DownForMaintenance,

    [Category("General")]
    [Name("Catalog Only")]
    [Description("Use this option to disable the accounts and shopping cart funcitonality")]
    [Type(typeof(bool))]
    [DefaultValue(false)]
    CatalogOnly,

    #endregion

    #region Frontend

    [Category("Frontend")]
    [Name("Products per page")]
    [Description("Specify how many products should be displayed per page by default")]
    [Type(typeof(int))]
    [DefaultValue(24)]
    ProductsPerPage,

    #endregion

    #region Currency

    [Category("Currency")]
    [Name("Currency code")]
    [Description("Specify currency code as per ISO 4127")]
    [Type(typeof(string))]
    [DefaultValue("EUR")]
    CurrencyCode,

    [Category("Currency")]
    [Name("Currency prefix")]
    [Description("Specify currency prefix symbol")]
    [Type(typeof(string))]
    [DefaultValue("")]
    CurrencyPrefix,

    [Category("Currency")]
    [Name("Currency suffix")]
    [Description("Specify currency suffix symbol")]
    [Type(typeof(string))]
    [DefaultValue("€")]
    CurrencySuffix,

    #endregion

    #region Tax

    [Category("Tax")]
    [Name("Tax Label")]
    [Description(
"Enter a general name that describes the type of tax applied to orders on your store. This will be shown throughout your store when prices are set to be shown as including and excluding tax, or when taxes on orders are shown as one summarized line item.<br /><br />Example: Tax, Sales Tax, VAT or GST."
)]
    [Type(typeof(string))]
    [DefaultValue("Tax")]
    TaxLabel,

    [Category("Tax")]
    [Name("Prices inclusive of tax")]
    [Description(
"Specify whether you will enter prices inclusive of tax or tax will be added separately to the item total")]
    [Type(typeof(bool))]
    [DefaultValue(true)]
    TaxIncludedInPrices,

    #endregion

    #region Regional

    [Category("Regional")]
    [Name("Use metric system")]
    [Description("Specify whether you will use the metric system (km, kg) or the imperial one (miles, lbs)")]
    [Type(typeof(bool))]
    [DefaultValue(true)]
    UseMetric,

    #endregion

    #region Email

    [Category("Email")]
    [Name("Default administrator email")]
    [Description("Specify the defailt administrator email address")]
    [Type(typeof(string))]
    [DefaultValue("admin@company.com")]
    AdministratorEmail,

    [Category("Email")]
    [Name("Order invoices CC address")]
    [Description("Specify CC address for the order invoices")]
    [Type(typeof(string))]
    [DefaultValue("orders@company.com")]
    OrderInvoicesCCEmail,

    [Category("Email")]
    [Name("Email sender address")]
    [Description("Specify sender address in the format \"Sender Name <name@company.com>\"")]
    [Type(typeof(string))]
    [DefaultValue("Your Name <noreply@company.com>")]
    EmailSenderName,

    /*

    [Category("Email")]
    [Name("SMTP server address")]
    [Description("Specify smtp server address (e.g. smtp.company.com)")]
    [Type(typeof(string))]
    [DefaultValue("smtp.company.com")]
    SmtpServerAddress,

    [Category("Email")]
    [Name("SMTP server port")]
    [Description("Specify smtp server port (default is 25)")]
    [Type(typeof(int))]
    [DefaultValue(25)]
    SmtpServerPort,

    [Category("Email")]
    [Name("SMTP server uses SSL")]
    [Description("Specify if smtp server uses SSL")]
    [Type(typeof(bool))]
    [DefaultValue(false)]
    SmtpServerSSL,

    [Category("Email")]
    [Name("SMTP username")]
    [Description("Specify smtp authentication username")]
    [Type(typeof(string))]
    [DefaultValue("username")]
    SmtpServerUsername,

    [Category("Email")]
    [Name("SMTP password")]
    [Description("Specify smtp authentication password")]
    [Type(typeof(string))]
    [DefaultValue("password")]
    SmtpServerPassword,

     */

    #endregion

    #region Hidden

    [Type(typeof(bool))]
    [DefaultValue(false)]
    IsInitialized,

    [Type(typeof(string))]
    [DefaultValue("Standard")]
    Theme,

    [Type(typeof(bool))]
    [DefaultValue(true)]
    ShowWelcomePage,

    [Type(typeof(bool))]
    [DefaultValue(true)]
    ShowCategoryTutorial,

    [Type(typeof(bool))]
    [DefaultValue(true)]
    ShowOptionTutorial,

    [Type(typeof(bool))]
    [DefaultValue(true)]
    ShowProductTutorial,

    [Type(typeof(bool))]
    [DefaultValue(true)]
    ShowTaxRateTutorial,

    [Type(typeof(bool))]
    [DefaultValue(true)]
    ShowShippingRateTutorial,

    #endregion
}

public class TypeAttribute : Attribute
{
    private readonly Type type;

    public TypeAttribute(Type type)
    {
        this.type = type;
    }

    public Type Type
    {
        get { return type; }
    }
}

public class NameAttribute : Attribute
{
    private readonly string name;

    public NameAttribute(string name)
    {
        this.name = name;
    }

    public string Name
    {
        get { return name; }
    }
}