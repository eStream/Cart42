using System;
using System.Web.Mvc;
using StructureMap;
using StructureMap.Configuration.DSL;
using StructureMap.TypeRules;

namespace Estream.Cart42.Web.DependencyResolution
{
    public class ActionFilterRegistry : Registry
    {
        public ActionFilterRegistry(Func<IContainer> containerFactory)
        {
            For<IFilterProvider>().Use(
                new StructureMapFilterProvider(containerFactory));

            Policies.SetAllProperties(x =>
                x.Matching(p =>
                    p.DeclaringType.CanBeCastTo(typeof(ActionFilterAttribute)) &&
                    p.DeclaringType.Namespace.StartsWith("Estream") &&
                    !p.PropertyType.IsPrimitive &&
                    p.PropertyType != typeof(string)));
        }
    }
}