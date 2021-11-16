﻿using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace detnsw_autologin
{
    public partial class Form1 : Form
    {
        public string fileName = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), "circularsprojects", "settings.json");

        public Form1()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string resourceName = new AssemblyName(args.Name).Name + ".dll";
                string resource = Array.Find(this.GetType().Assembly.GetManifestResourceNames(), element => element.EndsWith(resourceName));

                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
                {
                    Byte[] assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try {
                WebRequest request = WebRequest.Create("https://edgeportal.det.nsw.edu.au:6082/php/uid.php?vsys=1&rule=0");
                request.Method = "POST";
                request.Timeout = 5000;

                string postData = $"inputStr=&escapeUser=&preauthid=&user={textBox1.Text}&passwd={textBox2.Text}&ok=Login";
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;

                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
            } catch {
                MessageBox.Show("Could not connect to the server after 5000 milliseconds.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            /*
            WebResponse response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            using (dataStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                Console.WriteLine(responseFromServer);
            }
            response.Close();
            */
        }

        private void button2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://edgeportal.det.nsw.edu.au:6082/php/uid.php?vsys=1&rule=0");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                if (args[1] == "-startup")
                {
                    string username;
                    string password;
                    try
                    {
                        var jsonfile = File.ReadAllText(fileName);
                        var jsonobj = JObject.Parse(jsonfile);
                        username = jsonobj.SelectToken("username").Value<string>();
                        password = jsonobj.SelectToken("password").Value<string>();
                        WebRequest request = WebRequest.Create("https://edgeportal.det.nsw.edu.au:6082/php/uid.php?vsys=1&rule=0");
                        request.Method = "POST";
                        request.Timeout = 5000;

                        string postData = $"inputStr=&escapeUser=&preauthid=&user={username}&passwd={password}&ok=Login";
                        byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                        request.ContentType = "application/x-www-form-urlencoded";
                        request.ContentLength = byteArray.Length;

                        Stream dataStream = request.GetRequestStream();
                        dataStream.Write(byteArray, 0, byteArray.Length);
                        dataStream.Close();
                    } catch
                    {
                        MessageBox.Show("detnsw autologin: Could not connect to the server after 5000 milliseconds", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    Application.Exit();
                }
            }
            Console.WriteLine(fileName);
            try {
            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "circularsprojects")))
            {
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "circularsprojects"));
            }
            if (!File.Exists(fileName))
            {
                File.WriteAllText(fileName, "{\"username\":\"\",\"password\":\"\"}");
            }
            } catch
            {
                MessageBox.Show("Error creating settings file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            try {
                var myJsonString = File.ReadAllText(fileName);
                var myJObject = JObject.Parse(myJsonString);
                textBox1.Text = myJObject.SelectToken("username").Value<string>();
                textBox2.Text = myJObject.SelectToken("password").Value<string>();
            } catch
            {
                MessageBox.Show("Error reading settings file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            try {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                object o = key.GetValue("detnsw-autologin", null);
            if (o != null)
            {
                checkBox1.Checked = true;
            }
            else
            {
                checkBox1.Checked = false;
            }
            } catch
            {
                MessageBox.Show("Error reading registry", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try {
            File.WriteAllText(fileName, $"{{\"username\":\"{textBox1.Text}\",\"password\":\"{textBox2.Text}\"}}");
            } catch
            {
                MessageBox.Show("Error writing settings file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            try {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (checkBox1.Checked)
            {
                if (key.GetValue("detnsw-autologin") == null)
                {
                    key.SetValue("detnsw-autologin", $"{Application.ExecutablePath} -startup");
                }
            }
            else
            {
                if (key.GetValue("detnsw-autologin") != null)
                {
                    key.DeleteValue("detnsw-autologin");
                }
            }
            } catch (Exception ex)
            {
                MessageBox.Show($"Error writing registry, {Convert.ToString(ex)}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            // https://www.daveoncsharp.com/2009/08/read-write-delete-from-windows-registry-with-csharp/
            // https://newbedev.com/how-to-delete-a-registry-value-in-c
        }
    }
}