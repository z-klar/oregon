using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace cz.zk.OREGON
{
    public partial class frmEditSection : Form
    {
        public event EventHandler<ProcessNewParamsArgs> ProcessNewParams;

        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        public frmEditSection(string name, double len, int sumup)
        {
            InitializeComponent();

            txSectionName.Text = name;
            txSectionLen.Text = string.Format("{0:F1}", len);
            txSectionSumup.Text = sumup.ToString();
        }


        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOk_Click(object sender, EventArgs e)
        {
            ProcessNewParamsArgs args = new ProcessNewParamsArgs();
            try
            {
                args.Sumup = Convert.ToInt32(txSectionSumup.Text);
                args.Len = Convert.ToDouble(txSectionLen.Text);
                args.Name = txSectionName.Text;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            OnProcessNewParams(args);

            this.Hide();
        }

        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Hide();
        }


        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnProcessNewParams(ProcessNewParamsArgs e)
        {
            EventHandler<ProcessNewParamsArgs> handler = ProcessNewParams;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ProcessNewParamsArgs : EventArgs
    {
        public string Name { get; set; }
        public int Sumup { get; set; }
        public double Len { get; set; }
    }
}
