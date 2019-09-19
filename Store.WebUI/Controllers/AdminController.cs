using System.Linq;
using Store.Domain.Abstract;
using System.Web.Mvc;
using Store.Domain.Entities;

namespace Store.WebUI.Controllers
{
    public class AdminController : Controller
    {
        private IProductRepository repository;
        // GET: Admin

        public AdminController(IProductRepository repo)
        {
            repository = repo;
        }

        public ViewResult Index()
        {
            return View(repository.Products);
        }

        public ViewResult Edit(int productId)
        {
            Product product = repository.Products
                .FirstOrDefault(p => p.ProductID == productId);
            return View(product);
        }

        [HttpPost] //this is an overload of the action method in the admin controller that will handle post requests when saving
        public ActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                repository.SaveProduct(product);
                TempData["message"] = string.Format("{0} has been saved", product.Name); //Tempdata is like session, 
                return RedirectToAction("Index");                                        // but deleted at the end of the Http request. It is a dictionary.
            }                                                                            // I can't use Viewbag instead, cause it passes between Model and View
            else
            {
                // there is something wrong with the data values
                return View(product);
            }
        }

        public ViewResult Create()
        {
            return View("Edit", new Product());
        }

        [HttpPost]
        public ActionResult Delete(int productId)
        {
            Product deletedProduct = repository.DeleteProduct(productId);
            if(deletedProduct != null)
            {
                TempData["message"] = string.Format("{0} was deleted",
                    deletedProduct.Name);
            }
            return RedirectToAction("Index");
        }
    }
}