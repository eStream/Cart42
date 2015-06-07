using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using Estream.Cart42.Web.DependencyResolution;
using Estream.Cart42.Web.DependencyResolution.Tasks;
using StructureMap;

namespace Estream.Cart42.Web
{
    public class AutoMapperConfig : IRunAtStartup
    {
        public void Execute()
        {
            /*
            Mapper.Initialize(c => c.ConstructServicesUsing(
                type => IoC.StructureMapResolver.CurrentNestedContainer.GetInstance(type) ));
            */

            Type[] types = Assembly.GetExecutingAssembly().GetExportedTypes();

            LoadStandardMappings(types);

            LoadCustomMappings(types);
        }

        private static void LoadCustomMappings(IEnumerable<Type> types)
        {
            IHaveCustomMappings[] maps = (from t in types
                from i in t.GetInterfaces()
                where typeof (IHaveCustomMappings).IsAssignableFrom(t) &&
                      !t.IsAbstract &&
                      !t.IsInterface
                select (IHaveCustomMappings) Activator.CreateInstance(t)) //IoC.StructureMapResolver.GetInstance(t))
                .ToArray();

            foreach (IHaveCustomMappings map in maps)
            {
                map.CreateMappings(Mapper.Configuration);
            }
        }

        private static void LoadStandardMappings(IEnumerable<Type> types)
        {
            var maps = (from t in types
                from i in t.GetInterfaces()
                where i.IsGenericType && i.GetGenericTypeDefinition() == typeof (IMapFrom<>) &&
                      !t.IsAbstract &&
                      !t.IsInterface
                select new
                       {
                           Source = i.GetGenericArguments()[0],
                           Destination = t
                       }).ToArray();

            foreach (var map in maps)
            {
                Mapper.CreateMap(map.Source, map.Destination);
            }
        }
    }

    public interface IHaveCustomMappings
    {
        void CreateMappings(IConfiguration configuration);
    }

    public interface IMapFrom<T>
    {
    }
}