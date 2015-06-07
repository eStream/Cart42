using System.Reflection.Emit;
using System.Web.Mvc;
using Estream.Cart42.Web.DependencyResolution;
using Estream.Cart42.Web.Helpers;
using FluentValidation;
using FluentValidation.Mvc;

namespace Estream.Cart42.Web
{
    public static class ValidatorConfig
    {
        public static void Init()
        {
            var factory = new StructureMapValidatorFactory();
            ModelValidatorProviders.Providers.Add(new FluentValidationModelValidatorProvider(factory));
            DataAnnotationsModelValidatorProvider.AddImplicitRequiredAttributeForValueTypes = false;
            ValidatorOptions.DisplayNameResolver = (type, info, arg3) =>
            {
                if (info != null)
                {
                    return info.Name.TA();
                }
                return null;
            };
            ValidatorOptions.ResourceProviderType = typeof (TranslateResource);
        }
    }

    public class TranslateResource
    {
        public static string notempty_error
        {
            get { return "'{PropertyName}' is required!".TA(); }
        }

        public static string notnull_error
        {
            get { return "'{PropertyName}' is required!".TA(); }
        }

        public static string length_error
        {
            get { return "'{PropertyName}' must be between '{MinLength}' and '{MaxLength} characters".TA(); }
        }

        public static string email_error
        {
            get { return "Please specify valid email!".TA(); }
        }

        public static string greaterthanorequal_error
        {
            get { return "'{PropertyName}' must be greater than or equal to '{ComparisonValue}'".TA(); }
        }

        public static string greaterthan_error
        {
            get { return "'{PropertyName}' must be greater than '{ComparisonValue}'".TA(); }
        }

/*
email_error
equal_error
exact_length_error
exclusivebetween_error
greaterthan_error
greaterthanorequal_error
inclusivebetween_error
length_error
lessthan_error
lessthanorequal_error
notempty_error
notequal_error
notnull_error
predicate_error
regex_error
*/
    }
}