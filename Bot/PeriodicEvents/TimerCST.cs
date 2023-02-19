using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.PeriodicEvents
{
    internal class TimerCST
    {
        DBContextFactory dbContextFactory;
        Timer? timer;
        public TimerCST(DBContextFactory dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;          
        }

        public void EnablePeriodicEvents()
        {
            var currentTimeCST = DateTime.UtcNow.AddHours(-6);
            var after1Day = currentTimeCST.AddDays(1);
            var startOfNextDay = new DateTime(after1Day.Year, after1Day.Month, after1Day.Day, 0, 0, 0);
            var timeLeftBeforeNewDay = startOfNextDay - currentTimeCST;
            timer = new Timer(DailyEvents, null, timeLeftBeforeNewDay, TimeSpan.FromHours(24));
        }
        async void DailyEvents(object? c)
        {
            await RefreshCurrencyAwardLimit();
        }

        //Be very careful. DO NOT change CurrencyAwarders to CurrencyOwners by mistake. Otherwise everyone will lose all their currencies.
        async Task RefreshCurrencyAwardLimit()
        {
            using var context = dbContextFactory.GetNewContext();
            foreach(var awarder in context.CurrencyAwarders)
            {
                context.CurrencyAwarders.Remove(awarder);
            }
            await context.SaveChangesAsync();
        }
    }
}
