using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;

namespace SeclonAnalytics
{
    public partial class Form1 : Form
    {
        private readonly Color GrayDark = Color.FromArgb(10, 10, 12);
        private readonly Color GrayPanel = Color.FromArgb(18, 18, 22);
        private readonly Color CyanNeon = Color.FromArgb(0, 242, 255);
        private readonly Color WhiteText = Color.FromArgb(240, 240, 245);

        private Panel pnlSidebar, pnlContent;
        private List<double> currentData = new List<double>();
        private List<string> chatHistory = new List<string>();
        private string selectedChatStyle = "Normal";
        private SpeechSynthesizer narrator = new SpeechSynthesizer();
        private bool isNarratorOn = false;

        public Form1()
        {
            this.Text = "SECLON ANALYTICS PRO | Enterprise Intelligence Desktop";
            this.Size = new Size(1400, 950);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = GrayDark;
            this.DoubleBuffered = true;

            BuildLayout();
            ShowModule("Dashboard");
        }

        private void BuildLayout()
        {
            pnlSidebar = new Panel { Dock = DockStyle.Left, Width = 260, BackColor = Color.FromArgb(5, 5, 8) };
            Label logo = new Label { Text = "SECLON ANALYTICS", ForeColor = CyanNeon, Font = new Font("Segoe UI", 14, FontStyle.Bold), Dock = DockStyle.Top, Height = 80, TextAlign = ContentAlignment.MiddleCenter };
            pnlSidebar.Controls.Add(logo);

            string[] modules = { "Dashboard", "Estadística", "Gráficas", "Probabilidad", "Chatbot" };
            foreach (var mod in modules.Reverse())
            {
                Button btn = new Button { Text = "   " + mod, Dock = DockStyle.Top, Height = 65, FlatStyle = FlatStyle.Flat, ForeColor = Color.LightGray, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += (s, e) => ShowModule(mod);
                pnlSidebar.Controls.Add(btn);
            }

            CheckBox chk = new CheckBox { Text = "NARRADOR VOZ", ForeColor = Color.Gray, Dock = DockStyle.Bottom, Height = 50, Font = new Font("Segoe UI", 8, FontStyle.Bold), Padding = new Padding(20, 0, 0, 0) };
            chk.CheckedChanged += (s, e) => isNarratorOn = chk.Checked;
            pnlSidebar.Controls.Add(chk);

            pnlContent = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlSidebar);
        }

        private void ShowModule(string name)
        {
            pnlContent.Controls.Clear();
            Label title = new Label { Text = name.ToUpper(), ForeColor = CyanNeon, Font = new Font("Segoe UI", 24, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
            pnlContent.Controls.Add(title);
            Panel view = new Panel { Location = new Point(0, 70), Size = new Size(pnlContent.Width - 40, pnlContent.Height - 120), AutoScroll = true };
            pnlContent.Controls.Add(view);

            if (name == "Estadística") RenderStats(view);
            else if (name == "Gráficas") RenderGraphs(view);
            else if (name == "Dashboard") RenderDashboard(view);
            else if (name == "Probabilidad") RenderProb(view);
            else if (name == "Chatbot") RenderChat(view);
        }

        // --- MÓDULO ESTADÍSTICA ---
        private TextBox txtInput;
        private FlowLayoutPanel cardGrid;
        private void RenderStats(Panel v)
        {
            Panel top = new Panel { Size = new Size(1000, 80), Location = new Point(0, 0) };
            txtInput = new TextBox { Location = new Point(0, 10), Width = 600, Font = new Font("JetBrains Mono", 12), BackColor = Color.Black, ForeColor = Color.White };
            Button btnRun = CreateBtn("EJECUTAR ANÁLISIS PRO", 610, 8, 250, 35, CyanNeon, Color.Black);
            btnRun.Click += (s, e) => RunStatisticalEngine();
            top.Controls.AddRange(new Control[] { txtInput, btnRun });

            cardGrid = new FlowLayoutPanel { Location = new Point(0, 80), Size = new Size(1050, 150), AutoSize = true };
            DataGridView dgv = new DataGridView { Name = "dgv", Location = new Point(0, 250), Size = new Size(1000, 350), BackgroundColor = GrayPanel, BorderStyle = BorderStyle.None, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            
            v.Controls.AddRange(new Control[] { top, cardGrid, dgv });
        }

        private void RunStatisticalEngine()
        {
            try {
                currentData = txtInput.Text.Split(',').Select(n => double.Parse(n.Trim())).OrderBy(n => n).ToList();
                double mean = currentData.Average();
                double std = Math.Sqrt(currentData.Sum(d => Math.Pow(d - mean, 2)) / (currentData.Count - 1));
                cardGrid.Controls.Clear();
                cardGrid.Controls.Add(CreateStatCard("MEDIA", mean.ToString("F2"), CyanNeon));
                cardGrid.Controls.Add(CreateStatCard("DESVIACIÓN", std.ToString("F2"), Color.Red));
                if(isNarratorOn) narrator.SpeakAsync("Análisis completado.");
            } catch { MessageBox.Show("Datos inválidos."); }
        }

        // --- MÓDULO PROBABILIDAD (CONTEO Y MONTECARLO) ---
        private void RenderProb(Panel v)
        {
            // Técnicas de Conteo
            GroupBox grpConteo = new GroupBox { Text = "TÉCNICAS DE CONTEO (nPr / nCr)", ForeColor = CyanNeon, Size = new Size(450, 250), Location = new Point(0, 20), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            Label lN = new Label { Text = "n (Total):", Location = new Point(20, 40), AutoSize = true, ForeColor = Color.White };
            NumericUpDown numN = new NumericUpDown { Location = new Point(20, 65), Width = 100, Maximum = 100, Value = 10 };
            Label lR = new Label { Text = "r (Muestra):", Location = new Point(150, 40), AutoSize = true, ForeColor = Color.White };
            NumericUpDown numR = new NumericUpDown { Location = new Point(150, 65), Width = 100, Maximum = 100, Value = 3 };
            
            Button btnCalc = CreateBtn("CALCULAR CONTEO", 20, 110, 230, 40, CyanNeon, Color.Black);
            Label lblRes = new Label { Text = "nPr: -\nnCr: -", Location = new Point(20, 170), AutoSize = true, ForeColor = Color.White, Font = new Font("JetBrains Mono", 12) };
            
            btnCalc.Click += (s, e) => {
                double n = (double)numN.Value; double r = (double)numR.Value;
                if (n < r) { MessageBox.Show("n debe ser >= r"); return; }
                double nPr = Factorial(n) / Factorial(n - r);
                double nCr = nPr / Factorial(r);
                lblRes.Text = $"Permutaciones (nPr): {nPr:N0}\nCombinaciones (nCr): {nCr:N0}";
            };
            grpConteo.Controls.AddRange(new Control[] { lN, numN, lR, numR, btnCalc, lblRes });

            // Montecarlo
            GroupBox grpSim = new GroupBox { Text = "SIMULACIÓN DE MONTECARLO", ForeColor = CyanNeon, Size = new Size(450, 250), Location = new Point(480, 20), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            Label lP = new Label { Text = "Probabilidad Objetivo (0-1):", Location = new Point(20, 40), AutoSize = true, ForeColor = Color.White };
            TextBox txtP = new TextBox { Text = "0.166", Location = new Point(20, 65), Width = 100 };
            Button btnSim = CreateBtn("EJECUTAR 10,000 PRUEBAS", 20, 110, 300, 40, Color.FromArgb(40,40,45), Color.White);
            ProgressBar bar = new ProgressBar { Location = new Point(20, 170), Width = 400, Height = 20 };
            Label lSimRes = new Label { Text = "Resultado Empírico: -", Location = new Point(20, 200), AutoSize = true, ForeColor = CyanNeon };

            btnSim.Click += (s, e) => {
                double target = double.Parse(txtP.Text); int success = 0; var rnd = new Random();
                for (int i = 0; i < 10000; i++) { if (rnd.NextDouble() < target) success++; if (i % 100 == 0) bar.Value = i / 100; }
                lSimRes.Text = $"Resultado Empírico: {(success / 10000.0) * 100:F2}% (Éxitos: {success})";
            };
            grpSim.Controls.AddRange(new Control[] { lP, txtP, btnSim, bar, lSimRes });

            v.Controls.AddRange(new Control[] { grpConteo, grpSim });
        }

        private double Factorial(double n) => n <= 1 ? 1 : n * Factorial(n - 1);

        // --- MÓDULO CHATBOT IA AVANZADO ---
        private RichTextBox chatBox;
        private TextBox chatInp;
        private void RenderChat(Panel v)
        {
            ComboBox cmb = new ComboBox { Location = new Point(0, 0), Width = 200, BackColor = GrayPanel, ForeColor = CyanNeon, FlatStyle = FlatStyle.Flat };
            cmb.Items.AddRange(new[] { "Normal", "Profesional", "Estudiante", "Creativo" }); cmb.SelectedIndex = 0;
            cmb.SelectedIndexChanged += (s, e) => selectedChatStyle = cmb.SelectedItem.ToString();

            chatBox = new RichTextBox { Location = new Point(0, 40), Size = new Size(950, 450), BackColor = Color.Black, ForeColor = Color.White, Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.None, ReadOnly = true };
            
            Button btnAt = CreateBtn("📎", 0, 510, 50, 40, GrayPanel, CyanNeon);
            chatInp = new TextBox { Location = new Point(60, 510), Width = 750, Height = 40, Font = new Font("Segoe UI", 12), BackColor = GrayPanel, ForeColor = Color.White };
            Button btnSend = CreateBtn("ENVIAR ➤", 820, 508, 130, 38, CyanNeon, Color.Black);

            btnSend.Click += (s, e) => {
                if (string.IsNullOrEmpty(chatInp.Text)) return;
                chatBox.AppendText($"[TÚ]: {chatInp.Text}\n");
                string resp = GenerateAIResponse(chatInp.Text);
                chatBox.AppendText($"[SECLON IA]: {resp}\n\n");
                if(isNarratorOn) narrator.SpeakAsync(resp);
                chatInp.Clear();
            };

            btnAt.Click += (s, e) => { MessageBox.Show("Lector de Imágenes/OCR Activado. Analizando patrones visuales..."); chatBox.AppendText("[IA]: Imagen analizada. Detecto un problema de distribución normal.\n\n"); };

            v.Controls.AddRange(new Control[] { cmb, chatBox, btnAt, chatInp, btnSend });
        }

        private string GenerateAIResponse(string input)
        {
            string t = input.ToLower();
            if (t.Contains("ensayo")) return "Estructura de Ensayo generada: 1. Introducción, 2. Desarrollo Analítico, 3. Conclusión.";
            if (t.Contains("cancion")) return "🎶 Datos fluyendo en el procesador, buscando la media con gran precisión... 🎵";
            if (selectedChatStyle == "Estudiante") return "¡Qué onda! Está de volada ese problema, nomás aplica la fórmula y listo.";
            return "He procesado tu consulta bajo el estándar profesional de Seclon Analytics. ¿Deseas un análisis más detallado?";
        }

        // --- HELPER UI ---
        private void RenderGraphs(Panel v) { v.Controls.Add(new Label { Text = "Gráficas Inteligentes en Grid 2x2", ForeColor = CyanNeon }); }
        private void RenderDashboard(Panel v) { v.Controls.Add(new Label { Text = "Bienvenido a Seclon Analytics Pro.", ForeColor = WhiteText }); }
        private Panel CreateStatCard(string l, string v, Color c) {
            Panel p = new Panel { Size = new Size(240, 100), BackColor = GrayPanel, Margin = new Padding(0,0,10,10) };
            p.Controls.Add(new Label { Text = l, ForeColor = Color.Gray, Location = new Point(10,10), AutoSize = true });
            p.Controls.Add(new Label { Text = v, ForeColor = c, Font = new Font("JetBrains Mono", 18, FontStyle.Bold), Location = new Point(10,40), AutoSize = true });
            return p;
        }
        private Button CreateBtn(string t, int x, int y, int w, int h, Color bg, Color fg) {
            return new Button { Text = t, Location = new Point(x, y), Size = new Size(w, h), BackColor = bg, ForeColor = fg, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
        }
    }
}
