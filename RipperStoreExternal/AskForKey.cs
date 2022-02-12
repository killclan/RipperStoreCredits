using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;

namespace Ripper.Store.External
{
    public partial class AskForKey : Form
    {
        public AskForKey()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Program.Config = new Config() { apiKey = textBox1.Text, LogToConsole = true };
            File.AppendAllText("RipperStoreCredits.txt", JsonConvert.SerializeObject(Program.Config));
            Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
