using NLua;
using NLua.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace LuaTester
{
    public partial class FormMain : Form
    {
        Lua m_lua;
        private string m_filepath;

        public FormMain()
        {
            InitializeComponent();

            CreateLuaContext();
        }

        private void CreateLuaContext()
        {
            m_lua = new Lua();

            m_lua.RegisterFunction("log", this, this.GetType().GetMethod("Log"));
            m_lua.RegisterFunction("print", this, this.GetType().GetMethod("Log"));
        }

        private void DebugTable(LuaTable table)
        {
            Dictionary<object, object> dict = m_lua.GetTableDict(table);
            foreach (var item in dict)
            {
                if (item.Key == null)
                {
                    continue;
                }

                lbxLog.Items.Add(string.Format("{0}: {1}",
                    item.Key.ToString(),
                    item.Value.ToString()));
            }
        }

        public void Log(object obj)
        {
            if (obj == null)
            {
                return;
            }

            LuaTable table = obj as LuaTable;
            if (table != null)
            {
                DebugTable(table);
            }
            else
            {
                lbxLog.Items.Add("Log: " + obj.ToString());
            }

            lbxLog.SelectedIndex = lbxLog.Items.Count - 1;
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(rtbScript.Text))
            {
                try
                {
                    lbxLog.Items.Clear();

                    object[] results = m_lua.DoString(rtbScript.Text);

                    if (results != null && results.Length > 0)
                    {
                        lbxLog.Items.Add("--- Results ---");
                        for (int i = 0; i < results.Length; i++)
                        {
                            if (results[i] != null)
                            {
                                lbxLog.Items.Add(results[i].ToString());
                            }
                        }
                    }
                }
                catch (LuaException ex)
                {
                    lbxLog.Items.Add("Error: " + ex.Message);
                }
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Skript öffnen";
                dialog.Filter = "Lua-Dateien|*.lua|Alle Dateien|*.*";
                dialog.FilterIndex = 1;

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    rtbScript.Text = File.ReadAllText(dialog.FileName);
                    m_filepath = dialog.FileName;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(m_filepath))
            {
                File.WriteAllText(m_filepath, rtbScript.Text);
            }
        }

        private void btnClearContext_Click(object sender, EventArgs e)
        {
            lbxLog.Items.Clear();

            CreateLuaContext();
        }

        private void btnGlobals_Click(object sender, EventArgs e)
        {
            string key = tbxGlobalValue.Text.Trim();
            if (!string.IsNullOrWhiteSpace(key))
            {
                try
                {
                    m_lua.DoString("log(" + key + ");");
                }
                catch (LuaException ex)
                {
                    MessageBox.Show(ex.Message, "Lua Exception",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            StringBuilder buffer = new StringBuilder();

            foreach (var item in lbxLog.Items)
            {
                if (item == null)
                {
                    continue;
                }

                buffer.AppendLine(item.ToString());
            }

            Clipboard.SetText(buffer.ToString());
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            lbxLog.Items.Clear();
        }

        private void rtbScript_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\t')
            {
                int pos = rtbScript.SelectionStart;

                rtbScript.Text = rtbScript.Text.Insert(pos, "    ");
                rtbScript.SelectionStart = pos + 4;

                e.Handled = true;
            }
        }
    }
}