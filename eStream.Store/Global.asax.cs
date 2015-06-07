using System;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Elmah;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.DependencyResolution;
using Estream.Cart42.Web.DependencyResolution.ModelMetadata;
using Estream.Cart42.Web.DependencyResolution.Tasks;
using Estream.Cart42.Web.Engines;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Services;
using FluentValidation;
using FluentValidation.Mvc;
using RazorEngine.Templating;
using StructureMap;
using StructureMap.Web.Pipeline;

namespace Estream.Cart42.Web
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ModelBinders.Binders.DefaultBinder = new CustomModelBinder();
            IoC.Init();
            ValidatorConfig.Init();

            // Configure view engine
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(IoC.StructureMapResolver.GetInstance<ThemeableViewEngine>());

            // Configure razor parser
            var razrEngineConfig = new RazorEngine.Configuration.TemplateServiceConfiguration
                         {BaseTemplateType = typeof (RazorTemplateBase<>)};
            var razrTemplService = new TemplateService(razrEngineConfig);
            RazorEngine.Razor.SetTemplateService(razrTemplService);

            // Run other startup tasks
            foreach (var task in IoC.StructureMapResolver.Container.GetAllInstances<IRunAtStartup>())
            {
                task.Execute();
            }
        }

        protected void Application_End()
        {
        }

        public void Application_BeginRequest(Object sender, EventArgs e)
        {
            IoC.StructureMapResolver.CreateNestedContainer();

            foreach (var task in IoC.StructureMapResolver.CurrentNestedContainer.GetAllInstances<IRunOnEachRequest>())
            {
                task.Execute();
            }
        }

        public void Application_EndRequest(Object sender, EventArgs e)
        {
            try
            {
                foreach (var task in IoC.StructureMapResolver.CurrentNestedContainer.GetAllInstances<IRunAfterEachRequest>())
                {
                    task.Execute();
                }
            }
            finally
            {
                HttpContextLifecycle.DisposeAndClearAll();
                IoC.StructureMapResolver.DisposeNestedContainer();
            }

            DataContext.DisposeCurrent();
        }

        public void Application_Error(Object sender, EventArgs e)
        {
            try
            {
                var ex = Server.GetLastError();
                if (ex.GetType() == typeof (DbEntityValidationException))
                {
                    // Get more detailed db entity validation exception

                    var dbex = (DbEntityValidationException) ex;
                    var errorMessages = (from eve in dbex.EntityValidationErrors
                        let entity = eve.Entry.Entity.GetType().Name
                        from ev in eve.ValidationErrors
                        select new
                               {
                                   Entity = entity,
                                   PropertyName = ev.PropertyName,
                                   ErrorMessage = ev.ErrorMessage
                               });

                    var fullErrorMessage = string.Join("; ",
                        errorMessages.Select(err => 
                            string.Format("[Entity: {0}, Property: {1}] {2}", err.Entity, err.PropertyName,
                                    err.ErrorMessage)));

                    var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                    ErrorLog.GetDefault(HttpContext.Current).Log(
                        new Error(new DbEntityValidationException(exceptionMessage, dbex.EntityValidationErrors)));
                }
            }
            finally
            {
                foreach (var task in IoC.StructureMapResolver.CurrentNestedContainer.GetAllInstances<IRunOnError>())
                {
                    task.Execute();
                }

                DataContext.DisposeCurrent();
            }
        }

        public override string GetVaryByCustomString(HttpContext context, string custom)
        {
            if (custom.Contains(','))
            {
                var customStrings = custom.Split(new []{','}, StringSplitOptions.RemoveEmptyEntries);
                return string.Join(",", customStrings.Select(s => GetVaryByCustomString(context, s)));
            }
            switch (custom.ToLower())
            {
                case "lang":
                    return Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName + '-' +
                           Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
                case "category":
                    return CacheHelpers.CategorySeed.ToString(CultureInfo.InvariantCulture);
            }


            return base.GetVaryByCustomString(context, custom);
        }
    }
}