using StructureMap.Configuration.DSL;
using StructureMap.Graph;

namespace Estream.Cart42.Web.DependencyResolution
{
    public class ControllerRegistry : Registry
    {
        public ControllerRegistry()
        {
            Scan(
                scan =>
                {
                    scan.TheCallingAssembly();
                    scan.With(new ControllerConvention());
                });
        }
    }
}