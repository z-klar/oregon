using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.IO;

namespace cz.zk.OREGON
{
    public partial class Form1 : Form
    {

        /// <summary>
        /// Constants used for logging information (identifying the log message source)
        /// </summary>
        public const int LOGSRC_SQL = 0;
        public const int LOGSRC_XML = 1;
        public const int LOGSRC_INT = 2;
        public const int LOGSRC_FC = 3;
        public const int LOGSRC_GRF = 4;
        public const int LOGSRC_HTM = 5;

        private CultureInfo cult_us = new CultureInfo("en-US");

        private String[] SrcNames = { "SQL", "XML", "INT", "FC ", "GRF", "HTM" };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="msg"></param>
        public delegate void VoidIntString(int src, String msg);

        VoidIntString myVoidIntString;

        private DataTable dtTypes;
        private DataTable dtYears;
        private DataTable dtSections;

        private int nLogujSql = 1;

        private int dgvDetailedRowClicked;

        /// <summary>
        /// 
        /// </summary>
        private XmlParser XP;
        private SqlProcessing SP;
        private Charting CHART;
        private DataUtils DU;
        private ImportGpsData GPS;

        private int lbLogHorExtent = 0;

        private List<int> RouteSections = new List<int>();

        /// <summary>
        /// 
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            myVoidIntString = new VoidIntString(this.LogMessage);
            XP = new XmlParser(myVoidIntString);
            LoadConfig();
            SP = new SqlProcessing(XP.GetServerName(), XP.GetDbName(), XP.GetUserName(),
                       XP.GetPwd(), myVoidIntString);

            //dtTypes = SP.ReadData("SELECT * from TYPES order by DESCR", nLogujSql);
            dtTypes = SP.ReadData("SELECT * from TYPES", nLogujSql);
            foreach (DataRow row in dtTypes.Rows)
            {
                cbTypesDetail.Items.Add(row.ItemArray[1]);
                cbTypesInput.Items.Add(row.ItemArray[1]);
                cbTypesOvwTabs.Items.Add(row.ItemArray[1]);
            }
            cbTypesDetail.SelectedIndex = 0;
            cbTypesInput.SelectedIndex = 0;
            cbTypesOvwTabs.SelectedIndex = 0;

            dtYears = SP.ReadData("SELECT DISTINCT YEAR(DATUM) from ITEMS order by YEAR(DATUM)", nLogujSql);
            foreach (DataRow row in dtYears.Rows)
            {
                cbYearsDetail.Items.Add(row.ItemArray[0].ToString());
                cbYearMonOvw.Items.Add(row.ItemArray[0].ToString());
                cbYearWeekOvw.Items.Add(row.ItemArray[0].ToString());
                cbYearOvwTabs.Items.Add(row.ItemArray[0].ToString());
            }
            cbYearsDetail.SelectedIndex = dtYears.Rows.Count - 1;
            cbYearMonOvw.SelectedIndex = dtYears.Rows.Count-1;
            cbYearOvwTabs.SelectedIndex = dtYears.Rows.Count - 1;
            cbYearWeekOvw.SelectedIndex = dtYears.Rows.Count - 1;

            dtSections = SP.ReadData("SELECT * from SECTIONS order by NAME", nLogujSql);
            cbSections.Items.Clear();
            foreach(DataRow row in dtSections.Rows)
            {
                cbSections.Items.Add(Convert.ToString(row.ItemArray[1]));
            }
            cbSections.SelectedIndex = 0;

            CHART = new Charting();
            DU = new DataUtils(SP);
            GPS = new ImportGpsData(lbLogger);

            UpdateDetails(false);
            UpdateMonthStats();
            UpdateOverviewTables();
            UpdateWeekOvw();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private int LoadConfig()
        {
            XP.ReadConfig();
            txDatabase.Text = XP.GetDbName();
            txServer.Text = XP.GetServerName();
            txUser.Text = XP.GetUserName();
            txPwd.Text = "----------------";
            toolTip1.SetToolTip(txPwd, XP.GetPwd());
            
            return (0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="msg"></param>
        private void LogMessage(int src, String msg)
        {
            String ourmsg;

            ourmsg = string.Format("[{0}]: {1}", SrcNames[src], msg);
            lbLogger.Items.Add(ourmsg);
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateMainTab()
        {
            String sCmd, sFormat;
            DataTable dtKm, dtMain;
            int nPom;

            if (rbLength.Checked == true)
            {
                sCmd = "SELECT YEAR(Datum) as Rok, SUM(DISTANCE) as KM from ITEMS GROUP BY YEAR(DATUM)";
                sFormat = "D";
                nPom = 0;
            }
            else
            {
                sCmd = "SELECT YEAR(Datum) as Rok, SUM(DURATION)/60.0 as HOD from ITEMS GROUP BY YEAR(DATUM)";
                sFormat = "F1";
                nPom = 1;
            }
            dtKm = SP.ReadData(sCmd, nLogujSql);
            dgvMain.DataSource = dtKm;
            dgvMain.Columns[1].DefaultCellStyle.Format = sFormat;

            if (chkFilterTypesMain.Checked == false)
            {
                CHART.Render01(chartMain, dtKm, "Year");
            }
            else {
                dtMain = DU.GetYearlyTypeOverview(nPom, nLogujSql);
                CHART.Render01(chartMain, dtMain, "Year");
                dgvTest001.DataSource = dtMain;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateDetails(bool SetWidth)
        {
            DataTable dtDet, dtSummary;
            String sCmd;
            int nPom = 0;

            sCmd = "SELECT CONVERT(varchar,DATUM,104) as DATE, DATEPART(WEEK, DATUM) as CW, DURATION as LEN, DISTANCE as KM, AVG_SPEED as AVG,"; 
            sCmd += "MAX_SPEED as MAX, [ITEMS].[DESCR]as TRASA, WEIGHT as VAHA, ";
            sCmd += "SUMUP, [TYPES].[DESCR] as TYP from ITEMS inner join TYPES ON (ITEMS.TYP = TYPES.ID)";

            if (chkYersDetai.Checked == true)
            {
                nPom = 1;
                sCmd += String.Format(" where (YEAR(DATUM) = {0})", cbYearsDetail.SelectedItem.ToString());
            }

            if (chkTypesDetail.Checked == true)
            {
                if (nPom == 1)
                    sCmd += " AND ";
                else
                {
                    sCmd += " WHERE ";
                    nPom = 1;
                }
                sCmd += string.Format(" (ITEMS.TYP = {0})", GetTypIdByName(cbTypesDetail.SelectedItem.ToString()));
            }

            if (chkFilterStartEnd.Checked == true)
            {
                if (nPom == 1)
                    sCmd += " AND ";
                else
                {
                    sCmd += " WHERE ";
                    nPom = 1;
                }
                sCmd += string.Format("(DATUM>='{0:s}') and (DATUM<='{1:s}')", dtpDetailedStart.Value, dtpDetailedEnd.Value );
            }

            sCmd += "  ORDER BY DATUM";
            dtDet = SP.ReadData(sCmd, nLogujSql);

            dgvDetailed.DataSource = dtDet;

            dtSummary = DU.GetDetailesSummary(nLogujSql, chkYersDetai.Checked, cbYearsDetail.SelectedItem.ToString(),
                                             chkTypesDetail.Checked, GetTypIdByName(cbTypesDetail.SelectedItem.ToString()),
                                             chkFilterStartEnd.Checked, dtpDetailedStart.Value, dtpDetailedEnd.Value);
            dgvSumDetails.DataSource = dtSummary;

            if (SetWidth)
            {
                dgvDetailed.Columns[0].Width = 85;
                dgvDetailed.Columns[1].Width = 40;
                dgvDetailed.Columns[2].Width = 40;
                dgvDetailed.Columns[3].Width = 40;
                dgvDetailed.Columns[4].Width = 40;
                dgvDetailed.Columns[5].Width = 40;
                dgvDetailed.Columns[6].Width = 350;
                dgvDetailed.Columns[7].Width = 55;
                dgvDetailed.Columns[8].Width = 70;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TypeDescr"></param>
        /// <returns></returns>
        private int GetTypIdByName(String TypeDescr)
        {
            int i, j;
            String sPom;

            j = 1;
            for(i=0; i<dtTypes.Rows.Count; i++) {
                //if(dtTypes.Rows[i][1].ToString() == TypeDescr) {
                sPom = dtTypes.Rows[i][1].ToString();
                if(sPom == TypeDescr) {
                    j = (int) dtTypes.Rows[i][0];
                    break;
                }
            }

            return (j);
        }

        /// <summary>
        /// 
        /// </summary>
        private void InsertNewRecord()
        {
            String sDescr, sCmd, sDuration, sDistance, sAvg, sMax, sWeight, sSumup;
            int nTyp, nErr, noRec;
            String[] Params = { "Duration", "Distance", "Avg Spedd", "Max Speed", "Weight", "SumUp" };
            float fPom;
            CultureInfo cult = new CultureInfo("en-US");
            DataTable dtPom;

            sDescr = txInpDescription.Text;
            if (sDescr.Length > 250)
            {
                MessageBox.Show("Description too long !");
                return;
            }

            sDuration = txInpDuration.Text;
            sDistance = txInpDistance.Text;
            sAvg = txInpAvgspeed.Text;
            sMax = txInpMaxspeed.Text;
            sWeight = txInpWeight.Text;
            sSumup = txInpSumup.Text;
            nErr = 0;

            try
            {
                fPom = float.Parse(sDuration, cult);
                nErr++;
                fPom = float.Parse(sDistance, cult);
                nErr++;
                fPom = float.Parse(sAvg, cult);
                nErr++;
                fPom = float.Parse(sMax, cult);
                nErr++;
                fPom = float.Parse(sWeight, cult);
                nErr++;
                fPom = float.Parse(sSumup, cult);
                nErr++;
            }
            catch (Exception ex)
            {
                sDescr = ex.Message;
                MessageBox.Show("Wrong parameter: " + Params[nErr] + " !!");
                return;
            }

            /*
            sCmd = string.Format("SELECT COUNT(ID) from ITEMS where (DATUM='{0:s}')", dtpInpDate.Value);
            dtPom = SP.ReadData(sCmd, 0);
            noRec = Convert.ToInt32(dtPom.Rows[0].ItemArray[0]);
            if (noRec > 0)
            {
                MessageBox.Show("Record with given date already exists in database !");
                return;
            }
            */

            nTyp = GetTypIdByName(cbTypesInput.SelectedItem.ToString());

            sCmd = "INSERT into ITEMS (DATUM, TYP, DURATION, DISTANCE, AVG_SPEED, MAX_SPEED, DESCR, WEIGHT, SUMUP) ";
            sCmd += " VALUES ( ";
            sCmd += string.Format("'{0:s}', {1}, {2}, {3}, {4}, {5}, '{6}', {7}, {8})",
                                  dtpInpDate.Value, nTyp, txInpDuration.Text, txInpDistance.Text,
                                  txInpAvgspeed.Text, txInpMaxspeed.Text, txInpDescription.Text, txInpWeight.Text, txInpSumup.Text);


            lbLogger.Items.Add(sCmd);
            SP.ExecNonQuery(sCmd);

        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateMonthStats()
        {
            String sCmd;
            DataTable dtKm, dtMain;
            int nPom, i, j;


            if (rbLenMon.Checked == true)
            {
                sCmd = "SELECT MONTH(Datum) as Mesic, SUM(DISTANCE) as KM from ITEMS  ";
                nPom = 0;
            }
            else if(rbDurationMon.Checked == true)
            {
                sCmd = "SELECT MONTH(Datum) as Mesic, SUM(DURATION)/60.0 as HOD from ITEMS ";
                nPom = 1;
            }
            else 
            {
                sCmd = "SELECT MONTH(Datum) as Mesic, AVG(WEIGHT) as KG from ITEMS ";
                nPom = 2;
            }
            sCmd += " WHERE (YEAR(DATUM) = " + cbYearMonOvw.SelectedItem + ") GROUP BY MONTH(DATUM)";
            dtKm = SP.ReadData(sCmd, nLogujSql);
            dtMain = dtKm.Clone();
            for (i = 0; i < 12; i++)
            {
                j = FindMonth(dtKm, i + 1);
                if (j == -1) dtMain.Rows.Add(i + 1, 0);
                else dtMain.ImportRow(dtKm.Rows[j]);
            }

            if (chkFilterActivityMon.Checked == false)
            {
                CHART.Render01(chartMonOvw, dtMain, "Month");
            }
            else
            {
                dtMain = DU.GetMonthlyTypeOverview(nPom, cbYearMonOvw.SelectedItem.ToString(), nLogujSql);
                CHART.Render01(chartMonOvw, dtMain, "Month");
                //dgvTest001.DataSource = dtMain;
            }

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
        private void ImportFileGps()
        {
            TripRecord tr;
            float fAvg = 0.0F;
            int count;
            String d, m, r;
            DateTime dt;
            CultureInfo cult = new CultureInfo("en-US");

            tr = GPS.ReadTxtFile(txImportFileName.Text);

            String[] Phrases = tr.Datum.Split('.');
            count = Phrases.GetLength(0);
            if (count < 3) {
                MessageBox.Show("Unknown date: [" + tr.Datum + "] !!");
                return;
            }

            d = Phrases[0];
            m = Phrases[1]; ;
            r = Phrases[2]; ;
            dt = new DateTime(int.Parse(r), int.Parse(m), int.Parse(d));
            dtpInpDate.Value = dt;

            txInpDuration.Text = String.Format("{0}", tr.Duration/60);
            txInpSumup.Text = String.Format("{0}", tr.SumUphill);

            if ((tr.Length % 1000) > 500)
                txInpDistance.Text = String.Format("{0}", tr.Length / 1000 + 1);
            else
                txInpDistance.Text = String.Format("{0}", tr.Length / 1000);

            txInpMaxspeed.Text= String.Format(cult, "{0:F1}", tr.MaxSpeed);

            if (tr.Duration != 0)
            {
                fAvg = ((float)(tr.Length) / 1000.0F) / ((float)(tr.Duration) / 3600.0F);
            }
            txInpAvgspeed.Text = String.Format(cult, "{0:F1}", fAvg);

        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateOverviewTables()
        {
            DataTable dtSummary;

            dtSummary = DU.GetTableSummary(nLogujSql, chkFilterYearsOvwTabs.Checked, cbYearOvwTabs.SelectedItem.ToString(),
                                 chkFilterTypesOvwTab.Checked, GetTypIdByName(cbTypesOvwTabs.SelectedItem.ToString()));
            dgvOvwTables.DataSource = dtSummary;


        }

        /// <summary>
        /// 
        /// </summary>
        private void ImportGpxFile()
        {
            DateTime dtPom;
            TripRecord tr;
            float fAvg = 0.0F;
            CultureInfo cult = new CultureInfo("en-US");


            tr = GPS.ReadGpxFile(txImportGpxFilename.Text);

            dtPom = DateTime.Parse(tr.Datum);
            dtpInpDate.Value = dtPom;
            txInpDuration.Text = String.Format("{0}", tr.Duration / 60);
            txInpSumup.Text = String.Format("{0}", tr.SumUphill);

            if ((tr.Length % 1000) > 500)
                txInpDistance.Text = String.Format("{0}", tr.Length / 1000 + 1);
            else
                txInpDistance.Text = String.Format("{0}", tr.Length / 1000);

            txInpMaxspeed.Text = String.Format(cult, "{0:F1}", tr.MaxSpeed);

            if (tr.Duration != 0)
            {
                fAvg = ((float)(tr.Length) / 1000.0F) / ((float)(tr.Duration) / 3600.0F);
            }
            txInpAvgspeed.Text = String.Format(cult, "{0:F1}", fAvg);
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateWeekOvw()
        {
            DataTable dtPom;
            DataTable dtRead;
            int nPocTypes = dtTypes.Rows.Count;
            int i, nPom, j;
            double [,] dWeekValues = new double[53, nPocTypes];
            String sCmd, sPom;
            
            String[] ColNamesSum = { "Week", "Amount" };
            Type[] TypesSum = { typeof(int), typeof(double) };

            Array.Clear(dWeekValues, 0, dWeekValues.Length);

            sCmd = "SET DATEFIRST 1 ";
            sPom = SP.ExecNonQueryWithInfo(sCmd);
            //MessageBox.Show(sPom);

            if (chkFilterActivityWeek.Checked)    // detailed analyse of activities
            {
                if (rbLenWeek.Checked == true) nPom = 0;
                else if (rbDurationWeek.Checked == true) nPom = 1;
                else nPom = 2;
                dtPom = DU.GetWeeklyTypeOverview(nPom, cbYearWeekOvw.SelectedItem.ToString(), nLogujSql);
                CHART.Render01(chartWeekOvw, dtPom, "CW");
            }
            else  // summary overview
            {
                if (rbLenWeek.Checked == true)
                {
                    sCmd = "SELECT DATEPART(WEEK, DATUM) as CW, SUM(DISTANCE) as KM from ITEMS  ";
                    nPom = 0;
                }
                else if (rbDurationWeek.Checked == true)
                {
                    sCmd = "SELECT DATEPART(WEEK, DATUM) as CW, SUM(DURATION)/60.0 as HOD from ITEMS ";
                    nPom = 1;
                }
                else
                {
                    sCmd = "SELECT DATEPART(WEEK, DATUM) as CW, AVG(WEIGHT) as KG from ITEMS ";
                    nPom = 2;
                }
                sCmd += " WHERE (YEAR(DATUM) = " + cbYearWeekOvw.SelectedItem + ") GROUP BY DATEPART(WEEK, DATUM)";
                dtRead = SP.ReadData(sCmd, nLogujSql);
                dtPom = dtRead.Clone();
                for (i = 0; i < 53; i++)
                {
                    j = FindWeek(dtRead, i+1);
                    if(j == -1) dtPom.Rows.Add(i+1, 0);
                    else dtPom.ImportRow(dtRead.Rows[j]);
                }
                CHART.Render01(chartWeekOvw, dtPom, "CW");
            }
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

        //============================================================================================

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime dtime1;
            dtime1 = DateTime.Now;
            lblSysClock.Text = String.Format("{0}.{1}.{2}  {3:D2}:{4:D2}:{5:D2}",
            dtime1.Day, dtime1.Month, dtime1.Year, dtime1.Hour, dtime1.Minute, dtime1.Second);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdateMain_Click(object sender, EventArgs e)
        {
            UpdateMainTab();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdateDetails_Click(object sender, EventArgs e)
        {
            UpdateDetails(true);
        }

        private void chkTypesDetail_CheckedChanged(object sender, EventArgs e)
        {
            if (chkTypesDetail.Checked == true) cbTypesDetail.Enabled = true;
            else cbTypesDetail.Enabled = true;
        }

        private void chkYersDetai_CheckedChanged(object sender, EventArgs e)
        {
            if(chkYersDetai.Checked == true) cbYearsDetail.Enabled = true;
            else cbYearsDetail.Enabled = false;
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            lbLogger.Items.Clear();
            lbLogHorExtent = 0;
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            InsertNewRecord();
        }

        private void btnUpdateMonOvw_Click(object sender, EventArgs e)
        {
            UpdateMonthStats();
        }

        private void btnLocateImportFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "TXT files (*.txt)|*.txt|All files (*.*)|*.*";
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txImportFileName.Text = ofd.FileName;
            }
        }

        private void btnImportFile_Click(object sender, EventArgs e)
        {
            ImportFileGps();
        }

        private void btnExportLog_Click(object sender, EventArgs e)
        {
            StreamWriter sw = null;
            int i;

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.FileName = "OregonLog.txt";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //MessageBox.Show("Selected file:\n" + saveFileDialog1.FileName);
                FileStream fs = File.Open(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write);
                sw = new StreamWriter(fs, System.Text.Encoding.ASCII);
                for (i = 0; i < lbLogger.Items.Count; i++)
                    sw.WriteLine(lbLogger.Items[i]);
                sw.Close();
                fs.Close();
            }
        }

        private void FormShown(object sender, EventArgs e)
        {
            UpdateMainTab();
            nLogujSql = 0;
        }

        private void chkLogSql_CheckedChanged(object sender, EventArgs e)
        {
            if (chkLogSql.Checked == true) nLogujSql = 1;
            else nLogujSql = 0;
        }

        private void chkFilterStartEnd_CheckedChanged(object sender, EventArgs e)
        {
            if (chkFilterStartEnd.Checked == true)
            {
                dtpDetailedEnd.Enabled = true;
                dtpDetailedStart.Enabled = true;
            }
            else {
                dtpDetailedEnd.Enabled = false;
                dtpDetailedStart.Enabled = false;
            }

        }

        private void chFilterYearsOvwTabs_CheckedChanged(object sender, EventArgs e)
        {
            if (chkFilterYearsOvwTabs.Checked == true) cbYearOvwTabs.Enabled = true;
            else cbYearOvwTabs.Enabled = false;
        }

        private void chkFilterTypesOvwTab_CheckedChanged(object sender, EventArgs e)
        {
            if (chkFilterTypesOvwTab.Checked == true) cbTypesOvwTabs.Enabled = true;
            else cbTypesOvwTabs.Enabled = false;
        }

        private void bnUpdateOvwTables_Click(object sender, EventArgs e)
        {
            UpdateOverviewTables();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "GPX files (*.gpx)|*.gpx|All files (*.*)|*.*";
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txImportGpxFilename.Text = ofd.FileName;
            }

        }

        private void btnImportGpxFile_Click(object sender, EventArgs e)
        {
            ImportGpxFile();

        }

        private void cbYearsDetail_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void lbLogDrawItem(object sender, DrawItemEventArgs e)
        {
            Brush myBrush = Brushes.Black;

            if (e.Index < 0) return;
            //if the item state is selected them change the back color 


            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                e = new DrawItemEventArgs(e.Graphics,
                              e.Font,
                              e.Bounds,
                              e.Index,
                              e.State ^ DrawItemState.Selected,
                              Color.Lime,
                              Color.Maroon);//Choose the color
            }
            else
            {
                e = new DrawItemEventArgs(e.Graphics,
                              e.Font,
                              e.Bounds,
                              e.Index,
                              e.State,
                              Color.Lime,
                              Color.Black);//Choose the color
            }
            // Draw the background of the ListBox control for each item.
            e.DrawBackground();
            // Draw the current item text
            e.Graphics.DrawString(lbLogger.Items[e.Index].ToString(), e.Font, Brushes.Lime, e.Bounds, StringFormat.GenericDefault);
            // If the ListBox has focus, draw a focus rectangle around the selected item.
            e.DrawFocusRectangle();

            int hzSize = (int)e.Graphics.MeasureString(lbLogger.Items[e.Index].ToString(), e.Font).Width;
            if (hzSize > lbLogHorExtent)
            {
                lbLogHorExtent = lbLogger.HorizontalExtent = hzSize;
            }
        }

        private void rbTime_CheckedChanged(object sender, EventArgs e)
        {
            UpdateMainTab();

        }

        private void chkFilterTypesMain_CheckedChanged(object sender, EventArgs e)
        {
            UpdateMainTab();
        }

        private void rbLength_CheckedChanged(object sender, EventArgs e)
        {
            UpdateMainTab();

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 ab = new AboutBox1();
            ab.Show();
        }

        private void btnUpdateWeekOvw_Click(object sender, EventArgs e)
        {
            UpdateWeekOvw();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgDetailedMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            int FirstRow, RowHeight, iRow, iCol, iXcol, i;

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                FirstRow = dgvDetailed.FirstDisplayedScrollingRowIndex;
                RowHeight = dgvDetailed.Rows[0].Height;
                iRow = e.RowIndex;
                iCol = e.ColumnIndex;

                dgvDetailedRowClicked = e.RowIndex;

                iXcol = 0;
                for (i = 0; i < iCol; i++) iXcol += dgvDetailed.Columns[i].Width;
                popupDgvDetailed.Show((Control)sender, new Point(e.X + iXcol, (iRow - FirstRow) * RowHeight));
            }

        }

        private void copyRecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //MessageBox.Show(string.Format("Row = {0}", dgvDetailedRowClicked));
            string Duration, Distance, Avg, Max, SummUp;
            string Description, sType;

            Duration = Convert.ToString(dgvDetailed.Rows[dgvDetailedRowClicked].Cells[2].Value, cult_us);
            Distance = Convert.ToString(dgvDetailed.Rows[dgvDetailedRowClicked].Cells[3].Value, cult_us);
            Avg = Convert.ToString(dgvDetailed.Rows[dgvDetailedRowClicked].Cells[4].Value, cult_us);
            Max = Convert.ToString(dgvDetailed.Rows[dgvDetailedRowClicked].Cells[5].Value, cult_us);
            SummUp = Convert.ToString(dgvDetailed.Rows[dgvDetailedRowClicked].Cells[8].Value, cult_us);

            Description = Convert.ToString(dgvDetailed.Rows[dgvDetailedRowClicked].Cells[6].Value);
            sType = Convert.ToString(dgvDetailed.Rows[dgvDetailedRowClicked].Cells[9].Value);

            txInpAvgspeed.Text = Avg;
            txInpMaxspeed.Text = Max;
            txInpDuration.Text = Duration;
            txInpDistance.Text = Distance;
            txInpSumup.Text = SummUp;
            txInpDescription.Text = Description;
            cbTypesInput.SelectedItem = sType;
            tabControl1.SelectedIndex = 5;

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClearSectionList_Click(object sender, EventArgs e)
        {
            RouteSections.Clear();
            UpdateSectionList();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddSection_Click(object sender, EventArgs e)
        {
            AddSection();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRemoveSection_Click(object sender, EventArgs e)
        {
            RemoveSection();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdateSectionDef_Click(object sender, EventArgs e)
        {
            UpdateSectionDefList();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddSectionDef_Click(object sender, EventArgs e)
        {
            frmEditSection frm = new frmEditSection(" -- name --", 0.0, 0);
            frm.ProcessNewParams += new EventHandler<ProcessNewParamsArgs>(AddNewSectionDef);
            frm.Show();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddNewSectionDef(object sender, ProcessNewParamsArgs e)
        {
            string sCmd;
            CultureInfo cult = new CultureInfo("en-US");

            MessageBox.Show(string.Format("Add: [{0}] {1}  {2}", e.Name, e.Len, e.Sumup));

            sCmd = string.Format(cult, "INSERT into SECTIONS (NAME, LEN, SUMUP) values('{0}', {1}, {2})",
                                                                            e.Name, e.Len, e.Sumup);
            SP.ExecNonQuery(sCmd);
            UpdateSectionDefList();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabInsertDataEntered(object sender, EventArgs e)
        {
            //MessageBox.Show("Insert_Data ENTERED !");
            //cbTypesInput.SelectedIndex = cbTypesInput.Items.Count - 1;
        }
    }
}
