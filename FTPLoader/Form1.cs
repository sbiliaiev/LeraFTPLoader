﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentFTP;
using System.Net;
using Newtonsoft;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace FTPLoader
{
    public partial class Form1 : Form
    {
        static FtpClient client;
        public static string CurrentCity;
        public static FtpListItem CurrentItem;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            StreamReader file;
            JsonTextReader reader;
            JObject o2;
            try
            {
                Console.WriteLine("PATH " + Path.Combine(Environment.CurrentDirectory, "settings.json"));
                file = File.OpenText(Path.Combine(Directory.GetCurrentDirectory(), "settings.json"));
                reader = new JsonTextReader(file);
                o2 = (JObject)JToken.ReadFrom(reader);
            

                Console.WriteLine("JSON " + o2.ToString());
                Console.WriteLine("LOGIN " + o2["login"]);
                Console.WriteLine("LOGIN " + o2["port"]);


                /*client = new FtpClient("127.0.0.1");
                client.Port = 54218;
                client.Credentials = new NetworkCredential("dev_user", "dev_user_pass");*/
                client = new FtpClient((string)o2["server"]);
                var tmp = (int)o2["port"];
                Console.WriteLine("TYPE " + tmp.GetType());
                client.Port = tmp;
                client.Credentials = new NetworkCredential((string)o2["login"], (string)o2["password"]);
                NetworkCredential test1 = new NetworkCredential("test1_user", "test1_pass");
                NetworkCredential test2 = new NetworkCredential((string)o2["login"], (string)o2["password"]);
                client.Connect();

                List<string> cityList = new List<string>();

                foreach (FtpListItem item in client.GetListing("/"))
                {
                    cityList.Add(item.Name);
                }

                checkedListBox1.DataSource = cityList;
            }
            catch (Exception error)
            {
                Console.WriteLine("ERROR" + error.ToString());
                MessageBox.Show("Please, Create settings.json file");
                this.Close();
            }
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*List<string> checkedItems = new List<string>();
            foreach (var item in checkedListBox1.CheckedItems)
                checkedItems.Add(item.ToString());*/

            //if (e.NewValue == CheckState.Checked)
              //  checkedItems.Add(checkedListBox1.Items[e.Index].ToString());

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DateTime CurrentDate = DateTime.Now;
            string CurrentDateStr = CurrentDate.ToString("yyyyMMdd");

            foreach (var city in checkedListBox1.CheckedItems)
            {
                CurrentCity = city.ToString();
                FtpListItem item = client.GetListing(city.ToString() + "/Reports").Last();
                //foreach (FtpListItem item in client.GetListing(city.ToString() + "/Reports"))
                //{
                Console.WriteLine("FULL BOURNE " + item.FullName);
                    CurrentItem = item;
                    Console.WriteLine("HERERER " + item.Name.Substring(0, item.Name.IndexOf("Report")));
                    Console.WriteLine("HERERER " + item.Name.Substring(item.Name.IndexOf("Report") + 14));
                    string FileName = item.Name;
                    int Index = FileName.IndexOf("Report");
                    string FileNameDate = FileName.Substring(Index + 6, 8);
                    Console.WriteLine("FileNameDate" + FileNameDate);
                    if (FileNameDate.Equals(CurrentDateStr))
                    {
                        client.DownloadFile(Environment.CurrentDirectory +  "\\LeraFTPLoader\\" + CurrentDate.ToString("yyyy_MM_dd") + "\\" + item.Name, item.FullName);
                    } else
                    {
                        Prompt prompt = new Prompt();
                        prompt.ShowDialog();
                    }
                //}
            }
        }

        public static void download_Previous(FtpListItem item)
        {
            DateTime PreviousDate = DateTime.Now.AddDays(-1);
            string PreviousFileName = item.Name.Substring(0, item.Name.IndexOf("Report"))
                + "Report"
                + PreviousDate.ToString("yyyyMMdd")
                + item.Name.Substring(item.Name.IndexOf("Report") + 14);
            string PreviousFileFullName = item.FullName.Substring(0, item.FullName.IndexOf(CurrentCity + "Report")) + PreviousFileName;

            if (client.FileExists(PreviousFileFullName))
            {
                Console.WriteLine("EST");
                client.DownloadFile(Environment.CurrentDirectory + "\\LeraFTPLoader\\" + DateTime.Now.ToString("yyyy_MM_dd") + "\\" + PreviousFileName, PreviousFileFullName);
            }
            Console.WriteLine("PreviousFileName " + PreviousFileName);
            Console.WriteLine("PreviousFileFullName " + PreviousFileFullName);
        }
    }
}
