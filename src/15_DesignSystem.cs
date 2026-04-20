internal static class DesignSystem
{
    // --- Paleta de Cores (Design System) ---
    public static readonly Color C_FundoForm = Color.FromArgb(240, 242, 245);
    public static readonly Color C_CardFundo = Color.White;
    public static readonly Color C_Cabecalho = Color.FromArgb(0, 80, 150);
    public static readonly Color C_TextoPrimario = Color.FromArgb(30, 30, 30);
    public static readonly Color C_TextoSecundario = Color.FromArgb(100, 110, 120);
    public static readonly Color C_TextoClaro = Color.White;
    public static readonly Color C_TextoCabecalhoSec = Color.FromArgb(200, 220, 255);
    public static readonly Color C_Borda = Color.FromArgb(220, 224, 230);
    public static readonly Color C_Transparente = Color.Transparent;

    // Cores de Acao
    public static readonly Color C_BotaoHover = Color.FromArgb(245, 248, 255);
    public static readonly Color C_DestaqueAzul = Color.FromArgb(0, 120, 215);
    public static readonly Color C_DestaqueVermelho = Color.FromArgb(220, 53, 69);
    public static readonly Color C_FundoVermelhoSuave = Color.FromArgb(255, 245, 245);
    public static readonly Color C_FundoVermelhoHover = Color.FromArgb(255, 230, 230);

    // Tipografia
    public static readonly Font F_Titulo = new Font("Segoe UI", 12F, FontStyle.Bold);
    public static readonly Font F_Texto = new Font("Segoe UI", 10F, FontStyle.Regular);
    public static readonly Font F_Secao = new Font("Segoe UI", 10F, FontStyle.Bold);
    public static readonly Font F_Mono = new Font("Consolas", 9F);

    // --- Helpers UI ---
    public static GroupBox CriarGrupoDashboard(string titulo)
    {
        var group = new GroupBox();
        group.Text = titulo;
        group.Font = F_Secao;
        group.ForeColor = C_TextoPrimario;
        group.BackColor = C_CardFundo;
        group.Dock = DockStyle.Top;
        group.AutoSize = true;
        group.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        group.Padding = new Padding(12, 8, 12, 12);
        group.Margin = new Padding(0, 0, 0, 12);
        return group;
    }

    public static TableLayoutPanel CriarLayoutVertical()
    {
        var layout = new TableLayoutPanel();
        layout.ColumnCount = 1;
        layout.RowCount = 0;
        layout.Dock = DockStyle.Top;
        layout.AutoSize = true;
        layout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        layout.Margin = new Padding(0);
        layout.Padding = new Padding(0);
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        return layout;
    }

    public static void AdicionarLinha(TableLayoutPanel layout, Control control)
    {
        if (layout == null || control == null)
        {
            return;
        }

        int row = layout.RowCount;
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(control, 0, row);
        layout.RowCount = row + 1;
    }

    public static Label CriarLabelInfo(string texto)
    {
        var label = new Label();
        label.Text = texto;
        label.ForeColor = C_TextoSecundario;
        label.Font = F_Texto;
        label.AutoSize = true;
        label.Dock = DockStyle.Fill;
        label.Margin = new Padding(0, 0, 0, 8);
        return label;
    }

    public static Button CriarBotaoDashboard(string texto, bool ehPerigo)
    {
        var btn = new Button();
        btn.Text = texto;
        btn.Height = 36;
        btn.Dock = DockStyle.Fill;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 1;
        btn.FlatAppearance.BorderColor = C_Borda;
        btn.Cursor = Cursors.Hand;
        btn.Font = F_Texto;
        btn.TextAlign = ContentAlignment.MiddleLeft;
        btn.Padding = new Padding(12, 0, 12, 0);
        btn.Margin = new Padding(0, 0, 0, 8);
        btn.BackColor = ehPerigo ? C_FundoVermelhoSuave : C_CardFundo;
        btn.ForeColor = ehPerigo ? C_DestaqueVermelho : C_TextoPrimario;

        btn.MouseEnter += (s, e) => {
            btn.BackColor = ehPerigo ? C_FundoVermelhoHover : C_BotaoHover;
            if (!ehPerigo) btn.ForeColor = C_DestaqueAzul;
        };

        btn.MouseLeave += (s, e) => {
            btn.BackColor = ehPerigo ? C_FundoVermelhoSuave : C_CardFundo;
            btn.ForeColor = ehPerigo ? C_DestaqueVermelho : C_TextoPrimario;
        };
        return btn;
    }

    public static Button CriarBotaoHelp(string textoToolTip)
    {
        var btnHelp = new Button();
        btnHelp.Text = "[?]";
        btnHelp.Height = 36;
        btnHelp.Width = 36;
        btnHelp.FlatStyle = FlatStyle.Flat;
        btnHelp.FlatAppearance.BorderSize = 1;
        btnHelp.FlatAppearance.BorderColor = C_Borda;
        btnHelp.BackColor = C_CardFundo;
        btnHelp.ForeColor = C_DestaqueAzul;
        btnHelp.Font = F_Texto;
        btnHelp.Cursor = Cursors.Hand;
        btnHelp.Margin = new Padding(6, 0, 0, 0);
        btnHelp.Click += delegate {
            MessageBox.Show(
                textoToolTip,
                "Como usar",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        };
        return btnHelp;
    }

    // Retorna um TableLayoutPanel de 2 colunas:
    //   coluna 0 (Percent 100%) -> botao principal (texto nunca cortado)
    //   coluna 1 (Absolute 44px) -> botao [?]
    // Margem inferior padrao de 8px para separacao entre linhas.
    public static TableLayoutPanel CriarLinhaComHelp(Button mainButton, Button helpButton, int marginBottom)
    {
        var row = new TableLayoutPanel();
        row.ColumnCount = 2;
        row.RowCount = 1;
        row.Dock = DockStyle.Top;
        row.AutoSize = true;
        row.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        row.Margin = new Padding(0, 0, 0, marginBottom);
        row.Padding = new Padding(0);
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44F));
        row.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        // mainButton deve preencher a celula
        mainButton.Dock = DockStyle.Fill;
        mainButton.MinimumSize = new Size(0, 36);
        row.Controls.Add(mainButton, 0, 0);
        // helpButton alinhado verticalmente
        helpButton.Dock = DockStyle.Fill;
        helpButton.Margin = new Padding(4, 0, 0, 0);
        row.Controls.Add(helpButton, 1, 0);
        return row;
    }
}
