﻿using IndieGoat.Net.Updater;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            UniversalServiceUpdater updater = new UniversalServiceUpdater("test");
            updater.CheckUpdate("localhost", 7777);
        }
    }
}
