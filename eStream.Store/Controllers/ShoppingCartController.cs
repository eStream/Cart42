using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.DependencyResolution;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Models;
using Estream.Cart42.Web.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;

namespace Estream.Cart42.Web.Controllers
{
    public class ShoppingCartController : BaseController
    {
        private readonly ISettingService settings;
        private readonly ITaxZoneService taxZoneService;
        private readonly ICustomerService customerService;
        private readonly ITaxRateService taxRateService;
        private readonly IShippingService shippingService;
        private readonly IProductSkuService productSkuService;
        private readonly IProductService productService;
        public const string CART_COOKIE_NAME = "cartId";

        public ShoppingCartController(DataContext db, ICurrentUser currentUser, ISettingService settings,
            ITaxZoneService taxZoneService, ICustomerService customerService, ITaxRateService taxRateService,
            IShippingService shippingService, IProductSkuService productSkuService, IProductService productService) : base(db, currentUser)
        {
            this.settings = settings;
            this.taxZoneService = taxZoneService;
            this.customerService = customerService;
            this.taxRateService = taxRateService;
            this.shippingService = shippingService;
            this.productSkuService = productSkuService;
            this.productService = productService;
        }

        [ChildActionOnly]
        public PartialViewResult AddItem()
        {
            return PartialView("_AddToCart");
        }

        // POST: ShoppingCart/Create
        [HttpPost]
        public ActionResult AddItem(AddToCartViewModel model)
        {
            ShoppingCart cart = GetOrCreateCart();

            ShoppingCartItem existingCartItem =
                cart.ShoppingCartItems.FirstOrDefault(i => i.ProductId == model.ProductId && i.Options == model.Options);
            if (existingCartItem != null)
            {
                // Add quantity to existing cart item
                existingCartItem.Quantity += model.Quantity ?? 1;
            }
            else
            {
                // Add item to cart
                var cartItem = new ShoppingCartItem
                               {
                                   ShoppingCart = cart,
                                   ProductId = model.ProductId,
                                   Quantity = model.Quantity ?? 1,
                                   Options = model.Options
                               };

                Product product = db.Products.Find(cartItem.ProductId);

                if (product.Skus.Any())
                {
                    var optionsDeserialized =
                        JsonConvert.DeserializeObject<ShoppingCartItemOptionViewModel[]>(model.Options);
                    int[] optionIds = optionsDeserialized.Select(o => o.id).ToArray();
                    //List<Option> options = db.Options.Where(o => optionIds.Contains(o.Id)).ToList();

                    var skus = productSkuService.Find(product.Id, optionIds);
                    if (skus.Any())
                    {
                        cartItem.ProductSku = skus.First();
                    }
                }

                if (cartItem.ProductSku != null && cartItem.ProductSku.Price.HasValue)
                {
                    cartItem.ItemPrice = cartItem.ProductSku.Price.Value;
                }
                else
                {
                    cartItem.ItemPrice = product.SalePrice ?? product.Price;
                }

                db.ShoppingCartItems.Add(cartItem);
            }

            db.SaveChanges();

            return RedirectToAction("Index").WithSuccess("Item added to cart!");
        }

        public ActionResult RemoveItem(int itemId)
        {
            ShoppingCart cart = GetOrCreateCart();
            ShoppingCartItem item = cart.ShoppingCartItems.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                db.ShoppingCartItems.Remove(item);
                db.SaveChanges();
            }

            return RedirectToAction("Index").WithSuccess("Item removed from cart!");
        }

        [HttpPost]
        public ActionResult Update(ShoppingCartViewModel model)
        {
            ShoppingCart cart = GetOrCreateCart();
            foreach (ShoppingCartItem item in cart.ShoppingCartItems.ToList())
            {
                ShoppingCartItemViewModel updatedItem = model.Items.FirstOrDefault(i => i.Id == item.Id);
                if (updatedItem == null) continue;
                if (updatedItem.Quantity <= 0)
                {
                    db.ShoppingCartItems.Remove(item);
                    db.SaveChanges();
                }
                else if (updatedItem.Quantity != item.Quantity)
                {
                    item.Quantity = updatedItem.Quantity;
                    db.SaveChanges();
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult Index()
        {
            ShoppingCart cart = GetOrCreateCart();

            var model = new ShoppingCartViewModel();
            model.Items = Mapper.Map<List<ShoppingCartItemViewModel>>(cart.ShoppingCartItems);

            return View(model);
        }

        [ChildActionOnly]
        public PartialViewResult Preview()
        {
            ShoppingCart cart = GetOrCreateCart();

            var model = new ShoppingCartViewModel();
            model.Items = Mapper.Map<List<ShoppingCartItemViewModel>>(cart.ShoppingCartItems);

            return PartialView("_Preview", model);
        }

        public ActionResult Checkout()
        {
            ShoppingCart cart = GetOrCreateCart();

            if (!cart.ShoppingCartItems.Any())
                return RedirectToAction("Index");

            var model = Mapper.Map<ShoppingCartCheckoutViewModel>(cart);

            // Load default addresses
            if (User.Identity.IsAuthenticated)
            {
                User user = customerService.Find(currentUser.User.Id);

                if (user != null)
                {
                    var billingAddress = customerService.GetAddress(user.Id, AddressType.Billing);
                    model.BillingAddress = Mapper.Map<AddressViewModel>(billingAddress ?? new Address());

                    var shippingAddress = customerService.GetAddress(user.Id, AddressType.Billing);
                    model.ShippingAddress = Mapper.Map<AddressViewModel>(shippingAddress ?? new Address());

                    model.SameShippingAddress = billingAddress == shippingAddress;
                }
            }

            List<Country> countries = db.Countries.Where(c => c.IsActive).OrderBy(c => c.Name).ToList();
            ViewBag.Countries = countries;
            return View(model);
        }

        [HttpPost]
        public ActionResult Checkout(ShoppingCartCheckoutViewModel model)
        {
            if (!ModelState.IsValid)
                return JsonValidationError();

            // Get cart contents
            ShoppingCart cart = GetOrCreateCart();
            if (!cart.ShoppingCartItems.Any())
            {
                return JsonError("Your shopping cart is empty!");
            }

            // Check quantity
            foreach (ShoppingCartItem cartItem in cart.ShoppingCartItems)
            {
                Product product = db.Products.Find(cartItem.ProductId);
                int? qty = null;
                if (cartItem.ProductSkuId.HasValue && cartItem.ProductSku.Quantity.HasValue)
                    qty = cartItem.ProductSku.Quantity.Value;
                if (qty == null && product.Quantity.HasValue)
                    qty = product.Quantity.Value;
                if (qty.HasValue && qty < cartItem.Quantity)
                {
                    return JsonError(string.Format("The requested quantity for \"{0}\" is not available", product.Name));
                }
            }

            // Get current user (or create a new one)
            User user = null;
            if (User.Identity.IsAuthenticated)
            {
                user = customerService.Find(currentUser.User.Id);
            }
            if (user == null)
            {
                var userModel = new CustomerViewModel
                                {
                                    FirstName = model.BillingAddress.FirstName,
                                    LastName = model.BillingAddress.LastName,
                                    Company = model.BillingAddress.Company,
                                    PhoneNumber = model.BillingAddress.Phone,
                                    Email = model.Email
                                };
                try
                {
                    user = customerService.AddOrUpdate(userModel);
                }
                catch (ArgumentException err)
                {
                    return JsonError(err.Message);
                }

                customerService.LoginUser(HttpContext, user);
            }
            
            // Get addresses
            var billingAddress = Mapper.Map<Address>(model.BillingAddress);
            billingAddress.Type = AddressType.Billing;

            var shippingAddress = Mapper.Map<Address>(model.SameShippingAddress
                ? model.BillingAddress
                : model.ShippingAddress);
            shippingAddress.Type = AddressType.Shipping;

            var defaultBillingAddress = customerService.GetAddress(user.Id, AddressType.Billing);
            if (defaultBillingAddress == null)
            {
                // Add default billing address
                defaultBillingAddress = Mapper.Map<Address>(model.BillingAddress);
                defaultBillingAddress.Type = AddressType.Billing;
                defaultBillingAddress.IsPrimary = true;
                user.Addresses.Add(defaultBillingAddress);
            }

            var defaultShippingAddress = customerService.GetAddress(user.Id, AddressType.Shipping);
            if (defaultShippingAddress == null)
            {
                // Add default shipping address
                defaultShippingAddress = Mapper.Map<Address>(model.SameShippingAddress
                    ? model.BillingAddress
                    : model.ShippingAddress);
                defaultShippingAddress.Type = AddressType.Shipping;
                defaultShippingAddress.IsPrimary = true;
                user.Addresses.Add(defaultShippingAddress);
            }

            db.SaveChanges();

            // Create order
            var order = new Order
                        {
                            UserId = user.Id,
                            BillingAddress = billingAddress,
                            ShippingAddress = shippingAddress,
                            DatePlaced = DateTime.Now,
                            DateUpdated = DateTime.Now,
                            IPAddress = Request.UserHostAddress,
                            UserComments = model.UserComments,
                            Status = OrderStatus.AwaitingPayment
                        };

            db.Orders.Add(order);

            TaxZone taxZone = taxZoneService.Find(billingAddress.CountryCode, billingAddress.RegionId);

            foreach (ShoppingCartItem cartItem in cart.ShoppingCartItems)
            {
                Product product = db.Products.Find(cartItem.ProductId);
                ProductSku productSku = cartItem.ProductSku;

                if (productSku != null && productSku.Quantity.HasValue)
                {
                    productSkuService.RemoveQuantity(productSku.Id, cartItem.Quantity);
                }
                else if (product.Quantity.HasValue)
                {
                    productService.RemoveQuantity(product.Id, cartItem.Quantity);
                }
                
                decimal price = product.SalePrice ?? product.Price;
                if (cartItem.ProductSkuId.HasValue && cartItem.ProductSku.Price.HasValue)
                    price = cartItem.ProductSku.Price.Value;

                var cartItemOptions = JsonConvert.DeserializeObject<ShoppingCartItemOptionViewModel[]>(cartItem.Options);
                var orderItemOptions = Mapper.Map<OrderItemOption[]>(cartItemOptions);

                var orderItem = new OrderItem
                                {
                                    Order = order,
                                    ProductId = product.Id,
                                    ProductSkuId = cartItem.ProductSkuId,
                                    Quantity = cartItem.Quantity,
                                    Options = JsonConvert.SerializeObject(orderItemOptions),
                                    ItemPrice = price
                                };

                db.OrderItems.Add(orderItem);

                order.Subtotal += cartItem.Quantity * price;

                if (taxZone != null)
                    order.TaxAmount += taxRateService.CalculateTax(taxZone.Id, product.TaxClassId, price * cartItem.Quantity);
            }

            ShippingMethod shippingMethod = db.ShippingMethods.Find(model.ShippingMethodId);
            order.ShippingAmount = shippingService.CalculateShipping(shippingMethod,
                cart.ShoppingCartItems.Sum(i => i.Quantity),
                cart.ShoppingCartItems.Sum(i => i.Quantity*i.Product.Weight),
                order.Subtotal, shippingAddress).GetValueOrDefault();

            order.Total = order.Subtotal + order.ShippingAmount;
            if (!settings.Get<bool>(SettingField.TaxIncludedInPrices))
                order.Total += order.TaxAmount;

            db.SaveChanges();

            return JsonSuccess(new {orderId = order.Id, paymentMethodId = model.PaymentMethodId});
        }

        public StandardJsonResult CalculateTax(decimal subtotal, string countryCode, int? regionId, int? taxClassId)
        {
            var taxResult = new
                            {
                                taxRate = 0m,
                                taxAmount = 0m,
                                taxName = "Tax"
                            };

            TaxZone taxZone = taxZoneService.Find(countryCode, regionId);

            //TODO: Tax class should be calculated per product
            if (taxZone != null)
            {
                TaxRate taxRate = db.TaxRates.FirstOrDefault(r => r.TaxZoneId == taxZone.Id);
                if (taxRate != null)
                {
                    TaxClassRate taxClassRate =
                        db.TaxClassRates.FirstOrDefault(r => r.TaxRateId == taxRate.Id && r.TaxClassId == taxClassId);

                    decimal rate = taxClassRate != null ? taxClassRate.Amount : taxRate.Amount;
                    taxResult = new
                                {
                                    taxRate = rate,
                                    taxAmount = (subtotal*rate)/100m,
                                    taxName = taxRate.Name
                                };
                }
            }

            return JsonSuccess(taxResult);
        }

        public StandardJsonResult GetShippingMethods(ShoppingCartCheckoutViewModel model)
        {
            var result = new List<ShoppingCartShippingMethodsViewModel>();
            AddressViewModel address = model.SameShippingAddress ? model.BillingAddress : model.ShippingAddress;

            // Find zone for country and region (if supplied)
            ShippingZone zone = shippingService.FindZone(address.CountryCode, address.RegionId);

            if (zone != null)
            {
                List<ShippingMethod> methods = shippingService.FindMethods(zone.Id).ToList();
                foreach (ShippingMethod method in methods)
                {
                    decimal? cost = shippingService.CalculateShipping(method,
                        model.ShoppingCartItems.Sum(i => i.Quantity),
                        model.ShoppingCartItems.Sum(i => i.Quantity*db.Products.Find(i.ProductId).Weight),
                        model.ShoppingCartItems.Sum(i => i.Quantity*i.ItemPrice), Mapper.Map<Address>(address));

                    if (!cost.HasValue) continue;

                    var methodView = new ShoppingCartShippingMethodsViewModel
                                     {
                                         Id = method.Id,
                                         Name = method.Name,
                                         Amount = cost.Value
                                     };

                    result.Add(methodView);
                }
            }

            return JsonSuccess(result.OrderBy(s => s.Amount));
        }

        private ShoppingCart GetOrCreateCart()
        {
            ShoppingCart cart = null;

            // Try to find cart for logged in user
            if (Request.IsAuthenticated)
            {
                string userId = currentUser.User.Id;
                cart = db.ShoppingCarts.FirstOrDefault(c => c.UserId == userId);
            }

            // Try to find cart by cookie
            HttpCookie cartCookie = Request.Cookies[CART_COOKIE_NAME];
            if (cart == null && cartCookie != null)
            {
                string cartId = cartCookie.Value;
                cart = db.ShoppingCarts.Find(Guid.Parse(cartId));
                if (cart != null && Request.IsAuthenticated)
                {
                    cart.UserId = currentUser.User.Id;
                    db.SaveChanges();
                }
            }

            // Create new cart
            if (cart == null)
            {
                cart = new ShoppingCart {DateCreated = DateTime.Now};
                cart.DateUpdated = cart.DateCreated;
                if (Request.IsAuthenticated)
                    cart.UserId = currentUser.User.Id;

                db.ShoppingCarts.Add(cart);
                db.SaveChanges();

                // Add cookie with cart id
                cartCookie = new HttpCookie(CART_COOKIE_NAME)
                             {
                                 Value = cart.Id.ToString(),
                                 Expires = DateTime.Now.AddDays(30)
                             };
                Response.SetCookie(cartCookie);
            }
            return cart;
        }

        public static async Task SignInAsync(IAuthenticationManager authManager, ApplicationUserManager userManager,
            User user, bool isPersistent)
        {
            authManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            authManager.SignIn(new AuthenticationProperties {IsPersistent = isPersistent},
                await user.GenerateUserIdentityAsync(userManager));
        }
    }
}