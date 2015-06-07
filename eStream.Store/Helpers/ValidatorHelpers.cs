using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentValidation;

namespace Estream.Cart42.Web.Helpers
{
    public class NullValidator<T> : AbstractValidator<T>
    {
    }

    public class DontAutoWireupAttribute : Attribute
    {
    }
}