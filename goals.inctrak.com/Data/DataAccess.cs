using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IncTrak.GoalSetter.Data
{
    abstract class DataAccess : IDisposable
    {
        protected AppSettings _settings;
        protected abstract NpgsqlConnection Conn();

        protected DataAccess(AppSettings settings)
        {
            _settings = settings;
        }

        void IDisposable.Dispose()
        {
        }

        private void AttachParameters(IDbCommand command, NpgsqlParameter[] commandParameters)
        {
            foreach (NpgsqlParameter p in commandParameters)
            {
                if ((p.Value == null))
                {
                    p.Value = DBNull.Value;
                }

                command.Parameters.Add(p);
            }
        }

        private void PrepareCommand(IDbConnection connection, IDbCommand command, CommandType commandType, string commandText, NpgsqlParameter[] commandParameters)
        {
            //if the provided connection is not open, we will open it
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            //associate the connection with the command
            command.Connection = connection;

            //set the command text (stored procedure name or SQL statement)
            command.CommandText = commandText;

            //set the command type
            command.CommandType = commandType;

            //attach the command parameters if they are provided
            if (commandParameters != null)
            {
                AttachParameters(command, commandParameters);
            }

            command.CommandTimeout = 600;

            return;
        }

        private int ExecuteNonQuery(IDbConnection connection, CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
        {
            //call the overload that takes a connection in place of the connection string
            //create a command and prepare it for execution
            NpgsqlCommand cmd = new NpgsqlCommand();
            PrepareCommand(connection, cmd, commandType, commandText, commandParameters);

            //finally, execute the command.
            int retval = cmd.ExecuteNonQuery();

            // detach the SqlParameters from the command object, so they can be used again.
            cmd.Parameters.Clear();
            return retval;
        }

        private DataSet ExecuteDataset(IDbConnection connection, CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
        {
            //call the overload that takes a connection in place of the connection string
            //create a command and prepare it for execution
            NpgsqlCommand cmd = new NpgsqlCommand();
            PrepareCommand(connection, cmd, commandType, commandText, commandParameters);

            //create the DataAdapter & DataSet
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd);
            DataSet ds = new DataSet();

            //fill the DataSet using default values for DataTable names, etc.
            da.Fill(ds);

            // detach the SqlParameters from the command object, so they can be used again.			
            cmd.Parameters.Clear();

            //return the dataset
            return ds;
        }

        public int ExecuteNonQuery(string commandText, params NpgsqlParameter[] commandParameters)
        {
            using (NpgsqlConnection conn = Conn())
            {
                return ExecuteNonQuery(conn, CommandType.Text, commandText, commandParameters);
            }
        }

        public DataSet ExecuteDataset(string commandText, params NpgsqlParameter[] commandParameters)
        {
            using (NpgsqlConnection conn = Conn())
            {
                return ExecuteDataset(conn, CommandType.Text, commandText, commandParameters);
            }
        }

        public void ExecuteAll(string file)
        {
            using (StreamReader input = new StreamReader(file))
            {
                string commandText = input.ReadToEnd();
                string[] commands = CreateCommands(commandText);
                using (NpgsqlConnection conn = Conn())
                {
                    foreach (string command in commands)
                    {
                        ExecuteNonQuery(conn, CommandType.Text, command);
                    }
                }
            }
        }

        private string[] CreateCommands(string commandText)
        {
            List<string> commands = new List<string>();
            string[] parts = commandText.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder command = new StringBuilder();
            foreach (string part in parts)
            {
                if (part.Trim().Equals("GO", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (command.Length > 0)
                    {
                        commands.Add(command.ToString());
                        command.Length = 0;
                    }
                }
                else
                {
                    command.AppendFormat("{0}\r\n", part);
                }
            }
            if (command.Length > 0)
            {
                commands.Add(command.ToString());
            }

            return commands.ToArray();
        }
    }
}
