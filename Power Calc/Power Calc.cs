using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Parsing;

namespace Power_Calc
{
    public partial class PowerCalc : Form
    {
        public PowerCalc()
        {
            InitializeComponent();
            ParserFactory.InitializeFactoryFromResource("Power_Calc.Calculator.cgt");
            m_parser = new MyParser(Output);
            m_index = 2;
        }

        MyParser m_parser;
        int m_index;

        private void HandleKeys(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ListViewItem inputText = new ListViewItem(Input.Text);
                inputText.ForeColor = Color.Gray;

                ListViewItem outputText = new ListViewItem();
                int iAnswer = (Output.Items.Count / 2) + 1;
                if (Input.Text.Contains("=") && !Input.Text.StartsWith("set "))
                {
                    Input.Text = "set " + Input.Text;
                }
                if (!m_parser.Parse(Input.Text))
                {
                    outputText.Text = String.Format("    {0}", m_parser.ErrorString);
                    outputText.ForeColor = Color.Red;
                    inputText.Text = Input.Text;
                }
                else
                {
                    outputText.Text = String.Format("{0,6} = {1}", "$" + iAnswer, m_parser.Result.StringValue);
                    outputText.ForeColor = Color.Black;
                    Input.Text = "";
                    inputText.Text = m_parser.Result.Expression;
                }

                Output.Items.Add(inputText);
                Output.Items.Add(outputText);

                outputText.EnsureVisible();
                Output.Columns[0].Width = Output.ClientSize.Width;
                PerformLayout();
                base.OnClientSizeChanged(e);
                m_index = Output.Items.Count;
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (m_index < Output.Items.Count - 2)
                {
                    m_index += 2;
                    Input.Text = String.Format("({0})", Output.Items[m_index].Text);
                    Input.SelectionStart = Input.Text.Length;
                    Input.ScrollToCaret();
                }
                else
                {
                    Input.Text = "";
                }
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (m_index > 0)
                {
                    m_index -= 2;
                }
                if (Output.Items.Count > 0)
                {
                    Input.Text = Output.Items[m_index].Text;
                    Input.SelectionStart = Input.Text.Length;
                    Input.ScrollToCaret();
                }
            }
            else if (Input.Text.Length == 0 && Output.Items.Count > 0 &&
                (e.KeyCode == Keys.Add || e.KeyCode == Keys.Subtract ||
                e.KeyCode == Keys.Multiply || e.KeyCode == Keys.Divide))
            {
                Input.Text = "ans";
                Input.SelectionStart = Input.Text.Length;
                Input.ScrollToCaret();
            }
        }

        private void DiscardEnter(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                e.Handled = true;
            }
        }

        private void ResizeColumns(object sender, EventArgs e)
        {
            Output.Columns[0].Width = Output.ClientSize.Width;
            PerformLayout();
            base.OnClientSizeChanged(e);
        }
    }
}