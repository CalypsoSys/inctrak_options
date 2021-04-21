using IncTrak.GoalSetter.Controllers;
using IncTrak.GoalSetter.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace IncTrak.GoalSetter.Models
{
    public class GoalsContext : inctrak_goalsContext
    {
        private AppSettings _settings;
        private Guid _groupKey;
        private Guid _userKey;
        private string _userName;

        public GoalsContext(AppSettings settings)
        {
            _settings = settings;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseLazyLoadingProxies()
                    .UseNpgsql(_settings.IncTrakGoalsConnection);
            }
        }

        public void SetContext(Users user)
        {
            _groupKey = user.GroupFk;
            _userKey = user.UserPk;
            if (user.GoogleLogon)
                _userName = user.EmailAddress;
            else
                _userName = user.UserName;
        }

        private void SetContext()
        {
            RunSet(string.Format("SET inctrak.group_key = '{0}';", _groupKey) );
            RunSet(string.Format("SET inctrak.user_key = '{0}';", _userKey));
            RunSet(string.Format("SET inctrak.user_name = '{0}';", _userName));
        }

        private void RunSet(string setCommand)
        {
            using (var command = this.Database.GetDbConnection().CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = setCommand;
                if (command.Connection.State == ConnectionState.Closed)
                    command.Connection.Open();
                command.ExecuteScalar();
            }

        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            SetContext();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override int SaveChanges()
        {
            SetContext();
            return base.SaveChanges();
        }
    }
}
