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
        /// 
        /// </summary>
        private void UpdateSectionList()
        {
            double dLen = 0;
            int nSumup = 0;
            string descr = "";

            lbSection.Items.Clear();
            foreach(int id in RouteSections)
            {
                lbSection.Items.Add(GetSectionNameById(id));
                nSumup += GetSectionSumup(id);
                dLen += GetSectionLen(id);
                descr += GetSectionNameById(id);
                descr += ", ";
            }
            txInpDescription.Text = descr;
            txInpDistance.Text = string.Format(cult_us, "{0:F1}", dLen);
            txInpSumup.Text = string.Format(cult_us, "{0}", nSumup);
            txInpMaxspeed.Text = string.Format(cult_us, "{0:F1}", 5);
            txInpAvgspeed.Text = string.Format(cult_us, "{0:F1}", 5);
            txInpDuration.Text = string.Format(cult_us, "{0:F1}", dLen * 12);
            // switch TYPE to the last item, which should be "WALKING"
            cbTypesInput.SelectedIndex = cbTypesInput.Items.Count - 1;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private int GetSectionSumup(int id)
        {
            int dur = 1111;

            foreach(DataRow row in dtSections.Rows)
            {
                if(Convert.ToInt32(row.ItemArray[0]) == id)
                {
                    dur = Convert.ToInt32(row.ItemArray[3]);
                    break;
                }
            }
            return (dur);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private double GetSectionLen(int id)
        {
            double len = 9999;

            foreach (DataRow row in dtSections.Rows)
            {
                if (Convert.ToInt32(row.ItemArray[0]) == id)
                {
                    len = Convert.ToDouble(row.ItemArray[2]);
                    break;
                }
            }
            return (len);
        }

        /// <summary>
        /// 
        /// </summary>
        private void AddSection()
        {
            string sectionName = cbSections.SelectedItem.ToString();
            int id = GetSectionIdByName(sectionName);
            RouteSections.Add(id);
            UpdateSectionList();
        }


        /// <summary>
        /// 
        /// </summary>
        private void RemoveSection()
        {
            if (lbSection.SelectedIndex < 0) return;
            RouteSections.RemoveAt(lbSection.SelectedIndex);
            UpdateSectionList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private int GetSectionIdByName(string name)
        {
            int id = -1;

            foreach (DataRow row in dtSections.Rows)
            {
                if (Convert.ToString(row.ItemArray[1]).CompareTo(name) == 0)
                {
                    id = Convert.ToInt32(row.ItemArray[0]);
                    break;
                }
            }

            return (id);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private string GetSectionNameById(int id)
        {
            string sPom = "????";

            foreach(DataRow row in dtSections.Rows)
            {
                if(Convert.ToInt32(row.ItemArray[0]) == id)
                {
                    sPom = Convert.ToString(row.ItemArray[1]);
                    sPom = sPom.TrimEnd();
                    break;
                }
            }

            return (sPom);
        }


        private void UpdateSectionDefList()
        {
            dtSections = SP.ReadData("SELECT * from SECTIONS order by NAME", nLogujSql);
            cbSections.Items.Clear();
            foreach (DataRow row in dtSections.Rows)
            {
                cbSections.Items.Add(Convert.ToString(row.ItemArray[1]));
            }
            cbSections.SelectedIndex = 0;

            dgvTest001.DataSource = dtSections;
            dgvTest001.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

        }

    }
}
