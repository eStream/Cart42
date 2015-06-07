using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebGrease.Css.Extensions;

namespace Estream.Cart42.Web.DependencyResolution.ModelMetadata
{
    public class ExtensibleModelMetadataProvider
        : DataAnnotationsModelMetadataProvider
    {
        private readonly IModelMetadataFilter[] metadataFilters;

        public ExtensibleModelMetadataProvider(IModelMetadataFilter[] metadataFilters)
        {
            this.metadataFilters = metadataFilters;
        }

        protected override System.Web.Mvc.ModelMetadata CreateMetadata(
            IEnumerable<Attribute> attributes,
            Type containerType,
            Func<object> modelAccessor,
            Type modelType,
            string propertyName)
        {
            Attribute[] attributesArray = attributes as Attribute[] ?? attributes.ToArray();
            System.Web.Mvc.ModelMetadata metadata = base.CreateMetadata(
                attributesArray,
                containerType,
                modelAccessor,
                modelType,
                propertyName);

            metadataFilters.ForEach(m => m.TransformMetadata(metadata, attributesArray));

            return metadata;
        }
    }
}