using AutoMapper;
using BookVerse.DataAccess.Repository.IRepository;
using BookVerse.Models;
using BookVerse.Models.ViewModels;
using BookVerse.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace BookVerse.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;   // ? ADDED

        public ProductsController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IWebHostEnvironment webHostEnvironment)                // ? ADDED
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;               // ? ADDED
        }

        // GET: Products
        public IActionResult Index()
        {
            var products = _unitOfWork.Product.GetAll(includeProperties: "Category,Company");
            var productVMs = _mapper.Map<IEnumerable<ProductVM>>(products);
            return View(productVMs);
        }

        // GET: Products/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();
            var product = _unitOfWork.Product.Get(p => p.Id == id, includeProperties: "Category,Company");
            if (product == null) return NotFound();
            return View(_mapper.Map<ProductVM>(product));
        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Customer + "," + SD.Role_Company + "," + SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int productId, int count = 1)
        {
            if (count < 1)
            {
                count = 1;
            }

            var product = _unitOfWork.Product.Get(p => p.Id == productId);
            if (product == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartFromDb = _unitOfWork.ShoppingCart.Get(
                c => c.ApplicationUserId == userId && c.ProductId == productId);

            if (cartFromDb == null)
            {
                _unitOfWork.ShoppingCart.Add(new ShoppingCart
                {
                    ProductId = productId,
                    ApplicationUserId = userId!,
                    Count = count
                });
            }
            else
            {
                cartFromDb.Count += count;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }

            _unitOfWork.Save();
            TempData["success"] = "Book added to cart.";
            return RedirectToAction(nameof(Details), new { id = productId });
        }
        // GET: Products/Create
        public IActionResult Create()
        {
            return View(new ProductUpsertVM
            {
                Product = new Product(),
                CategoryList = GetCategorySelectList(),
                CompanyList = GetCompanySelectList()
            });
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductUpsertVM vm)
        {
            if (ModelState.IsValid)
            {
                // ? ADDED — handle uploaded image file
                if (vm.File != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(vm.File.FileName);
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    using (var fileStream = new FileStream(Path.Combine(uploadsFolder, fileName), FileMode.Create))
                    {
                        vm.File.CopyTo(fileStream);
                    }

                    vm.Product.ImageUrl = @"\images\products\" + fileName;
                }

                _unitOfWork.Product.Add(vm.Product);
                _unitOfWork.Save();
                TempData["success"] = "Product created successfully.";
                return RedirectToAction(nameof(Index));
            }
            vm.CategoryList = GetCategorySelectList();
            vm.CompanyList = GetCompanySelectList();
            return View(vm);
        }

        // GET: Products/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();
            var product = _unitOfWork.Product.Get(p => p.Id == id);
            if (product == null) return NotFound();
            return View(new ProductUpsertVM
            {
                Product = product,
                CategoryList = GetCategorySelectList(product.CategoryId),
                CompanyList = GetCompanySelectList(product.CompanyId)
            });
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ProductUpsertVM vm)
        {
            if (id != vm.Product.Id) return NotFound();
            if (ModelState.IsValid)
            {
                // ? ADDED — handle uploaded image file (optional on edit)
                if (vm.File != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Delete old image if one exists
                    if (!string.IsNullOrEmpty(vm.Product.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(
                            _webHostEnvironment.WebRootPath,
                            vm.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(vm.File.FileName);

                    using (var fileStream = new FileStream(Path.Combine(uploadsFolder, fileName), FileMode.Create))
                    {
                        vm.File.CopyTo(fileStream);
                    }

                    vm.Product.ImageUrl = @"\images\products\" + fileName;
                }
                // If vm.File is null, vm.Product.ImageUrl keeps whatever
                // value the form sent (preserved via hidden field in view)

                _unitOfWork.Product.Update(vm.Product);
                _unitOfWork.Save();
                TempData["success"] = "Product updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            vm.CategoryList = GetCategorySelectList(vm.Product.CategoryId);
            vm.CompanyList = GetCompanySelectList(vm.Product.CompanyId);
            return View(vm);
        }

        // GET: Products/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();
            var product = _unitOfWork.Product.Get(p => p.Id == id, includeProperties: "Category,Company");
            if (product == null) return NotFound();
            return View(_mapper.Map<ProductVM>(product));
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var product = _unitOfWork.Product.Get(p => p.Id == id);
            if (product == null) return NotFound();
            _unitOfWork.Product.Remove(product);
            _unitOfWork.Save();
            TempData["success"] = "Product deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // -- Helpers ----------------------------------------------------------

        private IEnumerable<SelectListItem> GetCategorySelectList(int? selectedId = null) =>
            _unitOfWork.Category.GetAll()
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString(),
                    Selected = c.Id == selectedId
                });

        private IEnumerable<SelectListItem> GetCompanySelectList(int? selectedId = null) =>
            _unitOfWork.Company.GetAll()
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString(),
                    Selected = c.Id == selectedId
                });
    }
}

