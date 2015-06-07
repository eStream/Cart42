using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Estream.Cart42.Web.DependencyResolution.ModelMetadata;
using Estream.Cart42.Web.DependencyResolution.Tasks;
using StructureMap;

namespace Estream.Cart42.Web.DependencyResolution
{
    public class IoC
    {
        public static StructureMapDependencyResolver StructureMapResolver { get; set; }

        public static void Init()
        {
            var container = new Container(cfg =>
            {
                cfg.AddRegistry(new DefaultRegistry());
                cfg.AddRegistry(new ControllerRegistry());
                cfg.AddRegistry(new ActionFilterRegistry(
                        () => StructureMapResolver.CurrentNestedContainer));
                cfg.AddRegistry(new MvcRegistry());
                cfg.AddRegistry(new TaskRegistry());
                cfg.AddRegistry(new ModelMetadataRegistry());
                cfg.AddRegistry(new PaymentMethodRegistry());
                cfg.AddRegistry(new ValidationRegistry());
            });

            StructureMapResolver = new StructureMapDependencyResolver(container);
            DependencyResolver.SetResolver(StructureMapResolver);

        }
    }
}