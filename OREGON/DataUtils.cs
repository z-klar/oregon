using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace cz.zk.OREGON
{
    class DataUtils
    {
        private SqlProcessing _SP;

        public DataUtils(SqlProcessing SP)
        {
            _SP = SP;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SP"></param>
        /// <param name="Mode"> 0 = KM, 1 = HOURS </param>
        /// <returns></returns>
        public DataTable GetYearlyTypeOverview(int Mode, int nVerbose)
        {
            DataTable dtYears, dtTypes, dtPom, dtRes;
            int i, j;
            String sCmd;
            object obj;

            dtYears = _SP.ReadData("SELECT DISTINCT YEAR(DATUM) from ITEMS", nVerbose);
            dtTypes = _SP.ReadData("SELECT * from TYPES", nVerbose);

            dtRes = new DataTable("dtRes");
            dtRes.Columns.Add("Rok", typeof(int));

            for (i = 0; i < dtYears.Rows.Count; i++)
                dtRes.Rows.Add(new object[] { dtYears.Rows[i].ItemArray[0] });

            foreach (DataRow row in dtTypes.Rows)
            {
                dtRes.Columns.Add(row.ItemArray[1].ToString(), typeof(double));

            }

           
            for (i = 0; i < dtYears.Rows.Count; i++)
            {
                for (j = 0; j < dtTypes.Rows.Count; j++)
                {
                    if (Mode == 0)
                    {
                        sCmd = String.Format("SELECT SUM(DISTANCE) from ITEMS where ((YEAR(DATUM) = {0}) AND (TYP = {1}))",
                                            dtYears.Rows[i].ItemArray[0], dtTypes.Rows[j].ItemArray[0]);
                    }
                    else {
                        sCmd = String.Format("SELECT SUM(DURATION)/60.0 from ITEMS where ((YEAR(DATUM) = {0}) AND (TYP = {1}))",
                                            dtYears.Rows[i].ItemArray[0], dtTypes.Rows[j].ItemArray[0]);
                    }

                    dtPom = _SP.ReadData(sCmd, nVerbose);
                    obj = dtPom.Rows[0].ItemArray[0];
                    if (!DBNull.Value.Equals(obj))
                    {
                        dtRes.Rows[i][j + 1] = dtPom.Rows[0].ItemArray[0];
                        //dtRes.Rows[i].ItemArray[j + 1] = 123;
                    }
                    else {
                        dtRes.Rows[i][j + 1] = 0;
                    }
                }
            }
            return(dtRes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SP"></param>
        /// <param name="Mode"></param>
        /// <param name="Year"></param>
        /// <returns></returns>
        public DataTable GetMonthlyTypeOverview(int Mode, String Year, int nVerbose)
        {
            DataTable dtMonths, dtTypes, dtPom, dtRes;
            int i, j, k, NoTypes;
            String sCmd;
            object obj;
            DataRow dtRow;

            dtMonths = _SP.ReadData("SELECT DISTINCT MONTH(DATUM) from ITEMS where (YEAR(DATUM)=" + Year + ")", nVerbose);
            dtTypes = _SP.ReadData("SELECT * from TYPES", nVerbose);
            NoTypes = dtTypes.Rows.Count;

            dtRes = new DataTable("dtRes");
            dtRes.Columns.Add("Mesic", typeof(int));

            for (i = 0; i < dtMonths.Rows.Count; i++)
                dtRes.Rows.Add(new object[] { dtMonths.Rows[i].ItemArray[0] });

            foreach (DataRow row in dtTypes.Rows)
            {
                dtRes.Columns.Add(row.ItemArray[1].ToString(), typeof(double));

            }

            for (i = 0; i < dtMonths.Rows.Count; i++)
            {
                for (j = 0; j < dtTypes.Rows.Count; j++)
                {
                    if (Mode == 0)
                    {
                        sCmd = String.Format("SELECT SUM(DISTANCE) from ITEMS where ((MONTH(DATUM) = {0}) AND (TYP = {1})",
                                            dtMonths.Rows[i].ItemArray[0], dtTypes.Rows[j].ItemArray[0]);
                    }
                    else if(Mode == 1)
                    {
                        sCmd = String.Format("SELECT SUM(DURATION)/60.0 from ITEMS where ((MONTH(DATUM) = {0}) AND (TYP = {1})",
                                            dtMonths.Rows[i].ItemArray[0], dtTypes.Rows[j].ItemArray[0]);
                    }
                    else
                    {
                        sCmd = String.Format("SELECT AVG(WEIGHT) from ITEMS where ((MONTH(DATUM) = {0}) AND (TYP = {1})",
                                            dtMonths.Rows[i].ItemArray[0], dtTypes.Rows[j].ItemArray[0]);
                    }
                    sCmd += " AND (YEAR(DATUM) = " + Year + "))";

                    dtPom = _SP.ReadData(sCmd, nVerbose);
                    obj = dtPom.Rows[0].ItemArray[0];
                    if (!DBNull.Value.Equals(obj))
                    {
                        dtRes.Rows[i][j + 1] = dtPom.Rows[0].ItemArray[0];
                    }
                    else
                    {
                        dtRes.Rows[i][j + 1] = 0;
                    }
                }
            }

            dtPom = dtRes.Clone();
            for (i = 0; i < 12; i++)
            {
                j = FindMonth(dtRes, i + 1);
                if (j == -1)
                {
                    dtRow = dtPom.NewRow();
                    dtRow[0] = i + 1;
                    for (k = 0; k < NoTypes; k++) dtRow[k + 1] = 0;
                    dtPom.Rows.Add(dtRow);
                }
                else dtPom.ImportRow(dtRes.Rows[j]);
            }
            return (dtPom);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="week"></param>
        /// <returns></returns>
        private int FindMonth(DataTable table, int month)
        {
            int j = 0;

            foreach (DataRow row in table.Rows)
            {
                if (Convert.ToInt32(row.ItemArray[0]) == month)
                    return (j);
                j++;
            }
            return (-1);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="Mode"></param>
        /// <param name="Year"></param>
        /// <param name="nVerbose"></param>
        /// <returns></returns>
        public DataTable GetWeeklyTypeOverview(int Mode, String Year, int nVerbose)
        {
            DataTable dtMonths, dtTypes, dtPom, dtRes;
            int i, j, k, NoTypes, PocetRadek = 0;
            String sCmd;
            object obj;
            DataRow dtRow;

            dtMonths = _SP.ReadData("SELECT DISTINCT DATEPART(WEEK, DATUM) from ITEMS where (YEAR(DATUM)=" + Year + ")", nVerbose);
            dtTypes = _SP.ReadData("SELECT * from TYPES", nVerbose);
            NoTypes = dtTypes.Rows.Count;

            dtRes = new DataTable("dtRes");
            dtRes.Columns.Add("CW", typeof(int));

            for (i = 0; i < dtMonths.Rows.Count; i++)
                dtRes.Rows.Add(new object[] { dtMonths.Rows[i].ItemArray[0] });

            foreach (DataRow row in dtTypes.Rows)
            {
                dtRes.Columns.Add(row.ItemArray[1].ToString(), typeof(double));

            }

            for (i = 0; i < dtMonths.Rows.Count; i++)
            {
                for (j = 0; j < dtTypes.Rows.Count; j++)
                {
                    if (Mode == 0)
                    {
                        sCmd = String.Format("SELECT SUM(DISTANCE) from ITEMS where ((DATEPART(WEEK, DATUM) = {0}) AND (TYP = {1})",
                                            dtMonths.Rows[i].ItemArray[0], dtTypes.Rows[j].ItemArray[0]);
                    }
                    else if (Mode == 1)
                    {
                        sCmd = String.Format("SELECT SUM(DURATION)/60.0 from ITEMS where ((DATEPART(WEEK, DATUM) = {0}) AND (TYP = {1})",
                                            dtMonths.Rows[i].ItemArray[0], dtTypes.Rows[j].ItemArray[0]);
                    }
                    else
                    {
                        sCmd = String.Format("SELECT AVG(WEIGHT) from ITEMS where ((DATEPART(WEEK, DATUM) = {0}) AND (TYP = {1})",
                                            dtMonths.Rows[i].ItemArray[0], dtTypes.Rows[j].ItemArray[0]);
                    }
                    sCmd += " AND (YEAR(DATUM) = " + Year + "))";

                    dtPom = _SP.ReadData(sCmd, nVerbose);
                    obj = dtPom.Rows[0].ItemArray[0];
                    if (!DBNull.Value.Equals(obj))
                    {
                        dtRes.Rows[i][j + 1] = dtPom.Rows[0].ItemArray[0];
                    }
                    else
                    {
                        dtRes.Rows[i][j + 1] = 0;
                    }
                }
            }

            dtPom = dtRes.Clone();
            for (i = 0; i < 52; i++)
            {
                PocetRadek = dtPom.Rows.Count;
                j = FindWeek(dtRes, i + 1);
                if (j == -1)
                {
                    dtRow = dtPom.NewRow();
                    dtRow[0] = i + 1;
                    for (k = 0; k < NoTypes; k++) dtRow[k+1] = 0;
                    dtPom.Rows.Add(dtRow);
                }
                else dtPom.ImportRow(dtRes.Rows[j]);
            }
            return (dtPom);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="week"></param>
        /// <returns></returns>
        private int FindWeek(DataTable table, int week)
        {
            int j = 0;

            foreach (DataRow row in table.Rows)
            {
                if (Convert.ToInt32(row.ItemArray[0]) == week)
                    return (j);
                j++;
            }
            return (-1);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="nLogujSql"></param>
        /// <param name="FilterYersDetail"></param>
        /// <param name="Year"></param>
        /// <param name="FilterYearTypesDetail"></param>
        /// <param name="Type"></param>
        /// <param name="FilterStartEnd"></param>
        /// <param name="Start"></param>
        /// <param name="End"></param>
        /// <returns></returns>
        public DataTable GetDetailesSummary(int nVerbose, bool FilterYersDetail, string Year,
                                            bool FilterTypesDetail, int Type,
                                            bool FilterStartEnd, DateTime Start, DateTime End) {

            DataColumn col; 
            DataTable newTable = new DataTable();
            DataTable dtPom;
            String sCommonCmd, sCmd;
            int nPom = 0;
            double dPom;

            col = new DataColumn(); 
            col.ColumnName = "Param"; 
            col.DataType = typeof(String);
            newTable.Columns.Add(col);
            object obj;

            col = new DataColumn();
            col.ColumnName = "Value";
            col.DataType = typeof(String);
            newTable.Columns.Add(col);

            sCommonCmd = "";
            if (FilterYersDetail == true)
            {
                nPom = 1;
                sCommonCmd += String.Format(" where (YEAR(DATUM) = {0})", Year);
            }

            if (FilterTypesDetail == true)
            {
                if (nPom == 1)
                    sCommonCmd += " AND ";
                else
                {
                    sCommonCmd += " WHERE ";
                    nPom = 1;
                }
                sCommonCmd += string.Format(" (ITEMS.TYP = {0})", Type);
            }

            if (FilterStartEnd == true)
            {
                if (nPom == 1)
                    sCommonCmd += " AND ";
                else
                {
                    sCommonCmd += " WHERE ";
                    nPom = 1;
                }
                sCommonCmd += string.Format("(DATUM>='{0:s}') and (DATUM<='{1:s}')", Start, End);
            }



            sCmd = "SELECT SUM(DURATION)/60.0 from ITEMS ";
            dtPom = _SP.ReadData(sCmd + sCommonCmd, nVerbose);
            obj = dtPom.Rows[0].ItemArray[0]; 
            if (!DBNull.Value.Equals(obj))
                dPom = double.Parse(dtPom.Rows[0].ItemArray[0].ToString());
            else
                dPom = 0.0;
            newTable.Rows.Add("Sum Time [Hr.]", string.Format("{0:F1}", dPom));

            sCmd = "SELECT SUM(DISTANCE) from ITEMS ";
            dtPom = _SP.ReadData(sCmd + sCommonCmd, nVerbose);
            obj = dtPom.Rows[0].ItemArray[0];
            if (!DBNull.Value.Equals(obj))
                dPom = double.Parse(dtPom.Rows[0].ItemArray[0].ToString());
            else
                dPom = 0.0;
            newTable.Rows.Add("Sum Len [km]", string.Format("{0:F0}", dPom));

            sCmd = "SELECT AVG(AVG_SPEED) from ITEMS ";
            dtPom = _SP.ReadData(sCmd + sCommonCmd, nVerbose);
            obj = dtPom.Rows[0].ItemArray[0];
            if (!DBNull.Value.Equals(obj))
                dPom = double.Parse(dtPom.Rows[0].ItemArray[0].ToString());
            else
                dPom = 0.0;
            newTable.Rows.Add("Avg Speed", string.Format("{0:F1}", dPom));

            sCmd = "SELECT MAX(MAX_SPEED) from ITEMS ";
            dtPom = _SP.ReadData(sCmd + sCommonCmd, nVerbose);
            obj = dtPom.Rows[0].ItemArray[0];
            if (!DBNull.Value.Equals(obj))
                dPom = double.Parse(dtPom.Rows[0].ItemArray[0].ToString());
            else
                dPom = 0.0;
            newTable.Rows.Add("Max Speed", string.Format("{0:F1}", dPom));

            sCmd = "SELECT AVG(WEIGHT) from ITEMS ";
            dtPom = _SP.ReadData(sCmd + sCommonCmd, nVerbose);
            obj = dtPom.Rows[0].ItemArray[0];
            if (!DBNull.Value.Equals(obj))
                dPom = double.Parse(dtPom.Rows[0].ItemArray[0].ToString());
            else
                dPom = 0.0;
            newTable.Rows.Add("AVG Weight", string.Format("{0:F1}", dPom));

            sCmd = "SELECT SUM(SUMUP) from ITEMS ";
            dtPom = _SP.ReadData(sCmd + sCommonCmd, nVerbose);
            obj = dtPom.Rows[0].ItemArray[0];
            if (!DBNull.Value.Equals(obj))
                dPom = double.Parse(dtPom.Rows[0].ItemArray[0].ToString());
            else
                dPom = 0.0;
            newTable.Rows.Add("SUM UpHill", string.Format("{0:F0}", dPom));

            sCmd = "SELECT COUNT(ID) from ITEMS ";
            dtPom = _SP.ReadData(sCmd + sCommonCmd, nVerbose);
            obj = dtPom.Rows[0].ItemArray[0];
            if (!DBNull.Value.Equals(obj))
                dPom = double.Parse(dtPom.Rows[0].ItemArray[0].ToString());
            else
                dPom = 0.0;
            newTable.Rows.Add("No of.Recs", string.Format("{0:F0}", dPom));




            return (newTable);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nVerbose"></param>
        /// <param name="FilterYersDetail"></param>
        /// <param name="Year"></param>
        /// <param name="FilterTypesDetail"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public DataTable GetTableSummary(int nVerbose, bool FilterYersDetail, string Year,
                                            bool FilterTypesDetail, int Type)
        {

            DataColumn col;
            DataTable newTable = new DataTable();
            DataTable dtPom, dtYears;
            String sCmd;
            int i, nMon;
            object obj;
            double dDist, dDur, dAvg, dMax, dWeight, dUphill, dCount;

            String[] MonthNames = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            String[] ColNamesAll = { "Year", "Dist [km]", "Dur [Hrs]", "Avg.Speed", "Max.Speed", "Weight", "UpHill", "Rec.No" };
            String[] ColNamesYear = { "Month", "Dist [km]", "Dur [Hrs]", "Avg.Speed", "Max.Speed", "Weight", "UpHill", "Rec.No" };
            Type[] Types = { typeof(String), typeof(double), typeof(double), typeof(double), typeof(double), 
                             //  Date            Dis            Dur             AVG               Max  
                               typeof(double), typeof(double), typeof(double) };
                             //    Weight           Uphill          Count


            //---------------------------------------------------------------------------------------
            // monthly overview per given year
            //---------------------------------------------------------------------------------------
            if (FilterYersDetail == true)
            {

                for (i = 0; i < ColNamesYear.GetLength(0); i++)
                {
                    col = new DataColumn();
                    col.ColumnName = ColNamesYear[i];
                    col.DataType = Types[i];
                    newTable.Columns.Add(col);
                }

                sCmd = "SELECT MONTH(DATUM), SUM(DISTANCE), ROUND(SUM(DURATION)/60.0,1), ROUND(AVG(AVG_SPEED), 1), MAX(MAX_SPEED), ROUND(AVG(WEIGHT),1), ";
                sCmd += " SUM(SUMUP), COUNT(ID) FROM ITEMS ";
                sCmd += " WHERE (YEAR(DATUM) = " + Year + ") ";
                if (FilterTypesDetail == true) sCmd += string.Format(" AND (ITEMS.TYP = {0})", Type);
                sCmd += " gRoup by MONTH(DATUM) ";
                dtPom = _SP.ReadData(sCmd, nVerbose);

                for (i = 0; i < dtPom.Rows.Count; i++)
                {
                    nMon = (int)dtPom.Rows[i][0];

                    obj = dtPom.Rows[i].ItemArray[1];
                    if (!DBNull.Value.Equals(obj)) dDist = double.Parse(dtPom.Rows[i].ItemArray[1].ToString());
                    else dDist = 0.0;

                    obj = dtPom.Rows[i].ItemArray[2];
                    if (!DBNull.Value.Equals(obj)) dDur = double.Parse(dtPom.Rows[i].ItemArray[2].ToString());
                    else dDur = 0.0;

                    obj = dtPom.Rows[i].ItemArray[3];
                    if (!DBNull.Value.Equals(obj)) dAvg = double.Parse(dtPom.Rows[i].ItemArray[3].ToString());
                    else dAvg = 0.0;

                    obj = dtPom.Rows[i].ItemArray[4];
                    if (!DBNull.Value.Equals(obj)) dMax = double.Parse(dtPom.Rows[i].ItemArray[4].ToString());
                    else dMax = 0.0;

                    obj = dtPom.Rows[i].ItemArray[5];
                    if (!DBNull.Value.Equals(obj)) dWeight = double.Parse(dtPom.Rows[i].ItemArray[5].ToString());
                    else dWeight = 0.0;

                    obj = dtPom.Rows[i].ItemArray[6];
                    if (!DBNull.Value.Equals(obj)) dUphill = double.Parse(dtPom.Rows[i].ItemArray[6].ToString());
                    else dUphill = 0.0;

                    obj = dtPom.Rows[i].ItemArray[7];
                    if (!DBNull.Value.Equals(obj)) dCount = double.Parse(dtPom.Rows[i].ItemArray[7].ToString());
                    else dCount = 0.0;

                    newTable.Rows.Add(MonthNames[nMon - 1], dDist, dDur, dAvg, dMax, dWeight, dUphill, dCount);
                }
                return (newTable);

            }
            else
            //------------------------------------------------------------------------------------------
            // Global overviw per years
            //------------------------------------------------------------------------------------------
            {

                for (i = 0; i < ColNamesYear.GetLength(0); i++)
                {
                    col = new DataColumn();
                    col.ColumnName = ColNamesAll[i];
                    col.DataType = Types[i];
                    newTable.Columns.Add(col);
                }

                dtYears = _SP.ReadData("SELECT DISTINCT YEAR(DATUM) from ITEMS", nVerbose);
                for (i = 0; i < dtYears.Rows.Count; i++)
                {
                    sCmd = "SELECT SUM(DISTANCE), ROUND(SUM(DURATION)/60.0,1), ROUND(AVG(AVG_SPEED),1), MAX(MAX_SPEED), ROUND(AVG(WEIGHT),1), ";
                    sCmd += " SUM(SUMUP), COUNT(ID) FROM ITEMS ";
                    sCmd += string.Format(" WHERE (YEAR(DATUM) = {0} )",dtYears.Rows[i][0]);
                    if (FilterTypesDetail == true) sCmd += string.Format(" AND (ITEMS.TYP = {0})", Type);
                    dtPom = _SP.ReadData(sCmd, nVerbose);

                    obj = dtPom.Rows[0].ItemArray[0];
                    if (!DBNull.Value.Equals(obj)) dDist = double.Parse(dtPom.Rows[0].ItemArray[0].ToString());
                    else dDist = 0.0;

                    obj = dtPom.Rows[0].ItemArray[1];
                    if (!DBNull.Value.Equals(obj)) dDur = double.Parse(dtPom.Rows[0].ItemArray[1].ToString());
                    else dDur = 0.0;

                    obj = dtPom.Rows[0].ItemArray[2];
                    if (!DBNull.Value.Equals(obj)) dAvg = double.Parse(dtPom.Rows[0].ItemArray[2].ToString());
                    else dAvg = 0.0;

                    obj = dtPom.Rows[0].ItemArray[3];
                    if (!DBNull.Value.Equals(obj)) dMax = double.Parse(dtPom.Rows[0].ItemArray[3].ToString());
                    else dMax = 0.0;

                    obj = dtPom.Rows[0].ItemArray[4];
                    if (!DBNull.Value.Equals(obj)) dWeight = double.Parse(dtPom.Rows[0].ItemArray[4].ToString());
                    else dWeight = 0.0;

                    obj = dtPom.Rows[0].ItemArray[5];
                    if (!DBNull.Value.Equals(obj)) dUphill = double.Parse(dtPom.Rows[0].ItemArray[5].ToString());
                    else dUphill = 0.0;

                    obj = dtPom.Rows[0].ItemArray[6];
                    if (!DBNull.Value.Equals(obj)) dCount = double.Parse(dtPom.Rows[0].ItemArray[6].ToString());
                    else dCount = 0.0;

                    newTable.Rows.Add(dtYears.Rows[i][0].ToString(), dDist, dDur, dAvg, dMax, dWeight, dUphill, dCount);

                }
                return (newTable);
            }
        }
            

    }
}
