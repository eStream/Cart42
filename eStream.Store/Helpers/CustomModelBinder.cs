using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Estream.Cart42.Web.Helpers
{
    // Auto create empty arrays when null
    public class CustomModelBinder : DefaultModelBinder
    {
        protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
        {
            var model = base.CreateModel(controllerContext, bindingContext, modelType);

            if (model == null || model is IEnumerable)
                return model;

            foreach (var property in modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                object value = property.GetValue(model);
                if (value != null)
                    continue;

                if (property.PropertyType.IsArray)
                {
                    value = Array.CreateInstance(property.PropertyType.GetElementType(), 0);
                    property.SetValue(model, value);
                }
                else if (property.PropertyType.IsGenericType)
                {
                    Type typeToCreate;
                    Type genericTypeDefinition = property.PropertyType.GetGenericTypeDefinition();
                    if (genericTypeDefinition == typeof(IDictionary<,>))
                    {
                        typeToCreate = typeof(Dictionary<,>).MakeGenericType(property.PropertyType.GetGenericArguments());
                    }
                    else if (genericTypeDefinition == typeof(IEnumerable<>) ||
                             genericTypeDefinition == typeof(ICollection<>) ||
                             genericTypeDefinition == typeof(IList<>))
                    {
                        typeToCreate = typeof(List<>).MakeGenericType(property.PropertyType.GetGenericArguments());
                    }
                    else
                    {
                        continue;
                    }

                    value = Activator.CreateInstance(typeToCreate);
                    property.SetValue(model, value);
                }
            }

            return model;
        }
    }
}