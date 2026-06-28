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
    public class CompaniesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CompaniesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // GET: Companies
        public IActionResult Index()
        {
            IEnumerable<Company> companies = _unitOfWork.Company.GetAll();
            IEnumerable<CompanyVM> companyVMs = _mapper.Map<IEnumerable<CompanyVM>>(companies);
            return View(companyVMs);
        }

        // GET: Companies/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
                return NotFound();

            Company company = _unitOfWork.Company.Get(c => c.Id == id);
            if (company == null)
                return NotFound();

            CompanyVM companyVM = _mapper.Map<CompanyVM>(company);
            return View(companyVM);
        }

        // GET: Companies/Create
        public IActionResult Create()
        {
            return View(new CompanyVM());
        }

        // POST: Companies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CompanyVM companyVM)
        {
            if (!ModelState.IsValid)
                return View(companyVM);

            Company company = _mapper.Map<Company>(companyVM);
            _unitOfWork.Company.Add(company);
            _unitOfWork.Save();

            TempData["success"] = "Company created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Companies/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
                return NotFound();

            Company company = _unitOfWork.Company.Get(c => c.Id == id);
            if (company == null)
                return NotFound();

            CompanyVM companyVM = _mapper.Map<CompanyVM>(company);
            return View(companyVM);
        }

        // POST: Companies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, CompanyVM companyVM)
        {
            if (id != companyVM.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(companyVM);

            Company company = _mapper.Map<Company>(companyVM);
            _unitOfWork.Company.Update(company);
            _unitOfWork.Save();

            TempData["success"] = "Company updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Companies/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
                return NotFound();

            Company company = _unitOfWork.Company.Get(c => c.Id == id);
            if (company == null)
                return NotFound();

            CompanyVM companyVM = _mapper.Map<CompanyVM>(company);
            return View(companyVM);
        }

        // POST: Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            Company company = _unitOfWork.Company.Get(c => c.Id == id);
            if (company == null)
                return NotFound();

            _unitOfWork.Company.Remove(company);
            _unitOfWork.Save();

            TempData["success"] = "Company deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}