using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;
using System.Web.Optimization;
using BundleTransformer.Core.Builders;
using BundleTransformer.Core.Bundles;
using BundleTransformer.Core.Orderers;
using BundleTransformer.Core.Resolvers;
using Glimpse.AspNet.Tab;
using Glimpse.Core.Tab.Assist;

namespace Estream.Cart42.Web
{
    public class BundleConfig
    {
        public static BundleCollection _bundles;

        public static void RegisterBundles(BundleCollection bundles)
        {
            _bundles = bundles;
            bundles.UseCdn = true;

            var nullBuilder = new NullBuilder();
            var nullOrderer = new NullOrderer();

            // Replace a default bundle resolver in order to the debugging HTTP-handler
            // can use transformations of the corresponding bundle
            BundleResolver.Current = new CustomBundleResolver();

            var commonStylesBundle = new CustomStyleBundle("~/Bundles/CommonStyles");
            commonStylesBundle.Include(
                "~/Content/bootstrap.css",
                "~/Content/font-awesome.css");
            commonStylesBundle.Orderer = nullOrderer;
            bundles.Add(commonStylesBundle);
            
            var themeFolders = Directory.GetDirectories(HostingEnvironment.MapPath("~/Content/Themes"));
            foreach (var folder in themeFolders)
            {
                try
                {
                    var themeName = folder.Substring(folder.LastIndexOf(@"\", System.StringComparison.Ordinal) + 1);

                    var stylesBundle = new CustomStyleBundle(string.Format("~/Bundles/Themes/{0}/Styles", themeName));
                    stylesBundle.Include(
                        string.Format("~/Content/Themes/{0}/css/*.css", themeName));
                    //string.Format("~/Content/Themes/{0}/css/*.less", themeName));
                    stylesBundle.Orderer = nullOrderer;
                    bundles.Add(stylesBundle);

                    var scriptsBundle = new CustomScriptBundle(string.Format("~/Bundles/Themes/{0}/Scripts", themeName));
                    scriptsBundle.Include(string.Format("~/Content/Themes/{0}/js/*.js", themeName));
                    scriptsBundle.Orderer = nullOrderer;
                    bundles.Add(scriptsBundle);
                }
                catch (ArgumentException err)
                {
                    if (err.ParamName != "directoryVirtualPath") throw;
                }
            }

            var adminStylesBundle = new CustomStyleBundle("~/Bundles/AdminStyles");
            adminStylesBundle.Include(
                "~/Content/admin/css/animate.css",
                "~/Content/admin/css/style.css",
                "~/Content/admin/css/*.css");
            adminStylesBundle.Orderer = nullOrderer;
            bundles.Add(adminStylesBundle);

            var modernizrBundle = new CustomScriptBundle("~/Bundles/Modernizr");
            modernizrBundle.Include("~/Scripts/modernizr-2.*");
            modernizrBundle.Orderer = nullOrderer;
            bundles.Add(modernizrBundle);

            var paceBundle = new CustomScriptBundle("~/Bundles/Pace");
            paceBundle.Include("~/Content/admin/js/pace.min.js");
            paceBundle.Orderer = nullOrderer;
            bundles.Add(paceBundle);

            var jQueryBundle = new CustomScriptBundle("~/Bundles/Jquery",
                "http://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.11.1.min.js");
            jQueryBundle.Include("~/Scripts/jquery-{version}.js");
            jQueryBundle.Orderer = nullOrderer;
            jQueryBundle.CdnFallbackExpression = "window.jquery";
            bundles.Add(jQueryBundle);

            var commonScriptsBundle = new CustomScriptBundle("~/Bundles/CommonScripts");
            commonScriptsBundle.Include(
                "~/Scripts/jquery.validate.js",
                "~/Scripts/jquery.validate.unobtrusive.js",
                "~/Scripts/jquery.unobtrusive-ajax.js",
                "~/Scripts/knockout-3.*",
                "~/Scripts/knockout.mapping-*",
                "~/Scripts/bootstrap.js",
                "~/Scripts/bootbox.js",
                "~/Scripts/alerts.js",
                "~/Scripts/misc.js");

            commonScriptsBundle.Orderer = nullOrderer;
            bundles.Add(commonScriptsBundle);

            var adminScriptBundle = new CustomScriptBundle("~/Bundles/AdminScripts");
            adminScriptBundle.Include(
                "~/Content/admin/js/style.js",
                "~/Content/admin/js/misc.js",
                "~/Content/admin/js/jquery.metisMenu.js",
                "~/Content/admin/js/summernote.min.js",
                "~/Content/admin/js/intro.min.js",
                "~/Scripts/jquery.treegrid.js",
                "~/Scripts/moment-with-locales.js");
            adminScriptBundle.Orderer = nullOrderer;
            bundles.Add(adminScriptBundle);

            var oldBrowsersScriptBundle = new CustomScriptBundle("~/Bundles/OldBrowsers");
            oldBrowsersScriptBundle.Include("~/Scripts/respond.js");
            oldBrowsersScriptBundle.Orderer = nullOrderer;
            bundles.Add(oldBrowsersScriptBundle);

            BundleTable.EnableOptimizations = false;
        }
    }

    public class AsDefinedBundleOrderer : IBundleOrderer
    {
        public IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files)
        {
            return files;
        }
    }
}