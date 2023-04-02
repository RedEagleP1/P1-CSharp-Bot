using Models;

namespace WebApp.Pages.Partials
{
    public class CurrencyDropdownPartialModel
    {
        public string ButtonName { get; set; }
        public IEnumerable<Currency> Currencies { get; set; }
        public bool AddNoneOption { get; set; }
    }
}
