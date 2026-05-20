using IncTrak.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IncTrak.Domain
{
    public class ScheduleCalc
    {
        public static List<VestingPeriod> GetVestedShares(Grants grant)
        {
            decimal sharesRemaining = grant.Shares;
            int lastPeriod = grant.VestingScheduleFkNavigation.Periods.Max(p => p.Order);
            int order = 0;
            List<VestingPeriod> periods = new List<VestingPeriod>();
            foreach(var period in grant.VestingScheduleFkNavigation.Periods.OrderBy(p=>p.Order) )
            {
                int increments = period.Increments;
                decimal amount = period.Amount;
                if (lastPeriod == period.Order && ((EvenOverTypes)period.EvenOverN == EvenOverTypes.Amounts || (EvenOverTypes)period.EvenOverN == EvenOverTypes.Periods))
                {
                    if ((EvenOverTypes)period.EvenOverN == EvenOverTypes.Amounts)
                    {
                        amount = sharesRemaining / period.Increments;
                        if  ((AmountTypes)period.AmountTypeFk == AmountTypes.Percentage)
                        {
                            amount = amount / grant.Shares * 100;
                        }
                    }
                    else if  ((AmountTypes)period.AmountTypeFk == AmountTypes.Number)
                    {
                        increments = (int)(sharesRemaining / period.Amount);
                    }
                    else
                    {
                        increments = (int)(sharesRemaining / (grant.Shares / 100 * period.Amount));
                    }
                }

                for (int i = 0; i < increments; i++)
                {
                    if ( sharesRemaining > 0 )
                    {
                        VestingPeriod vest = new VestingPeriod();
                        if ((AmountTypes)period.AmountTypeFk == AmountTypes.Number)
                        {
                            vest.Shares = amount;
                        }
                        else
                        {
                            vest.Shares = (grant.Shares / 100 * amount);
                        }
                        if ( (sharesRemaining - vest.Shares) < 0 )
                        {
                            vest.Shares = sharesRemaining;
                            sharesRemaining = 0;
                        }
                        else
                        {
                            sharesRemaining -= vest.Shares;
                        }

                        vest.Percent = vest.Shares / grant.Shares * 100;
                        vest.Order = ++order;
                        vest.Period = (PeriodTypes)period.PeriodTypeFk;
                        vest.PeriodAmount = period.PeriodAmount;

                        periods.Add(vest);
                    }
                }
            }

            decimal lastOrder = periods.Max(p => p.Order);
            DateTime vestDate = grant.VestingStart;
            decimal totalPct = 0;
            decimal totalShares = 0;
            foreach(var vest in periods.OrderBy(p=>p.Order))
            {
                if (vest.Period == PeriodTypes.Years)
                    vestDate = vestDate.AddYears(vest.PeriodAmount);
                else if (vest.Period == PeriodTypes.Months)
                    vestDate = vestDate.AddMonths(vest.PeriodAmount);
                else if (vest.Period == PeriodTypes.Weeks)
                    vestDate = vestDate.AddDays(vest.PeriodAmount*7);
                else if (vest.Period == PeriodTypes.Days)
                    vestDate = vestDate.AddDays(vest.PeriodAmount);

                vest.VestDate = vestDate;
                vest.IsVested = vest.VestDate <= DateTime.Now;

                if (vest.Order == lastOrder)
                {
                    vest.Percent = 100 - totalPct;
                    if (sharesRemaining > 0)
                        vest.Shares += sharesRemaining;
                    vest.TotalPercent = 100;
                    vest.TotalShares = grant.Shares;
                }
                else
                {
                    totalPct += vest.Percent;
                    totalShares += vest.Shares;

                    vest.TotalPercent = totalPct;
                    vest.TotalShares = totalShares;
                }
            }

            return periods;
        }

        public static DateTime GetTerminationDate(Grants grant)
        {
            Terminations termination = grant.TerminationFkNavigation;
            if (termination.IsAbsolute || (TermFrom)termination.TermFromFkNavigation.TermFromPk == TermFrom.AbsoluteDate)
                return termination.AbsoluteDate;
            else
            {
                DateTime date =DateTime.Now;
                if ((TermFrom)termination.TermFromFkNavigation.TermFromPk == TermFrom.GrantDate)
                    date = grant.DateOfGrant;
                else if ((TermFrom)termination.TermFromFkNavigation.TermFromPk == TermFrom.VestStart)
                    date = grant.VestingStart;
                else if ((TermFrom)termination.TermFromFkNavigation.TermFromPk == TermFrom.SpecificDate)
                    date = termination.SpecificDate;

                if (termination.Years > 0)
                    date = date.AddYears(termination.Years);
                if (termination.Months > 0)
                    date = date.AddMonths(termination.Months);
                if (termination.Days > 0)
                    date = date.AddDays(termination.Days);

                return date;
            }
        }
     }
}