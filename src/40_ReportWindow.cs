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
        using (var copyButton = new Button())
        using (var closeButton = new Button())
        using (var panel = new TableLayoutPanel())
        using (var buttonPanel = new FlowLayoutPanel())
        {
            form.Text = title;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.Size = new Size(720, 520);
            form.MinimumSize = new Size(540, 360);
            form.BackColor = DesignSystem.C_FundoForm;
            form.Font = DesignSystem.F_Texto;

            textBox.Multiline = true;
            textBox.ReadOnly = true;
            textBox.ScrollBars = RichTextBoxScrollBars.Both;
            textBox.WordWrap = false;
            textBox.Dock = DockStyle.Fill;
            textBox.Font = DesignSystem.F_Mono;
            textBox.BackColor = DesignSystem.C_CardFundo;
            textBox.ForeColor = DesignSystem.C_TextoPrimario;
            textBox.Clear();
            AppendColoredText(textBox, text);

            copyButton.Text = "Copiar";
            copyButton.AutoSize = true;
            copyButton.FlatStyle = FlatStyle.Flat;
            copyButton.FlatAppearance.BorderSize = 1;
            copyButton.FlatAppearance.BorderColor = DesignSystem.C_Borda;
            copyButton.BackColor = DesignSystem.C_CardFundo;
            copyButton.ForeColor = DesignSystem.C_TextoPrimario;
            copyButton.Click += delegate { 
                try
                {
                    Clipboard.SetText(textBox.Text); 
                }
                catch
                {
                    MessageBox.Show("Nao foi possivel copiar. Tente novamente.");
                }
            };

            closeButton.Text = "Fechar";
            closeButton.AutoSize = true;
            closeButton.DialogResult = DialogResult.OK;
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.FlatAppearance.BorderSize = 1;
            closeButton.FlatAppearance.BorderColor = DesignSystem.C_Borda;
            closeButton.BackColor = DesignSystem.C_CardFundo;
            closeButton.ForeColor = DesignSystem.C_TextoPrimario;

            var exportButton = new Button();
            exportButton.Text = "Exportar";
            exportButton.AutoSize = true;
            exportButton.FlatStyle = FlatStyle.Flat;
            exportButton.FlatAppearance.BorderSize = 1;
            exportButton.FlatAppearance.BorderColor = DesignSystem.C_Borda;
            exportButton.BackColor = DesignSystem.C_CardFundo;
            exportButton.ForeColor = DesignSystem.C_DestaqueAzul;
            exportButton.Click += delegate {
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
                            MessageBox.Show("Arquivo salvo com sucesso!", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception exFile)
                {
                    MessageBox.Show("Nao foi possivel salvar o arquivo: " + exFile.Message);
                }
            };

            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.AutoSize = true;
            buttonPanel.Controls.Add(closeButton);
            buttonPanel.Controls.Add(exportButton);
            buttonPanel.Controls.Add(copyButton);

            panel.Dock = DockStyle.Fill;
            panel.ColumnCount = 1;
            panel.RowCount = 2;
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.Controls.Add(textBox, 0, 0);
            panel.Controls.Add(buttonPanel, 0, 1);

            form.Controls.Add(panel);
            form.AcceptButton = closeButton;
            form.CancelButton = closeButton;

            form.ShowDialog();
        }
    }

    private static void AppendColoredText(RichTextBox box, string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        string normalized = text.Replace("\r\n", "\n");
        string[] lines = normalized.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            Color color = DesignSystem.C_TextoPrimario;

            if (line.StartsWith("[OK]"))
            {
                color = Color.ForestGreen;
            }
            else if (line.StartsWith("[X]"))
            {
                color = Color.Firebrick;
            }
            else if (line.StartsWith("[~]"))
            {
                color = Color.FromArgb(180, 130, 0);
            }
            else if (line.StartsWith("==="))
            {
                color = DesignSystem.C_Cabecalho;
            }
            else if (line.Contains("APROVADO"))
            {
                color = Color.ForestGreen;
            }
            else if (line.Contains("REPROVADO"))
            {
                color = Color.Firebrick;
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
