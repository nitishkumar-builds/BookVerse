using AutoMapper;
using BookVerse.DataAccess.Repository.IRepository;
using BookVerse.Models;
using BookVerse.Models.ViewModels;
using BookVerse.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoriesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoriesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // GET: Categories
        public IActionResult Index()
        {
            IEnumerable<Category> categories = _unitOfWork.Category.GetAll();
            IEnumerable<CategoryVM> categoryVMs = _mapper.Map<IEnumerable<CategoryVM>>(categories);
            return View(categoryVMs);
        }

        // GET: Categories/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
                return NotFound();

            Category category = _unitOfWork.Category.Get(c => c.Id == id);
            if (category == null)
                return NotFound();

            CategoryVM categoryVM = _mapper.Map<CategoryVM>(category);
            return View(categoryVM);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View(new CategoryVM());
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CategoryVM categoryVM)
        {
            if (!ModelState.IsValid)
                return View(categoryVM);

            Category category = _mapper.Map<Category>(categoryVM);
            _unitOfWork.Category.Add(category);
            _unitOfWork.Save();

            TempData["success"] = "Category created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Categories/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
                return NotFound();

            Category category = _unitOfWork.Category.Get(c => c.Id == id);
            if (category == null)
                return NotFound();

            CategoryVM categoryVM = _mapper.Map<CategoryVM>(category);
            return View(categoryVM);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, CategoryVM categoryVM)
        {
            if (id != categoryVM.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(categoryVM);

            Category category = _mapper.Map<Category>(categoryVM);
            _unitOfWork.Category.Update(category);
            _unitOfWork.Save();

            TempData["success"] = "Category updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Categories/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
                return NotFound();

            Category category = _unitOfWork.Category.Get(c => c.Id == id);
            if (category == null)
                return NotFound();

            CategoryVM categoryVM = _mapper.Map<CategoryVM>(category);
            return View(categoryVM);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            Category category = _unitOfWork.Category.Get(c => c.Id == id);
            if (category == null)
                return NotFound();

            _unitOfWork.Category.Remove(category);
            _unitOfWork.Save();

            TempData["success"] = "Category deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}