using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;

namespace cz.zk.OREGON
{
    class Charting
    {
        public Charting()
        {


        }


        public int Render01(Chart Graf, DataTable dtInp, string sXname)
        {
            int i;

            int PocetSloupcu;
            String ColName, xName;

            DataSet ds3 = new DataSet();
            ds3.Tables.Add(dtInp);


            Graf.Series.Clear();
            xName = dtInp.Columns[0].ColumnName;
            PocetSloupcu = dtInp.Columns.Count; 
            for (i = 1; i < PocetSloupcu; i++)
            {
                ColName = dtInp.Columns[i].ColumnName;
                Graf.Series.Add(ColName);
                Graf.Series[ColName].ChartType = SeriesChartType.StackedColumn;
                Graf.Series[ColName].YValueMembers = ColName;
                Graf.Series[ColName].XValueMember = xName;
                Graf.Series[ColName].ToolTip = sXname + ": #VALX  #SERIESNAME: #VALY";
            }
            Graf.Series[0].Color = Color.Yellow;


            //Graf.Titles[0].ForeColor = Color.Yellow;
            Graf.Legends[0].BackColor = Color.Black;
            Graf.Legends[0].ForeColor = Color.White;
            Graf.Legends[0].Docking = Docking.Top;
            Graf.BackColor = Color.Black;
            Graf.ChartAreas[0].BackColor = Color.Black;
            Graf.ChartAreas[0].AxisX.LineColor = Color.White;
            Graf.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.White;
            Graf.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.White;
            Graf.ChartAreas[0].AxisY.LineColor = Color.White;
            Graf.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.White;
            Graf.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.White;

            Graf.DataSource = ds3.Tables[0].DefaultView;
            Graf.Series[dtInp.Columns[1].ColumnName].XValueMember = xName;

            Graf.DataBind();

            return (0);
        }


    }
}
