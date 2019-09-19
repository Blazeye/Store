using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Store.Domain.Abstract;
using System.Linq;

namespace Store.WebUI.Controllers
{
    public class NavController : Controller
    {
        private IProductRepository repository;

        public NavController(IProductRepository repo)
        {
            repository = repo;
        }
        // GET: Nav
        public PartialViewResult Menu(string category = null)
        {
            // naast null <, bool horizontalLayout = false>
            ViewBag.SelectedCategory = category;

            IEnumerable<string> categories = repository.Products
                                    .Select(x => x.Category)
                                    .Distinct()
                                    .OrderBy(x => x);

            //string viewName = horizontalLayout ? "MenuHorizontal" : "Menu";
            return PartialView("FlexMenu", categories);
            //"FlexMenu" ipv viewName
        }
    }
}