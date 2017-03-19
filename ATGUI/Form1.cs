using AutomataTranslator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ATGUI {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            openFileDialog1.ShowDialog();
        }

        private SMDManager Editor;
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e) {
            listBox1.Items.Clear();
            Editor = new SMDManager(File.ReadAllBytes(openFileDialog1.FileName));
            string[] strs = Editor.Import();
            foreach (string str in strs)
                listBox1.Items.Add(str);
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == '\n' || e.KeyChar == '\r') {
                try {
                    listBox1.Items[listBox1.SelectedIndex] = textBox1.Text;
                }
                catch { }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
            try {
                int i = listBox1.SelectedIndex;
                Text = "ID: " + i + "/" + listBox1.Items.Count;
                textBox1.Text = listBox1.Items[i].ToString();
            }
            catch { }
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e) {
            string[] Strs = new string[listBox1.Items.Count];
            for (int i = 0; i < Strs.Length; i++)
                Strs[i] = listBox1.Items[i].ToString();
            File.WriteAllBytes(saveFileDialog1.FileName, Editor.Export(Strs));
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
            saveFileDialog1.ShowDialog();
        }
    }
}
