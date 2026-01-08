internal static class ReportWindow
{
    public static void ShowReport(string text)
    {
        using (var form = new Form())
        using (var textBox = new TextBox())
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
            textBox.ScrollBars = ScrollBars.Both;
            textBox.WordWrap = false;
            textBox.Dock = DockStyle.Fill;
            textBox.Font = new Font("Consolas", 9f);
            textBox.Text = text;

            copyButton.Text = "Copiar";
            copyButton.AutoSize = true;
            copyButton.Click += delegate { Clipboard.SetText(text); };

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
}
