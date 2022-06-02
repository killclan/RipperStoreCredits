using System;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;

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
            label3.Visible = false;

            HttpClient http = new HttpClient();
            var res = http.GetAsync($"https://api.ripper.store/clientarea/credits/validate?apiKey={textBox1.Text}").Result;
            if ((int)res.StatusCode == 200)
            {
                Program.Config = new Config() { apiKey = textBox1.Text, LogToConsole = true };
                File.WriteAllText("RipperStoreCredits.txt", JsonConvert.SerializeObject(Program.Config));
                Close();
            }
            else
            {
                textBox1.Text = "";
                label3.Visible = true;
                return;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click_1(object sender, EventArgs e)
        {

        }
    }
}
