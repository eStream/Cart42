using StructureMap.Configuration.DSL;
using StructureMap.Graph;

namespace Estream.Cart42.Web.DependencyResolution
{
    public class DefaultRegistry : Registry
    {
        public DefaultRegistry()
        {
            Scan(
                scan =>
                {
                    scan.TheCallingAssembly();
                    scan.WithDefaultConventions();
                });
        }
    }
}