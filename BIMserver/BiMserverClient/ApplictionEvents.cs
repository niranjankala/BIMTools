using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BiMserverClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnCreateProject_Click(object sender, EventArgs e)
        {

        }         

        private void btnLogin_Click(object sender, EventArgs e)
        {
            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.Append("{");
            jsonBuilder.Append("\"token\": \"d79201beb506f7037539b26d2aba4fc40492c1da95b128a9217902616fa8fc8bbf64d32e2f2b0b502d5cab766d4be156\",");
            jsonBuilder.Append("\"request\": {");
            jsonBuilder.Append("\"interface\": \"AuthInterface\",");
            jsonBuilder.Append("\"method\": \"login\",");
            jsonBuilder.Append("\"parameters\": {");
            jsonBuilder.Append($"\"username\": \"{txtLoginId.Text}\",");
            jsonBuilder.Append($"\"password\": \"{txtPassword.Text}\"");
            jsonBuilder.Append("}");
            jsonBuilder.Append("}");
            jsonBuilder.Append("}");
            string json = jsonBuilder.ToString();
            var result = CreateRequest(json);
            var o = JsonConvert.DeserializeObject<dynamic>(result);
            txtResult.Text = result;
        }

        private string CreateRequest(string json)
        {
            string result;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create($"{txtBiMServerUrl.Text}/json");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {                
                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }
            return result;
        }

    }
}
