﻿using IndieGoat.Net.SSH;
using IndieGoat.Net.Updater;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IndieGoat.Net;
using IndieGoat.InideClient.Default;

namespace Universal_Service_Updater_Tets
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GlobalSSH sshService = new GlobalSSH("indiegoat.us", 80, "public", "Public36", false);
            sshService.ConnectSSH("indiegoat.us", 80, "public", "Public36");
            Console.WriteLine(sshService.IsConnected());
            sshService.TunnelLocalPort("192.168.0.16", "3389", true);
            sshService.TunnelLocalPort("192.168.0.16", "5750", true);
            new Uri("https://dl.dropbox.com/s/vppsempy90194q1/install.zip?dl=0");
            UniversalServiceUpdater updater = new UniversalServiceUpdater();
            updater.UpdateUrlLocation = "https://.dropbox.com/s/vppsempy90194q1/install.zip?dl=0";
            updater.CheckUpdate("localhost", 5750);
        }
    }
}
