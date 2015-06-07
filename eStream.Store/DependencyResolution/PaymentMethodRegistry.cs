using Estream.Cart42.Web.DependencyResolution.PaymentMethods;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;

namespace Estream.Cart42.Web.DependencyResolution
{
    public class PaymentMethodRegistry : Registry
    {
        public PaymentMethodRegistry()
        {
            Scan(
                scan =>
                {
                    scan.TheCallingAssembly();
                    scan.AddAllTypesOf<IPaymentMethod>().NameBy(t => t.Name);
                });
        }
    }
}