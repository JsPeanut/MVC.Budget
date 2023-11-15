using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVC.Budget.JsPeanut.Areas.Identity.Data;
using MVC.Budget.JsPeanut.Data;
using MVC.Budget.JsPeanut.Models;
using MVC.Budget.JsPeanut.Models.ViewModel;
using MVC.Budget.JsPeanut.Services;
using System.Text.Json;

namespace MVC.Budget.JsPeanut.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly DataContext _context;
        private readonly CategoryService _categoryService;
        private readonly TransactionService _transactionService;
        private readonly JsonFileCurrencyService _jsonFileCurrencyService;
        private readonly CurrencyConverterService _currencyConverterService;
        private readonly UserManager<ApplicationUser> _userManager;
        public TransactionsController(DataContext context, CategoryService categoryService, TransactionService transactionService, JsonFileCurrencyService jsonFileCurrencyService, CurrencyConverterService currencyConverterService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _categoryService = categoryService;
            _transactionService = transactionService;
            _jsonFileCurrencyService = jsonFileCurrencyService;
            _currencyConverterService = currencyConverterService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int id = -1, string name = "", string imageurl = "", string timeline = "", string searchStringForName = "", string filterByCategoryString = "", string filterByDateString = "")
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);

			if (user == null)
			{
				return RedirectToPage("/Account/Login", new { area = "Identity" });
			}

            var transactions = _transactionService.GetAllTransactions().Where(t => t.UserId == userId).ToList();

			if (!string.IsNullOrEmpty(timeline))
            {
                switch (timeline)
                {
                    case "Today":
                        transactions = transactions.Where(x => x.Date.Day == DateTime.Now.Day).ToList();
                        break;
                    case "Yesterday":
                        transactions = transactions.Where(x => x.Date.Day == (DateTime.Now.AddDays(-1).Day)).ToList();
                        break;
                    case "default":
                        transactions = _transactionService.GetAllTransactions();
                        break;
                }
            }
            if (!string.IsNullOrEmpty(searchStringForName))
            {
                transactions = transactions.Where(x => x.Name.Contains(searchStringForName)).ToList();
            }
            if (!string.IsNullOrEmpty(filterByCategoryString))
            {
                var categories_ = _categoryService.GetAllCategories();
                Category? category = categories_.Where(x => x.Name == filterByCategoryString).FirstOrDefault();
                if (category != null)
                {
                    transactions = transactions.Where(x => x.CategoryId == category.Id).ToList();
                }
            }
            if (!string.IsNullOrEmpty(filterByDateString))
            {
                DateTime searchDate;
                if (DateTime.TryParse(filterByDateString, out searchDate))
                {
                    transactions = transactions.Where(x => x.Date.Date == searchDate.Date).ToList();
                }
            }

            var transactionsToShow = new List<Transaction>();
            var categories = _categoryService.GetAllCategories();
            var currencies = _jsonFileCurrencyService.GetCurrencyList();
            var categoryselectlist_ = new List<SelectListItem>();
            var currencyselectlist_ = new List<SelectListItem>();

            ViewBag.ImageUrl = imageurl;
            ViewBag.Category = name;

            foreach (var category in categories)
            {
                categoryselectlist_.Add(new SelectListItem
                {
                    Text = category.Name,
                    Value = category.Id.ToString()
                });
            }
            foreach (var currency in currencies)
            {
                currencyselectlist_.Add(new SelectListItem
                {
                    Text = $"{currency.Name} ({currency.CurrencyCode})",
                    Value = JsonSerializer.Serialize(currency)
                });
            }
            if (id == -1)
            {
                transactionsToShow = transactions;
                ViewBag.ImageUrl = null;
                ViewBag.Category = null;
            }
            else
            {
                transactionsToShow = _transactionService.GetAllTransactions().Where(x => x.CategoryId == id).ToList();
            }

            var transactionViewModel = new TransactionViewModel
            {
                Transactions = transactionsToShow,
                Categories = _categoryService.GetAllCategories(),
                CategorySelectList = categoryselectlist_,
                CurrencySelectList = currencyselectlist_
            };

            return View(transactionViewModel);
        }

        public IActionResult UpdateTransaction(Models.TransactionInputModel transactionInputModel, CategoryViewModel cvm)
        {
			ModelState.Remove("Categories");
			ModelState.Remove("Transactions");
			ModelState.Remove("CategorySelectList");
			ModelState.Remove("CurrencySelectList");
			ModelState.Remove("CurrentUser");
			ModelState.Remove("CurrencyObjectJson");
			if (!ModelState.IsValid)
			{
				TempData["error"] = "Something went wrong, your transaction wasn't updated";

				return RedirectToAction("Index", "Transactions");
			}
            try
            {
                var transaction = new Transaction
                {
                    Id = transactionInputModel.Id,
                    Date = transactionInputModel.Date,
                    Name = transactionInputModel.Name,
                    CategoryId = transactionInputModel.CategoryId,
                    Value = transactionInputModel.Value,
                    Description = transactionInputModel.Description,
                    UserId = transactionInputModel.UserId
                };

                var existingTransaction = _transactionService.GetTransaction(transaction.Id);

				if (existingTransaction != null)
				{
					existingTransaction.Date = transaction.Date;
					existingTransaction.Name = transaction.Name;
					existingTransaction.CategoryId = transaction.CategoryId;
					existingTransaction.Value = transaction.Value;
					existingTransaction.Category = transaction.Category;

					_transactionService.UpdateTransaction(existingTransaction);

					var categories = _categoryService.GetAllCategories();
					var transactionCategory = categories.Where(c => c.Id == transaction.CategoryId).First();
					var selectedCurrencyOption = JsonSerializer.Deserialize<Currency>(cvm.CurrencyObjectJson);
					existingTransaction.CurrencyCode = selectedCurrencyOption.CurrencyCode;
					existingTransaction.CurrencyNativeSymbol = selectedCurrencyOption.NativeSymbol;
				}
			}
            catch (Exception ex)
            {
				TempData["error"] = $"Something went wrong, your transaction wasn't updated: {ex.Message}";

				return RedirectToAction("Index", "Transactions");
			}

			TempData["success"] = "Transaction updated successfully";

			return RedirectToAction("Index", "Categories", new { showUpdatedCurrencyToastr = false });
		}

        [HttpPost]
        public IActionResult DeleteTransaction(int id)
        {
			ModelState.Remove("Categories");
			ModelState.Remove("Transactions");
			ModelState.Remove("CategorySelectList");
			ModelState.Remove("CurrencySelectList");
			ModelState.Remove("CurrentUser");
			ModelState.Remove("CurrencyObjectJson");
			if (!ModelState.IsValid)
			{
				TempData["error"] = "Something went wrong, your transaction wasn't deleted";

				return RedirectToAction("Index", "Transactions");
			}
            try
            {
				var transaction = _transactionService.GetTransaction(id);

				_transactionService.DeleteTransaction(transaction);
			}
            catch (Exception ex)
            {
				TempData["error"] = $"Something went wrong, your transaction wasn't deleted: {ex.Message}";

				return RedirectToAction("Index", "Transactions");
			}

			TempData["success"] = "Transaction deleted successfully";

			return RedirectToAction("Index", "Categories", new { showUpdatedCurrencyToastr = false });
		}
    }
}
