using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using AutoMapper;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Models
{
    public class OrderCompletedViewModel
    {
        public int OrderId { get; set; }

        public string Message { get; set; }
    }
}