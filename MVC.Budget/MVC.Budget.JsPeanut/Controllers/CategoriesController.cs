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
    public class CategoriesController : Controller
    {
        private readonly DataContext _context;
        private readonly CategoryService _categoryService;
        private readonly TransactionService _transactionService;
        private readonly JsonFileCurrencyService _jsonFileCurrencyService;
        private readonly CurrencyConverterService _currencyConverterService;
        private readonly UserManager<ApplicationUser> _userManager;
        public CategoriesController(DataContext context, CategoryService categoryService, TransactionService transactionService, JsonFileCurrencyService jsonFileCurrencyService, CurrencyConverterService currencyConverterService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _categoryService = categoryService;
            _transactionService = transactionService;
            _jsonFileCurrencyService = jsonFileCurrencyService;
            _currencyConverterService = currencyConverterService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string timeline = "", string searchStringOne = "", string searchStringTwo = "", bool showUpdatedCurrencyToastr = true)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

			var categories = _categoryService.GetAllCategories();
            var transactions = _transactionService.GetAllTransactions().Where(t => t.UserId == userId).ToList();

            if (!string.IsNullOrEmpty(timeline))
            {
                DateTime firstDayOfTheMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                DateTime lastDayOfTheMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));

                DateTime firstDayOfThreeMonthsAgoMonth = new DateTime(DateTime.Now.AddMonths(-3).Year, DateTime.Now.AddMonths(-3).Month, 1);
                DateTime lastDayOfThreeMonthsAgoMonth = new DateTime(DateTime.Now.AddMonths(-3).Year, DateTime.Now.AddMonths(-3).Month, DateTime.DaysInMonth(DateTime.Now.AddMonths(-3).Year, DateTime.Now.AddMonths(-3).Month));

                DateTime firstDayOfSixMonthsAgoMonth = new DateTime(DateTime.Now.AddMonths(-6).Year, DateTime.Now.AddMonths(-6).Month, 1);
                DateTime lastDayOfSixMonthsAgoMonth = new DateTime(DateTime.Now.AddMonths(-6).Year, DateTime.Now.AddMonths(-6).Month, DateTime.DaysInMonth(DateTime.Now.AddMonths(-6).Year, DateTime.Now.AddMonths(-6).Month));

                DateTime firstDayOfTLastYearMonth = new DateTime(DateTime.Now.AddYears(-1).Year, DateTime.Now.AddYears(-1).Month, 1);
                DateTime lastDayOfLastYearMonth = new DateTime(DateTime.Now.AddYears(-1).Year, DateTime.Now.AddYears(-1).Month, DateTime.DaysInMonth(DateTime.Now.AddYears(-1).Year, DateTime.Now.AddYears(-1).Month));
                switch (timeline)
                {
                    case "LastMonth":
                        transactions = transactions = transactions.Where(x => x.Date.Date >= firstDayOfTheMonth && x.Date.Date <= lastDayOfTheMonth).ToList();
                        break;
                    case "LastThreeMonths":
                        transactions = transactions = transactions.Where(x => x.Date.Date >= firstDayOfThreeMonthsAgoMonth && x.Date.Date <= lastDayOfTheMonth).ToList();
                        break;
                    case "LastSixMonths":
                        transactions = transactions = transactions.Where(x => x.Date.Date >= firstDayOfSixMonthsAgoMonth && x.Date.Date <= lastDayOfTheMonth).ToList();
                        break;
                    case "LastYear":
                        transactions = transactions = transactions.Where(x => x.Date.Date >= firstDayOfTLastYearMonth && x.Date.Date <= lastDayOfTheMonth).ToList();
                        break;
                    case "default":
                        transactions = _transactionService.GetAllTransactions();
                        break;
                }
            }
            if (!string.IsNullOrEmpty(searchStringOne) && !string.IsNullOrEmpty(searchStringTwo))
            {
                DateTime searchDateOne;
                DateTime searchDateTwo;
                if (DateTime.TryParse(searchStringOne, out searchDateOne) && DateTime.TryParse(searchStringTwo, out searchDateTwo))
                {
                    transactions = transactions.Where(x => x.Date.Date >= searchDateOne.Date && x.Date.Date <= searchDateTwo.Date).ToList();
                }
            }
            var currencies = _jsonFileCurrencyService.GetCurrencyList();
            var categoryselectlist_ = new List<SelectListItem>();
            var currencyselectlist_ = new List<SelectListItem>();
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
                    Value = JsonSerializer.Serialize<Currency>(currency)
                });
            }
            var categoriesviewmodel = new CategoryViewModel
            {
                Categories = categories,
                CategorySelectList = categoryselectlist_,
                CurrencySelectList = currencyselectlist_,
                Transactions = transactions
            };
            Currency currencyObject = new Currency
            {
                CurrencyCode = user.CurrencyCode,
                NativeSymbol = user.CurrencyNativeSymbol,
                Name = string.Empty
            };
            ViewBag.Currency = currencyObject.CurrencyCode;
            var currencyObjectJson = JsonSerializer.Serialize<Currency>(currencyObject);
            if (!showUpdatedCurrencyToastr)
            {
				await UpdateCurrency(currencyObjectJson, "crud", transactions);
			}
            else if (showUpdatedCurrencyToastr)
            {
				await UpdateCurrency(currencyObjectJson, "update", transactions);
			}

            return View(categoriesviewmodel);
        }

        //updateOrCrud parameter is used so that if the method is called after performing a CUD operation (to update category values), no notifications about currency updating are shown
        [HttpPost]
        public async Task<IActionResult> UpdateCurrency(string selectedCurrency, string updateOrCrud, List<Transaction> sortedTransactions = null)
        {
            try
            {
				var userId = _userManager.GetUserId(User);
				var user = await _userManager.FindByIdAsync(userId);

				if (user == null)
				{
					return RedirectToPage("/Account/Login", new { area = "Identity" });
				}

				var categories = _categoryService.GetAllCategories();
				var transactions = sortedTransactions ?? _transactionService.GetAllTransactions();

				var selectedCurrencyOption = JsonSerializer.Deserialize<Currency>(selectedCurrency);
				foreach (var category in categories)
				{
					user.CurrencyCode = selectedCurrencyOption.CurrencyCode;
					user.CurrencyNativeSymbol = selectedCurrencyOption.NativeSymbol;

					decimal totalValue = 0;

					var transactionsWhereCategoryIsEqualToLoopsCategory = transactions.Where(x => x.CategoryId == category.Id && x.UserId == userId);
					foreach (var transaction in transactionsWhereCategoryIsEqualToLoopsCategory)
					{
						if (transaction.CurrencyCode == user.CurrencyCode)
						{
							totalValue += transaction.Value;
						}
						else
						{
							decimal conversionResult = _currencyConverterService.ConvertValueToCategoryCurrency(transaction.CurrencyCode, transaction.Value, user.CurrencyCode);

							totalValue += conversionResult;
						}
					}

					totalValue = Decimal.Round(totalValue, 2);

					await ChangeUserCategoryValue(user, category.Name, totalValue);

					_categoryService.UpdateCategory(category);
				}
			}
            catch (Exception ex)
            {
                TempData["error"] = $"Something went wrong while trying to update your default currency. Error: {ex.Message}";

				return RedirectToAction("Index", "Categories", new {showUpdatedCurrencyToastr = false});
			}

            if (updateOrCrud == "update" && ViewBag.Currency != null)
            {
                TempData["success"] = "Your default currency was updated successfully!";
			}
            
            return Redirect("https://localhost:7229");
        }

        public async Task<IActionResult> AddTransaction(Models.TransactionInputModel transactionInputModel, CategoryViewModel cvm)
        {
            ModelState.Remove("Categories");
			ModelState.Remove("Transactions");
			ModelState.Remove("CategorySelectList");
			ModelState.Remove("CurrencySelectList");
			ModelState.Remove("CurrentUser");
			ModelState.Remove("CurrencyObjectJson");
			if (!ModelState.IsValid)
            {
                TempData["error"] = "Something went wrong, your transaction wasn't added: Model state is not valid";

				return RedirectToAction("Index", "Categories", new { showUpdatedCurrencyToastr = false });
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

                var userId = transaction.UserId;
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }

                decimal transactionValue = decimal.Parse(transaction.Value.ToString());

                var categories = _categoryService.GetAllCategories();
                var transactions = _transactionService.GetAllTransactions();

                var transactionCategory = categories.Where(c => c.Id == transaction.CategoryId).First();

                var selectedCurrencyOption = JsonSerializer.Deserialize<Currency>(cvm.CurrencyObjectJson);

                transaction.CurrencyCode = selectedCurrencyOption.CurrencyCode;
                transaction.CurrencyNativeSymbol = selectedCurrencyOption.NativeSymbol;

                _transactionService.AddTransaction(transaction);

                await ChangeUserCategoryValue(user, transactionCategory.Name, transactionValue);

                user = await _userManager.FindByIdAsync(userId);

                Currency currencyObject = new Currency
                {
                    CurrencyCode = user.CurrencyCode,
                    NativeSymbol = user.CurrencyNativeSymbol
                };

                string currencyJson = JsonSerializer.Serialize(currencyObject);

                await UpdateCurrency(currencyJson, "crud");
            }
            catch (Exception ex)
            {
				TempData["error"] = $"Something went wrong, your transaction wasn't added. Error: {ex.Message}";

				return RedirectToAction("Index", "Categories", new { showUpdatedCurrencyToastr = false });
			}

            TempData["success"] = "Transaction added successfully!";

			return RedirectToAction("Index", "Categories", new { showUpdatedCurrencyToastr = false });
		}

		public async Task ChangeUserCategoryValue(ApplicationUser user, string categoryName, decimal transactionValue)
        {
            switch (categoryName)
            {
                case "Food":
                    user.FoodValue = transactionValue;
                    break;
                case "Transportation":
                    user.TransportationValue = transactionValue;
                    break;
                case "Housing":
                    user.HousingValue = transactionValue;
                    break;
                case "Utilities":
                    user.UtilitiesValue = transactionValue;
                    break;
                case "Subscriptions":
                    user.SubscriptionsValue = transactionValue;
                    break;
                case "Healthcare":
                    user.HealthcareValue = transactionValue;
                    break;
                case "Personal expenses":
                    user.ExpensesValue = transactionValue;
                    break;
                case "Savings and investments":
                    user.SavingsValue = transactionValue;
                    break;
                case "Debt payment":
                    user.DebtPaymentValue = transactionValue;
                    break;
                case "Miscellaneous expenses":
                    user.MiscellaneousValue = transactionValue;
                    break;
            }
            await _userManager.UpdateAsync(user);
        }
    }
}
