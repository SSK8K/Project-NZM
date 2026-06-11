using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

namespace ProjectNZM
{
    public partial class MainForm : Form
    {
        private TextBox txtGrammar;
        private Button btnAnalyze;
        private RichTextBox rtbResult;
        private Panel pnlGraph;
        private Grammer currentGrammar;
        private GrammarConverter converter;
        private Dfa currentDfa;

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "DFA Converter - Grammar Analyzer";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Panel ورودی
            var pnlInput = new Panel 
            { 
                Location = new Point(10, 10), 
                Size = new Size(500, 250), 
                BorderStyle = BorderStyle.FixedSingle 
            };
            
            var lblGrammar = new Label 
            { 
                Text = "Grammar Rules (هر قانون در یک خط):", 
                Location = new Point(10, 10), 
                Size = new Size(300, 25),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            
            txtGrammar = new TextBox 
            { 
                Location = new Point(10, 40), 
                Size = new Size(475, 150), 
                Multiline = true, 
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 10)
            };
            
            // مثال پیش‌فرض
            txtGrammar.Text = @"S->aA
A->aX|b
X->bS";
            
            btnAnalyze = new Button 
            { 
                Text = "Analyze & Draw DFA", 
                Location = new Point(10, 200), 
                Size = new Size(200, 40), 
                BackColor = Color.LightBlue,
                Font = new Font("Arial", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnAnalyze.Click += BtnAnalyze_Click;
            
            pnlInput.Controls.AddRange(new Control[] { lblGrammar, txtGrammar, btnAnalyze });

            // Panel نتیجه
            var pnlResult = new Panel 
            { 
                Location = new Point(520, 10), 
                Size = new Size(650, 250), 
                BorderStyle = BorderStyle.FixedSingle 
            };
            
            var lblResult = new Label 
            { 
                Text = "Analysis Result:", 
                Location = new Point(10, 10), 
                Size = new Size(200, 25),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            
            rtbResult = new RichTextBox 
            { 
                Location = new Point(10, 40), 
                Size = new Size(625, 195), 
                ReadOnly = true,
                Font = new Font("Consolas", 10)
            };
            
            pnlResult.Controls.AddRange(new Control[] { lblResult, rtbResult });

            // Panel گراف
            var lblGraph = new Label 
            { 
                Text = "DFA Graph:", 
                Location = new Point(10, 270), 
                Size = new Size(200, 25),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            
            pnlGraph = new Panel 
            { 
                Location = new Point(10, 300), 
                Size = new Size(1160, 350), 
                BorderStyle = BorderStyle.FixedSingle, 
                BackColor = Color.White 
            };
            pnlGraph.Paint += PnlGraph_Paint;

            this.Controls.AddRange(new Control[] { pnlInput, pnlResult, lblGraph, pnlGraph });
        }
         private void BtnAnalyze_Click(object sender, EventArgs e)
        {
            //For sajad & Ali
        }
         private void PnlGraph_Paint(object sender, PaintEventArgs e)
        {
            //for ali
        }
         private void DrawArrow(Graphics g, Point from, Point to, string label, int radius)
        {
            //for sajad
        }
    }
}