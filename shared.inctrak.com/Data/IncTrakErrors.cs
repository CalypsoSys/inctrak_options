using IncTrak.Controllers;
using IncTrak.Domain;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace IncTrak.Data
{
    class IncTrakErrors : DataAccess
    {
        protected IncTrakErrors(AppSettings settings) : base(settings)
        {
        }

        protected override NpgsqlConnection Conn()
        {
            var connStr = new NpgsqlConnectionStringBuilder();
            connStr.Host = _settings.GetErrorsHost();
            connStr.Database = "inctrak_errors";
            connStr.Username = _settings.GetErrorsUsername();
            connStr.Password = _settings.GetErrorsPassword();
            var conn = new NpgsqlConnection();
            conn.ConnectionString = connStr.ConnectionString;

            return conn;
        }

        public static string LogError(AppSettings settings, LoginRights login, Exception excp, string message, params object[] args)
        {
            string errorCode = Base36.NumberToBase36(DateTime.Now.Ticks);
            try
            {
                string messageOut = "N/A";
                try
                {
                    messageOut = string.Format(message, args);
                }
                catch
                {
                    if (string.IsNullOrWhiteSpace(message) == false)
                        messageOut = message;

                }
                string excption = "N/A";
                try
                {
                    StringBuilder output = new StringBuilder();
                    for (int i = 2; excp != null; i++)
                    {
                        string indent = string.Empty.PadLeft(i, '\t');
                        output.AppendFormat("{0}Exception: {1}\r\n", indent, excp.Message);
                        output.AppendFormat("{0}Stack Trace: {1}\r\n", indent, excp.StackTrace.Replace("\r\n", string.Format("\r\n{0}", indent)));
                        excp = excp.InnerException;
                    }
                    output.AppendLine();
                    excption = output.ToString();
                }
                catch
                {
                    if (excp != null)
                        excption = excp.ToString();
                }
                string uuid = "N/A";
                Guid userKey = Guid.Empty;
                if (login != null)
                {
                    if (login.UUID != null)
                        uuid = login.UUID;
                    if (login.UserKeyForError != Guid.Empty)
                        userKey = login.UserKeyForError;
                }

                using (DataAccess access = new IncTrakErrors(settings))
                {
                    var eventParams = new NpgsqlParameter[]
                        {
                            new NpgsqlParameter("@MESSAGE", messageOut),
                            new NpgsqlParameter("@CALL_STACK", excption),
                            new NpgsqlParameter("@UUID", uuid),
                            new NpgsqlParameter("@USER_FK", userKey),
                            new NpgsqlParameter("@CODE", errorCode),
                        };
                    int check = access.ExecuteNonQuery("INSERT INTO OPTIONEEPLAN_ERRORS (MESSAGE, CALL_STACK, UUID, USER_FK, CODE) VALUES (@MESSAGE, @CALL_STACK, @UUID, @USER_FK, @CODE)", eventParams);
                }
            }
            catch(Exception excp1)
            {
                // do no harm
            }

            return string.Format("Unknown error occured code: [{0}], try again?", errorCode);
        }
    }
}
