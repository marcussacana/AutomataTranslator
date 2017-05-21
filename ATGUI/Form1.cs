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

        bool BinMode = false;
        bool TmdMode = false;

        private BinTL BinEditor;
        private SMDManager Editor;
        private TMDEditor TmdEditor;
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e) {
            listBox1.Items.Clear();
            BinMode = openFileDialog1.FileName.ToLower().EndsWith(".bin");
            TmdMode = openFileDialog1.FileName.ToLower().EndsWith(".tmd");
            byte[] Script = File.ReadAllBytes(openFileDialog1.FileName);
            string[] strs;
            if (TmdMode) {
                TmdEditor = new TMDEditor(Script);
                strs = TmdEditor.Import();
            } else if (BinMode) {
                BinEditor = new BinTL(Script);
                strs = BinEditor.Import();
            } else {
                Editor = new SMDManager(Script);
                strs = Editor.Import();
            }
            
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
                if (BinMode) {

                    const string Mask = "ID: {0}/{1} |Detect Lang: {2} |Is Replace Target: {3}";
                    Language Lang = BinEditor.LanguageMap[BinEditor.IndexMap[i]];
                    Text = string.Format(Mask, i, listBox1.Items.Count, Lang.ToString(), BinEditor.ReplacesTargets.Contains(Lang) ? "Yes" : "No");
                } else
                    Text = "ID: " + i + "/" + listBox1.Items.Count;
                textBox1.Text = listBox1.Items[i].ToString();
            }
            catch { }
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e) {
            string[] Strs = new string[listBox1.Items.Count];
            for (int i = 0; i < Strs.Length; i++)
                Strs[i] = listBox1.Items[i].ToString();
            
            File.WriteAllBytes(saveFileDialog1.FileName, TmdMode ? TmdEditor.Export(Strs) : (BinMode ?  BinEditor.Export(Strs) : Editor.Export(Strs)));
            MessageBox.Show("File Saved.", "AutomataTranslator", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
            saveFileDialog1.FilterIndex = BinMode ? 2 : 1;
            saveFileDialog1.ShowDialog();
        }
    }
}
