using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;

namespace cz.zk.OREGON
{
    class SqlProcessing
    {
        private string connectionString;
        private SqlConnection con, conpom;
        private String server, db, user, pwd;
        private cz.zk.OREGON.Form1.VoidIntString loggingFunc;
        private static String ErrorMessage;

        public SqlProcessing(String _server, String _db, String _user,
                             String _pwd, cz.zk.OREGON.Form1.VoidIntString _fun)
        {
            server = _server;
            db = _db;
            user = _user;
            pwd = _pwd;
            loggingFunc = _fun;

            connectionString = "server=" + server +
            ";database=" + db + ";uid=" + user + ";pwd=" + pwd;

            // Instantiate the connection, passing the
            // connection string into the constructor
            con = new SqlConnection(connectionString);
            con.InfoMessage += new SqlInfoMessageEventHandler(OnInfoMessage);

            connectionString = "server=" + server +
            ";database=master" + ";uid=" + user + ";pwd=" + pwd;
            conpom = new SqlConnection(connectionString);
            conpom.InfoMessage += new SqlInfoMessageEventHandler(OnInfoMessage);

            loggingFunc(Form1.LOGSRC_SQL, "Constructor Ready !");

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected static void OnInfoMessage(object sender, SqlInfoMessageEventArgs args)
        {
            foreach (SqlError err in args.Errors)
            {
                ErrorMessage = String.Format("{0}", err.Message);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <returns></returns>
        public DataTable ReadData(String Command, int Verbose)
        {
            DataTable newTable = new DataTable();
            DataColumn col;
            DataRow row;
            int i;
            SqlCommand cmd;
            SqlDataReader reader = null;


            if(Verbose != 0) loggingFunc(Form1.LOGSRC_SQL, String.Format("Query: [{0}]", Command));

            try
            {
                // Open the connection
                con.Open();
            }
            catch (Exception ex)
            {
                if (Verbose == 0)
                {
                    System.Windows.Forms.MessageBox.Show("Exception during SQL Open Connection occured,\nsee the logger window!");
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
                ProcessException(ex);
                return (newTable);
            }

            try
            {
                // Create and execute the query
                cmd = new SqlCommand("SET DATEFIRST 1 ", con);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand(Command, con);
                reader = cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                if (Verbose == 0)
                {
                    System.Windows.Forms.MessageBox.Show("Exception during SQL Query occured,\nsee the logger window!");
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
                ProcessException(ex);
                con.Close();
                return (newTable);
            }

            for (i = 0; i < reader.FieldCount; i++)
            {

                col = new DataColumn();
                col.ColumnName = reader.GetName(i);
                col.DataType = reader.GetFieldType(i);

                newTable.Columns.Add(col);
            }

            while (reader.Read())
            {
                row = newTable.NewRow();
                for (i = 0; i < reader.FieldCount; i++)
                {
                    row[i] = reader.GetValue(i);
                }
                newTable.Rows.Add(row);
            }
            con.Close();
            return (newTable);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sCmd"></param>
        public void ExecNonQuery(String sCmd)
        {
            try
            {
                SqlCommand cmd = new SqlCommand(sCmd, con);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            catch (SqlException ex)
            {
                MessageBox.Show("Exception during SQL NONQuery occured,\nsee the logger window!");
                ProcessException(ex);
            }

            finally
            {
                con.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sCmd"></param>
        public String ExecNonQueryWithInfo(String sCmd)
        {
            try
            {
                SqlCommand cmd = new SqlCommand(sCmd, con);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                return (ErrorMessage);
            }

            catch (SqlException ex)
            {
                MessageBox.Show("Exception during SQL NONQuery occured,\nsee the logger window!");
                ProcessException(ex);
                con.Close();
                return ("");
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sSource"></param>
        /// <returns></returns>
        public String RestoreDB(String sSource)
        {

            string sCmd;

            sCmd = string.Format("restore database INVEST from disk='{0}'", sSource);
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("alter database INVEST set offline with rollback immediate", con);
                cmd.ExecuteNonQuery();
                cmd = new SqlCommand(sCmd, con);
                cmd.ExecuteNonQuery();
                cmd = new SqlCommand("alter database INVEST set online with rollback immediate", con);
                cmd.ExecuteNonQuery();
                con.Close();

                connectionString = "server=" + server +
                ";database=" + db + ";uid=" + user + ";pwd=" + pwd;
                con = new SqlConnection(connectionString);
                con.InfoMessage += new SqlInfoMessageEventHandler(OnInfoMessage);

                return (ErrorMessage);
            }

            catch (SqlException ex)
            {
                MessageBox.Show("Exception during SQL RestoreDB occured,\nsee the logger window!");
                ProcessException(ex);
                con.Close();
                return ("");
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int ExecNonQueryNoException()
        {
            try
            {
                SqlCommand cmd = new SqlCommand("SELECT * from FONDY", con);
                con.Open();
                cmd.ExecuteReader();
                con.Close();
                return (0);
            }

            catch (SqlException ex)
            {
                ProcessException(ex);
                con.Close();
                return (1);
            }
        }



        /// <summary>
        /// Log exception related messages: the Message field does not contain newlines
        /// therefore we use shredding it into pieces of given maximum length (80)
        /// ald log line by line. The StackTrace field contains the newlines, so it
        /// it is enough to use the Split method of the String class
        /// </summary>
        /// <param name="ex"></param>
        private void ProcessException(Exception ex)
        {
            ZKStringUtils SU = new ZKStringUtils();
            ArrayList al = SU.DivideString(ex.Message, 80);

            foreach (Object o in al)
                loggingFunc(Form1.LOGSRC_SQL, String.Format("   EXCEPTION: [{0}]", (String)o));

            String[] Phrases = ex.StackTrace.Split('\n');
            foreach (String sent in Phrases)
                loggingFunc(Form1.LOGSRC_SQL, String.Format("   Stack:   [{0}]", sent));
        }
    }
}
