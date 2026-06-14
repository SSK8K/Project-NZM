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
            this.Size = new Size(1200, 900);
            this.StartPosition = FormStartPosition.CenterScreen;

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

            txtGrammar.Text = @"S->aA
A->aX|b
X->bS";

            btnAnalyze = new Button
            {
                Text = "Analyze & Draw DFA",
                Location = new Point(10, 200),
                Size = new Size(200, 40),
                BackColor = Color.LawnGreen,
                Font = new Font("Arial", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnAnalyze.Click += BtnAnalyze_Click;

            pnlInput.Controls.AddRange(new Control[] { lblGrammar, txtGrammar, btnAnalyze });

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
                Size = new Size(1160, 475),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.MintCream
            };
            pnlGraph.Paint += PnlGraph_Paint;

            this.Controls.AddRange(new Control[] { pnlInput, pnlResult, lblGraph, pnlGraph });
        }
        private void BtnAnalyze_Click(object sender, EventArgs e)
        {
            try
            {
                var lines = txtGrammar.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                currentGrammar = Grammer.Parse(lines);
                converter = new GrammarConverter(currentGrammar);

                var grammarType = converter.DetermineType();
                bool isRegular = (grammarType == GrammerType.RightRegular ||
                                 grammarType == GrammerType.LeftRegular);

                rtbResult.Clear();
                rtbResult.SelectionFont = new Font("Consolas", 10, FontStyle.Bold);
                rtbResult.SelectionColor = Color.RoyalBlue;
                rtbResult.AppendText("=== GRAMMAR ANALYSIS === \r\n\r\nنکاتی در مورد نمودار:\r\n1.اگر استیت شروع و پایانی یکی باشند خط مشکی دور استیت شروع پررنگ می شود");
                rtbResult.AppendText("\r\n2.اگر استیت پایانی استیت شروع نباشد به رنگ زرد در می آید");
                rtbResult.AppendText("\r\n3.(D,F):در تمامی گراف ها استیت های مقابل به صورت دیفالت وجود دارند\r\n\r\n");
                rtbResult.SelectionColor = Color.Black;
                rtbResult.SelectionFont = new Font("Consolas", 10, FontStyle.Regular);
                rtbResult.AppendText($"Start Symbol: {currentGrammar.Startsymbol}\r\n");
                rtbResult.AppendText($"Non-terminals: {string.Join(", ", currentGrammar.Nonterminals)}\r\n");
                rtbResult.AppendText($"Terminals: {string.Join(", ", currentGrammar.Terminals)}\r\n\r\n");

                rtbResult.SelectionFont = new Font("Consolas", 10, FontStyle.Bold);
                rtbResult.AppendText($"Grammar Type: {grammarType}\r\n\r\n");

                if (isRegular)
                {
                    rtbResult.SelectionColor = Color.Green;
                    rtbResult.AppendText("✓ This is a REGULAR grammar!\r\n\r\n");
                    rtbResult.SelectionColor = Color.Black;

                    currentDfa = converter.ConvertToDFA();

                    rtbResult.AppendText("=== DFA INFORMATION ===\r\n");
                    rtbResult.AppendText($"States: {string.Join(", ", currentDfa.Allstates)}\r\n");
                    rtbResult.AppendText($"Start State: {currentDfa.StartState}\r\n");
                    rtbResult.AppendText($"Final States: {string.Join(", ", currentDfa.Finalstates)}\r\n\r\n");

                    rtbResult.AppendText("Transitions:\r\n");
                    foreach (var t in currentDfa.Transitions)
                    {
                        rtbResult.AppendText($"  {t.FromState} --{t.Symbol}--> {t.ToState}\r\n");
                    }
                }
                else
                {
                    rtbResult.SelectionColor = Color.Red;
                    rtbResult.AppendText("✗ This is NOT a regular grammar!\r\n");
                    rtbResult.SelectionColor = Color.Black;
                    currentDfa = null;
                }

                pnlGraph.Invalidate();

                if (isRegular)
                {
                    MessageBox.Show("Grammar is regular! DFA generated successfully.",
                                  "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Grammar is not regular. DFA cannot be generated.",
                                  "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Conflict detected"))
                {
                    MessageBox.Show($"⚠️ Warning: {ex.Message}\n\nThis grammar is non-deterministic and produces an NFA, not a DFA.\n\nPlease check your grammar rules.",
                                   "Non-Deterministic Grammar",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Warning);
                    currentDfa = null;
                    pnlGraph.Invalidate();
                }
                else
                {
                    MessageBox.Show($"Error: {ex.Message}", "Conversion Failed",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    currentDfa = null;
                    pnlGraph.Invalidate();
                }
            }
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
            g2.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            var states = currentDfa.Allstates.ToList();
            int radius = 35;
            int centerX = pnlGraph.Width / 2;
            int centerY = pnlGraph.Height / 2;

            var positions = new Dictionary<string, Point>();
            for (int i = 0; i < states.Count; i++)
            {
                double angle = 2 * Math.PI * i / states.Count - Math.PI / 2;
                int x = centerX + (int)(180 * Math.Cos(angle));
                int y = centerY + (int)(180 * Math.Sin(angle));
                positions[states[i]] = new Point(x, y);
            }
            if (positions.ContainsKey("S"))
            {
                Point sPos = positions["S"];
                positions["S"] = new Point(sPos.X, sPos.Y + 30);
            }

            var transitionGroups = new Dictionary<string, List<char>>();
            foreach (var t in currentDfa.Transitions)
            {
                string key = $"{t.FromState}->{t.ToState}";
                if (!transitionGroups.ContainsKey(key))
                    transitionGroups[key] = new List<char>();
                if (!transitionGroups[key].Contains(t.Symbol))
                    transitionGroups[key].Add(t.Symbol);
            }
            foreach (var group in transitionGroups)
            {
                var parts = group.Key.Split(new[] { "->" }, StringSplitOptions.None);
                string fromState = parts[0];
                string toState = parts[1];
                string labels = string.Join(",", group.Value);

                if (positions.ContainsKey(fromState) && positions.ContainsKey(toState))
                {
                    if (fromState == toState)
                    {
                        DrawSelfLoop(g2, positions[fromState], labels, radius);
                    }
                    else
                    {
                        DrawArrow(g2, positions[fromState], positions[toState], labels, radius);
                    }
                }
            }

            foreach (var state in states)
            {
                Point pos = positions[state];
                bool isFinal = currentDfa.Finalstates.Contains(state);
                bool isStart = state == currentDfa.StartState;

                g2.DrawEllipse(Pens.Black, pos.X - radius, pos.Y - radius, radius * 2, radius * 2);
                if (isStart && isFinal)
                {
                    using (Pen thickPen = new Pen(Color.Black, 2.5f))
                    {
                        g2.DrawEllipse(thickPen, pos.X - radius, pos.Y - radius, radius * 2, radius * 2);
                    }

                    g2.DrawEllipse(Pens.Black, pos.X - radius + 5, pos.Y - radius + 5,
                                  (radius - 5) * 2, (radius - 5) * 2);

                    g2.FillEllipse(Brushes.LightGreen, pos.X - radius + 1, pos.Y - radius + 1,
                                  radius * 2 - 2, radius * 2 - 2);

                    using (Pen yellowPen = new Pen(Color.Gold, 1.5f))
                    {
                        g2.DrawEllipse(yellowPen, pos.X - radius + 3, pos.Y - radius + 3,
                                      (radius - 3) * 2, (radius - 3) * 2);
                    }
                }

                if (isFinal)
                {
                    g2.DrawEllipse(Pens.Black, pos.X - radius + 5, pos.Y - radius + 5,
                                  (radius - 5) * 2, (radius - 5) * 2);
                }

                if (isStart)
                {
                    g2.FillEllipse(Brushes.LightGreen, pos.X - radius + 1, pos.Y - radius + 1,
                                  radius * 2 - 2, radius * 2 - 2);
                }
                else if (isFinal)
                {
                    g2.FillEllipse(Brushes.Yellow, pos.X - radius + 1, pos.Y - radius + 1,
                                  radius * 2 - 2, radius * 2 - 2);
                }
                else
                {
                    g2.FillEllipse(Brushes.White, pos.X - radius + 1, pos.Y - radius + 1,
                                  radius * 2 - 2, radius * 2 - 2);
                }

                if (isStart)
                {
                    int arrowX = pos.X - radius - 15;
                    int arrowY = pos.Y;
                    g2.DrawLine(new Pen(Color.Black, 2), arrowX, arrowY, pos.X - radius, arrowY);
                    g2.DrawLine(new Pen(Color.Black, 2), pos.X - radius - 6, arrowY - 4, pos.X - radius, arrowY);
                    g2.DrawLine(new Pen(Color.Black, 2), pos.X - radius - 6, arrowY + 4, pos.X - radius, arrowY);
                }

                using (var font = new Font("Arial", 11, FontStyle.Bold))
                {
                    var size = g2.MeasureString(state, font);
                    g2.DrawString(state, font, Brushes.Black,
                                 pos.X - size.Width / 2, pos.Y - size.Height / 2);
                }
            }
        }
        private void DrawSelfLoop(Graphics g, Point center, string labels, int radius)
        {
            int loopSize = 40;
            int loopX = center.X - loopSize / 2;
            int loopY = center.Y - radius - loopSize;

            g.DrawEllipse(new Pen(Color.Blue, 2), loopX, loopY, loopSize, loopSize);

            int arrowX = center.X;
            int arrowY = center.Y - radius - 5;
            g.DrawLine(new Pen(Color.Blue, 2), arrowX, arrowY, arrowX - 5, arrowY - 7);
            g.DrawLine(new Pen(Color.Blue, 2), arrowX, arrowY, arrowX + 5, arrowY - 7);

            using (var font = new Font("Arial", 9, FontStyle.Bold))
            {
                var size = g.MeasureString(labels, font);
                float labelX = center.X - size.Width / 2;
                float labelY = center.Y - radius - loopSize - 5;

                g.FillRectangle(Brushes.White, labelX - 2, labelY - 2, size.Width + 4, size.Height + 4);
                g.DrawString(labels, font, Brushes.Red, labelX, labelY);
            }
        }
        private void DrawArrow(Graphics g, Point from, Point to, string labels, int radius)
        {
            double angle = Math.Atan2(to.Y - from.Y, to.X - from.X);
            int startX = from.X + (int)(radius * Math.Cos(angle));
            int startY = from.Y + (int)(radius * Math.Sin(angle));
            int endX = to.X - (int)(radius * Math.Cos(angle));
            int endY = to.Y - (int)(radius * Math.Sin(angle));

            g.DrawLine(new Pen(Color.Blue, 2), startX, startY, endX, endY);

            double arrowAngle = Math.PI / 6;
            int arrowSize = 12;
            int arrowX = endX;
            int arrowY = endY;
            int arrowX1 = (int)(arrowX - arrowSize * Math.Cos(angle - arrowAngle));
            int arrowY1 = (int)(arrowY - arrowSize * Math.Sin(angle - arrowAngle));
            int arrowX2 = (int)(arrowX - arrowSize * Math.Cos(angle + arrowAngle));
            int arrowY2 = (int)(arrowY - arrowSize * Math.Sin(angle + arrowAngle));

            g.DrawLine(new Pen(Color.Blue, 2), arrowX, arrowY, arrowX1, arrowY1);
            g.DrawLine(new Pen(Color.Blue, 2), arrowX, arrowY, arrowX2, arrowY2);

            int midX = (startX + endX) / 2;
            int midY = (startY + endY) / 2;
            double perpAngle = angle + Math.PI / 2;
            int offset = 12;
            int labelX = midX + (int)(offset * Math.Cos(perpAngle));
            int labelY = midY + (int)(offset * Math.Sin(perpAngle));

            using (var font = new Font("Arial", 9, FontStyle.Bold))
            {
                var size = g.MeasureString(labels, font);

                g.FillRectangle(Brushes.White, labelX - size.Width / 2 - 2, labelY - size.Height / 2 - 2,
                               size.Width + 4, size.Height + 4);
                g.DrawString(labels, font, Brushes.Red,
                            labelX - size.Width / 2, labelY - size.Height / 2);
            }
        }
    }
}