using System;
using FluentValidation;

namespace Estream.Cart42.Web.DependencyResolution
{
    public class StructureMapValidatorFactory : ValidatorFactoryBase
    {
        public override IValidator CreateInstance(Type validatorType)
        {
            return IoC.StructureMapResolver.Container.TryGetInstance(validatorType) as IValidator;
        }
    }
}