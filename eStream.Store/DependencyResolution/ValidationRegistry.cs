using System;
using System.Linq;
using System.Reflection;
using Estream.Cart42.Web.Helpers;
using FluentValidation;
using LinqKit;
using StructureMap.Configuration.DSL;
using StructureMap.Pipeline;

namespace Estream.Cart42.Web.DependencyResolution
{
    public class ValidationRegistry : Registry
    {
        public ValidationRegistry()
        {
            AssemblyScanner.FindValidatorsInAssembly(Assembly.GetCallingAssembly())
                .ForEach(result =>
                         {
                             if (! Attribute.IsDefined(result.ValidatorType, typeof (DontAutoWireupAttribute)))
                             {
                                 {
                                     For(result.InterfaceType)
                                         .LifecycleIs(new UniquePerRequestLifecycle())
                                         .Use(result.ValidatorType);
                                 }
                             }
                         });
        }
    }
}