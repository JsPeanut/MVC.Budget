using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace MVC.Budget.JsPeanut.Areas.Identity.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    [PersonalData]
    [Column(TypeName = "nvarchar(100)")]
    public string FirstName { get; set; }
    [PersonalData]
    [Column(TypeName = "nvarchar(100)")]
    public string LastName { get; set; }
    public string? CurrencyCode { get; set; } = null;
    public string? CurrencyNativeSymbol { get; set; } = null;
    public decimal FoodValue { get; set; } = 0;
    public decimal TransportationValue { get; set; } = 0;
    public decimal HousingValue { get; set; } = 0;
    public decimal UtilitiesValue { get; set; } = 0;
    public decimal SubscriptionsValue { get; set; } = 0;
    public decimal HealthcareValue { get; set; } = 0;
    public decimal ExpensesValue { get; set; } = 0;
    public decimal SavingsValue { get; set; } = 0;
    public decimal DebtPaymentValue { get; set; } = 0;
    public decimal MiscellaneousValue { get; set; } = 0;

}

