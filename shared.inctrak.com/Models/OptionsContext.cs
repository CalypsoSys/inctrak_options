using IncTrak.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IncTrak.Models
{
    public class OptionsContext : inctrakContext
    {
        private AppSettings _settings;
        public OptionsContext(AppSettings settings)
        {
            _settings = settings;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseLazyLoadingProxies()
                    .UseNpgsql(_settings.GetIncTrakConnection());
            }
        }
    }
}
