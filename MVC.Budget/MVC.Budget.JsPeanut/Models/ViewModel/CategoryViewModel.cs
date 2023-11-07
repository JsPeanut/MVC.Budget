using Microsoft.AspNetCore.Mvc.Rendering;
using MVC.Budget.JsPeanut.Areas.Identity.Data;

namespace MVC.Budget.JsPeanut.Models.ViewModel
{
    public class CategoryViewModel
    {
        public List<Category> Categories { get; set; }
        public List<Transaction> Transactions { get; set; }
        public List<SelectListItem> CategorySelectList { get; set; }
        public List<SelectListItem> CurrencySelectList { get; set; }
        public ApplicationUser CurrentUser { get; set; }
        public string CurrencyObjectJson { get; set; }
        public string? SearchStringOne { get; set; }
        public string? SearchStringTwo { get; set; }
        public Transaction Transaction { get; set; }
    }
}
