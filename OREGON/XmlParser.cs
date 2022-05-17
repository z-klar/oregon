using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace cz.zk.OREGON
{
    class XmlParser
    {
        private cz.zk.OREGON.Form1.VoidIntString funcLogging;

        private String Server = "", Db = "", User = "", Pwd = "";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_funcLog"></param>
        public XmlParser(cz.zk.OREGON.Form1.VoidIntString _funcLog)
        {
            funcLogging = _funcLog;

            funcLogging(Form1.LOGSRC_XML, "Constructor ready !");
        }

        /// <summary>
        /// 
        /// </summary>
        public void ReadConfig()
        {

            const String filename = "c:\\zdenda\\oregon.xml";

            XmlTextReader reader = null;

            try
            {
                // Load the reader with the data file and ignore all white space nodes.         
                reader = new XmlTextReader(filename);
                reader.WhitespaceHandling = WhitespaceHandling.None;

                // Parse the file and display each of the nodes.
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "ServerName":
                                reader.Read();
                                Server = reader.Value;
                                break;
                            case "DatabaseName":
                                reader.Read();
                                Db = reader.Value;
                                break;
                            case "User":
                                reader.Read();
                                User = reader.Value;
                                break;
                            case "PWD":
                                reader.Read();
                                Pwd = reader.Value;
                                break;
                        }
                    }
                }
                funcLogging(Form1.LOGSRC_XML, String.Format("Server: {0}", Server));
                funcLogging(Form1.LOGSRC_XML, String.Format("Database: {0}", Db));
                funcLogging(Form1.LOGSRC_XML, String.Format("User: {0}", User));
                funcLogging(Form1.LOGSRC_XML, String.Format("PWD: {0}", "xxxxxxxxxxxxxx"));
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Exception during XML parsing occured,\nsee the logger window!");
                funcLogging(Form1.LOGSRC_XML, String.Format("   EXC: {0}", ex.Message));

            }

            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }
        public String GetServerName()
        {
            return (Server);
        }

        public String GetDbName()
        {
            return (Db);
        }
        public String GetUserName()
        {
            return (User);
        }
        public String GetPwd()
        {
            return (Pwd);
        }

    }
}
