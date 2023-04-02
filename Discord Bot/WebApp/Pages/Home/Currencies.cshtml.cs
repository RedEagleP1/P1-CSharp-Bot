using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;

namespace WebApp.Pages.Home
{
    [Authorize(Policy = "Allowed")]
    public class CurrenciesModel : PageModel
    {
        public List<Currency> Currencies { get; set; }
        public bool IsCreatingNewCurrency { get; set; }
        ApplicationDbContext _db;
        public CurrenciesModel(ApplicationDbContext _db)
        {
            this._db = _db;
        }
        public void OnGet()
        {
            Currencies = _db.Currencies.ToList();
        }
        public void OnGetWithMessage(string message)
        {
            Currencies = _db.Currencies.ToList();
            ViewData["Message"] = message;
        }

        public void OnGetCreatingCurrency()
        {
            Currencies = _db.Currencies.ToList();
            IsCreatingNewCurrency = true;
        }

        public async Task<IActionResult> OnPostCreateNewCurrency(string currencyName)
        {
            string message;
            if(string.IsNullOrEmpty(currencyName))
            {
                message = "Currency Was Not Created. Name cannot be empty string.";
                return RedirectToPage("Currencies", "WithMessage", new { message });
            }

            if(await _db.Currencies.FirstOrDefaultAsync(c => c.Name == currencyName) != null)
            {
                message = "Currency Was Not Created. Name already exists.";
                return RedirectToPage("Currencies", "WithMessage", new { message });
            }

            _db.Currencies.Add(new Currency() { Name = currencyName });
            await _db.SaveChangesAsync();
            message = "Currency was created.";
            return RedirectToPage("Currencies", "WithMessage", new { message });
        }
    }
}
