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
                    if (currentDfa == null) 
                    {
                        Graphics g = e.Graphics;
                g.DrawString("No DFA to display. Enter a regular grammar and click 'Analyze & Draw DFA'.",
                        new Font("Arial", 12), Brushes.Gray, new PointF(50, 150));
                        return;
                    }

                    Graphics g2 = e.Graphics;
                    g2.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                    var positions = new Dictionary<string, Point>();
                    var states = currentDfa.Allstates.ToList();
                    int radius = 35;
                    int centerX = pnlGraph.Width / 2;
                    int centerY = pnlGraph.Height / 2;

                    // Position states in a circle
                    for (int i = 0; i < states.Count; i++)
                    {
                        double angle = 2 * Math.PI * i / states.Count - Math.PI / 2;
                        int x = centerX + (int)(180 * Math.Cos(angle));
                        int y = centerY + (int)(180 * Math.Sin(angle));
                        positions[states[i]] = new Point(x, y);
                    }

                    // Draw transitions
                    foreach (var transition in currentDfa.Transitions)
                    {
                        if (positions.ContainsKey(transition.FromState) && positions.ContainsKey(transition.ToState))
                        {
                            DrawArrow(g2, positions[transition.FromState], positions[transition.ToState], 
                                    transition.Symbol.ToString(), radius);
                        }
                    }

                    // Draw states
                    foreach (var state in states)
                    {
                        Point pos = positions[state];
                        bool isFinal = currentDfa.Finalstates.Contains(state);
                        bool isStart = state == currentDfa.StartState;

                        // Draw circle
                        if (isFinal)
                        {
                            g2.DrawEllipse(Pens.Black, pos.X - radius, pos.Y - radius, radius * 2, radius * 2);
                            g2.DrawEllipse(Pens.Black, pos.X - radius + 4, pos.Y - radius + 4, 
                                        (radius - 4) * 2, (radius - 4) * 2);
                        }
                        else
                        {
                            g2.DrawEllipse(Pens.Black, pos.X - radius, pos.Y - radius, radius * 2, radius * 2);
                        }

                        // Fill state
                        if (isStart && isFinal)
                            g2.FillEllipse(Brushes.LightGreen, pos.X - radius + 1, pos.Y - radius + 1, 
                                        radius * 2 - 2, radius * 2 - 2);
                        else if (isStart)
                            g2.FillEllipse(Brushes.LightGreen, pos.X - radius + 1, pos.Y - radius + 1, 
                                        radius * 2 - 2, radius * 2 - 2);
                        else if (isFinal)
                            g2.FillEllipse(Brushes.LightYellow, pos.X - radius + 1, pos.Y - radius + 1, 
                                        radius * 2 - 2, radius * 2 - 2);
                        else
                            g2.FillEllipse(Brushes.White, pos.X - radius + 1, pos.Y - radius + 1, 
                                        radius * 2 - 2, radius * 2 - 2);

                        // Draw start arrow
                        if (isStart)
                        {
                            g2.DrawLine(Pens.Black, pos.X - radius - 20, pos.Y, pos.X - radius, pos.Y);
                            g2.DrawLine(Pens.Black, pos.X - radius - 10, pos.Y - 6, pos.X - radius, pos.Y);
                            g2.DrawLine(Pens.Black, pos.X - radius - 10, pos.Y + 6, pos.X - radius, pos.Y);
                        }

                        // Draw state name
                        var font = new Font("Arial", 11, FontStyle.Bold);
                        var size = g2.MeasureString(state, font);
                        g2.DrawString(state, font, Brushes.Black, 
                                    pos.X - size.Width / 2, pos.Y - size.Height / 2);
                    }
        }
         private void DrawArrow(Graphics g, Point from, Point to, string label, int radius)
        {
            //for sajad
        }
    }
}