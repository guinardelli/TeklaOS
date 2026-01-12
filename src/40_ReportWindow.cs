internal static class ReportWindow
{
    public static void ShowReport(string text)
    {
        using (var form = new Form())
        using (var textBox = new RichTextBox())
        using (var copyButton = new Button())
        using (var closeButton = new Button())
        using (var panel = new TableLayoutPanel())
        using (var buttonPanel = new FlowLayoutPanel())
        {
            form.Text = "Informacoes do Modelo";
            form.StartPosition = FormStartPosition.CenterScreen;
            form.Size = new Size(720, 520);
            form.MinimumSize = new Size(540, 360);

            textBox.Multiline = true;
            textBox.ReadOnly = true;
            textBox.ScrollBars = RichTextBoxScrollBars.Both;
            textBox.WordWrap = false;
            textBox.Dock = DockStyle.Fill;
            textBox.Font = new Font("Consolas", 9f);
            textBox.BackColor = SystemColors.Window;
            textBox.ForeColor = SystemColors.WindowText;
            textBox.Clear();
            AppendColoredText(textBox, text);

            copyButton.Text = "Copiar";
            copyButton.AutoSize = true;
            copyButton.Click += delegate { Clipboard.SetText(textBox.Text); };

            closeButton.Text = "Fechar";
            closeButton.AutoSize = true;
            closeButton.DialogResult = DialogResult.OK;

            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.AutoSize = true;
            buttonPanel.Controls.Add(closeButton);
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
            Color color = SystemColors.WindowText;

            if (line.StartsWith("[OK]"))
            {
                color = Color.ForestGreen;
            }
            else if (line.StartsWith("[X]"))
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
