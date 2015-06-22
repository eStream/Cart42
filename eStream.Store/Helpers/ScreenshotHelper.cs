using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Web;
using System.Windows.Forms;

namespace Estream.Cart42.Web.Helpers
{
    public static class ScreenshotHelper
    {
        public static void GenerateScreenshot(string url, string filename)
        {
            // Skip if running in Azure
            if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"))) return;


            using (var bitmap = GenerateScreenshot(url))
            {
                bitmap.Save(filename, ImageFormat.Png);
            }
        }

        public static Bitmap GenerateScreenshot(string url)
        {
            // Load the webpage into a WebBrowser control
            var wb = new CustomBrowser();

            return wb.GetWebpage(url);
        }
    }

    public class CustomBrowser
    {
        protected string _url;
        private Bitmap bitmap;

        public Bitmap GetWebpage(string url)
        {
            _url = url;
            // WebBrowser is an ActiveX control that must be run in a
            // single-threaded apartment so create a thread to create the
            // control and generate the thumbnail
            var thread = new Thread(GetWebPageWorker);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            return bitmap;
        }

        protected void GetWebPageWorker()
        {
            using (var browser = new WebBrowser())
            {
                browser.ScrollBarsEnabled = false;
                browser.ScriptErrorsSuppressed = true;
                browser.Navigate(_url);

                // Wait for control to load page
                while (browser.ReadyState != WebBrowserReadyState.Complete)
                    Application.DoEvents();
                browser.ClientSize = new Size(1280, 1024);
                Thread.Sleep(1000);

                bitmap = new Bitmap(1280, 1024);
                browser.DrawToBitmap(bitmap, new Rectangle(0, 0, browser.Width, browser.Height));
                browser.Dispose();
            }
        }
    }
}