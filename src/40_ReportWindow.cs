internal static class ReportWindow
{
    public static void ShowReport(string text)
    {
        ShowReport(text, "Informacoes do Modelo");
    }

    public static void ShowReport(string text, string title)
    {
        using (var form = new Form())
        using (var textBox = new RichTextBox())
        using (var txtFilter = new TextBox())
        using (var lblFilterStatus = new Label())
        using (var copyButton = new Button())
        using (var closeButton = new Button())
        using (var exportButton = new Button())
        using (var panel = new TableLayoutPanel())
        using (var filterPanel = new TableLayoutPanel())
        using (var buttonPanel = new FlowLayoutPanel())
        {
            string reportText = text ?? string.Empty;

            form.Text = title;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.Size = new Size(720, 520);
            form.MinimumSize = new Size(500, 340);
            form.BackColor = DesignSystem.C_Fundo;
            form.Font = DesignSystem.F_Texto;

            textBox.Multiline = true;
            textBox.ReadOnly = true;
            textBox.ScrollBars = RichTextBoxScrollBars.Both;
            textBox.WordWrap = false;
            textBox.Dock = DockStyle.Fill;
            textBox.Font = DesignSystem.F_Mono;
            textBox.BackColor = DesignSystem.C_Superficie;
            textBox.ForeColor = DesignSystem.C_TextoPrimario;
            textBox.BorderStyle = BorderStyle.FixedSingle;

            filterPanel.Dock = DockStyle.Fill;
            filterPanel.ColumnCount = 2;
            filterPanel.RowCount = 1;
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            filterPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            filterPanel.Margin = new Padding(0, 0, 0, 6);

            txtFilter.Dock = DockStyle.Fill;
            txtFilter.Margin = new Padding(0, 0, 8, 0);
            txtFilter.Font = DesignSystem.F_Texto;
            txtFilter.BorderStyle = BorderStyle.FixedSingle;
            txtFilter.BackColor = DesignSystem.C_Superficie;
            txtFilter.ForeColor = DesignSystem.C_TextoPrimario;

            lblFilterStatus.Text = "";
            lblFilterStatus.AutoSize = true;
            lblFilterStatus.Margin = new Padding(0, 6, 0, 0);
            lblFilterStatus.ForeColor = DesignSystem.C_TextoSecundario;
            lblFilterStatus.Font = DesignSystem.F_TextoPequeno;

            filterPanel.Controls.Add(txtFilter, 0, 0);
            filterPanel.Controls.Add(lblFilterStatus, 1, 0);

            // --- Botoes ---
            Action<Button, string, bool> styleButton = delegate(Button b, string t, bool primary)
            {
                b.Text = t;
                b.AutoSize = false;
                b.Size = new Size(90, 30);
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderSize = 1;
                b.Font = DesignSystem.F_Botao;
                b.Cursor = Cursors.Hand;

                if (primary)
                {
                    b.BackColor = DesignSystem.C_Destaque;
                    b.ForeColor = DesignSystem.C_TextoClaro;
                    b.FlatAppearance.BorderColor = DesignSystem.C_DestaqueHover;
                }
                else
                {
                    b.BackColor = DesignSystem.C_Superficie;
                    b.ForeColor = DesignSystem.C_TextoPrimario;
                    b.FlatAppearance.BorderColor = DesignSystem.C_Borda;
                }
            };

            styleButton(copyButton, "Copiar", false);
            styleButton(exportButton, "Exportar", false);
            styleButton(closeButton, "Fechar", true);
            closeButton.DialogResult = DialogResult.OK;

            copyButton.Click += delegate
            {
                try { Clipboard.SetText(textBox.Text); }
                catch { MessageBox.Show("Nao foi possivel copiar."); }
            };

            exportButton.Click += delegate
            {
                try
                {
                    using (var sfd = new SaveFileDialog())
                    {
                        sfd.Filter = "Texto (*.txt)|*.txt|CSV (*.csv)|*.csv|Todos (*.*)|*.*";
                        sfd.DefaultExt = "txt";
                        sfd.FileName = string.Format("TeklaOS_Relatorio_{0}", DateTime.Now.ToString("yyyyMMdd_HHmm"));
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            File.WriteAllText(sfd.FileName, textBox.Text, Encoding.UTF8);
                            MessageBox.Show("Arquivo salvo.", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception exFile)
                {
                    MessageBox.Show("Erro ao salvar: " + exFile.Message);
                }
            };

            Action refreshFilter = delegate
            {
                int matches;
                string filtered = FilterReportLines(reportText, txtFilter.Text, out matches);
                textBox.Clear();
                AppendColoredText(textBox, filtered);

                if (string.IsNullOrWhiteSpace(txtFilter.Text))
                {
                    lblFilterStatus.Text = "";
                }
                else if (matches == 0)
                {
                    lblFilterStatus.Text = "0 resultados";
                    lblFilterStatus.ForeColor = DesignSystem.C_Erro;
                }
                else
                {
                    lblFilterStatus.Text = string.Format("{0} linha(s)", matches);
                    lblFilterStatus.ForeColor = DesignSystem.C_TextoSecundario;
                }
            };

            txtFilter.TextChanged += delegate { refreshFilter(); };

            txtFilter.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Escape)
                {
                    txtFilter.Text = string.Empty;
                    e.SuppressKeyPress = true;
                }
            };

            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.AutoSize = true;
            buttonPanel.Padding = new Padding(0, 6, 0, 0);
            buttonPanel.WrapContents = false;
            buttonPanel.Controls.Add(closeButton);
            buttonPanel.Controls.Add(exportButton);
            buttonPanel.Controls.Add(copyButton);

            panel.Dock = DockStyle.Fill;
            panel.ColumnCount = 1;
            panel.RowCount = 3;
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.Padding = new Padding(12);
            panel.Controls.Add(filterPanel, 0, 0);
            panel.Controls.Add(textBox, 0, 1);
            panel.Controls.Add(buttonPanel, 0, 2);

            form.Controls.Add(panel);
            form.AcceptButton = closeButton;
            form.CancelButton = closeButton;

            refreshFilter();
            form.ShowDialog();
        }
    }

    private static string FilterReportLines(string source, string query, out int matches)
    {
        matches = 0;
        if (string.IsNullOrEmpty(source)) return string.Empty;

        string normalized = source.Replace("\r\n", "\n");
        string[] lines = normalized.Split('\n');

        if (string.IsNullOrWhiteSpace(query))
        {
            matches = lines.Length;
            return source;
        }

        string search = query.Trim();
        var sb = new StringBuilder();
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (line.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0) continue;
            if (matches > 0) sb.AppendLine();
            sb.Append(line);
            matches++;
        }

        return sb.ToString();
    }

    private static void AppendColoredText(RichTextBox box, string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        string normalized = text.Replace("\r\n", "\n");
        string[] lines = normalized.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            Color color = DesignSystem.C_TextoPrimario;

            if (line.StartsWith("[OK]"))
            {
                color = DesignSystem.C_Sucesso;
            }
            else if (line.StartsWith("[X]"))
            {
                color = DesignSystem.C_Erro;
            }
            else if (line.StartsWith("[~]"))
            {
                color = DesignSystem.C_Atencao;
            }
            else if (line.StartsWith("==="))
            {
                color = DesignSystem.C_Cabecalho;
            }
            else if (line.Contains("APROVADO"))
            {
                color = DesignSystem.C_Sucesso;
            }
            else if (line.Contains("REPROVADO"))
            {
                color = DesignSystem.C_Erro;
            }

            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;
            box.SelectionColor = color;
            box.AppendText(line);
            if (i < lines.Length - 1)
            {
                box.AppendText(Environment.NewLine);
            }
        }

        box.SelectionColor = box.ForeColor;
    }
}
