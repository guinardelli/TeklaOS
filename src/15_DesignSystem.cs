internal static class DesignSystem
{
    // --- Paleta Minimalista (Cinzas + Destaque unico) ---
    public static readonly Color C_Fundo = Color.FromArgb(245, 245, 245);
    public static readonly Color C_Superficie = Color.White;
    public static readonly Color C_Cabecalho = Color.FromArgb(38, 38, 38);
    public static readonly Color C_TextoPrimario = Color.FromArgb(26, 26, 26);
    public static readonly Color C_TextoSecundario = Color.FromArgb(102, 102, 102);
    public static readonly Color C_TextoClaro = Color.White;
    public static readonly Color C_TextoDesabilitado = Color.FromArgb(160, 160, 160);
    public static readonly Color C_Borda = Color.FromArgb(218, 218, 218);
    public static readonly Color C_BordaLeve = Color.FromArgb(234, 234, 234);
    public static readonly Color C_Hover = Color.FromArgb(240, 240, 240);

    // Destaque (unico tom de azul — usado para acoes primarias)
    public static readonly Color C_Destaque = Color.FromArgb(37, 99, 235);
    public static readonly Color C_DestaqueHover = Color.FromArgb(29, 78, 216);
    public static readonly Color C_DestaqueSuave = Color.FromArgb(239, 246, 255);

    // Semanticas (minimas)
    public static readonly Color C_Sucesso = Color.FromArgb(22, 163, 74);
    public static readonly Color C_Erro = Color.FromArgb(220, 38, 38);
    public static readonly Color C_Atencao = Color.FromArgb(180, 130, 0);

    // Tipografia
    public static readonly Font F_Titulo = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
    public static readonly Font F_Secao = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
    public static readonly Font F_Texto = new Font("Segoe UI", 9.5F, FontStyle.Regular);
    public static readonly Font F_TextoPequeno = new Font("Segoe UI", 8.5F, FontStyle.Regular);
    public static readonly Font F_Botao = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
    public static readonly Font F_Mono = new Font("Consolas", 9F);

    // --- Helpers UI ---

    public static Label CriarRotuloSecao(string texto)
    {
        var label = new Label();
        label.Text = texto.ToUpperInvariant();
        label.Font = F_TextoPequeno;
        label.ForeColor = C_TextoSecundario;
        label.AutoSize = false;
        label.Dock = DockStyle.Top;
        label.Height = 28;
        label.TextAlign = ContentAlignment.BottomLeft;
        label.Padding = new Padding(2, 0, 0, 0);
        label.Margin = new Padding(0, 6, 0, 0);
        return label;
    }

    public static Panel CriarSeparador()
    {
        var line = new Panel();
        line.Height = 1;
        line.Dock = DockStyle.Top;
        line.BackColor = C_BordaLeve;
        line.Margin = new Padding(0, 2, 0, 6);
        return line;
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
        if (layout == null || control == null) return;
        int row = layout.RowCount;
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(control, 0, row);
        layout.RowCount = row + 1;
    }

    public static Button CriarBotao(string texto, bool primario)
    {
        var btn = new Button();
        btn.Text = texto;
        btn.Height = 34;
        btn.Dock = DockStyle.Fill;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 1;
        btn.Cursor = Cursors.Hand;
        btn.Font = F_Botao;
        btn.TextAlign = ContentAlignment.MiddleLeft;
        btn.Padding = new Padding(8, 0, 8, 0);
        btn.Margin = new Padding(0, 0, 0, 3);

        if (primario)
        {
            btn.BackColor = C_Destaque;
            btn.ForeColor = C_TextoClaro;
            btn.FlatAppearance.BorderColor = C_DestaqueHover;
            btn.FlatAppearance.MouseDownBackColor = C_DestaqueHover;
            btn.MouseEnter += (s, e) => { btn.BackColor = C_DestaqueHover; };
            btn.MouseLeave += (s, e) => { btn.BackColor = C_Destaque; };
        }
        else
        {
            btn.BackColor = C_Superficie;
            btn.ForeColor = C_TextoPrimario;
            btn.FlatAppearance.BorderColor = C_Borda;
            btn.FlatAppearance.MouseDownBackColor = C_Hover;
            btn.MouseEnter += (s, e) => { btn.BackColor = C_Hover; };
            btn.MouseLeave += (s, e) => { btn.BackColor = C_Superficie; };
        }

        return btn;
    }

    public static Button CriarBotaoPerigo(string texto)
    {
        var btn = CriarBotao(texto, false);
        btn.ForeColor = C_Erro;
        btn.MouseEnter += (s, e) => { btn.ForeColor = C_Erro; };
        btn.MouseLeave += (s, e) => { btn.ForeColor = C_Erro; };
        return btn;
    }

    public static TextBox CriarCampoEntrada(bool multiline, int altura)
    {
        var txt = new TextBox();
        txt.Multiline = multiline;
        txt.ScrollBars = multiline ? ScrollBars.Vertical : ScrollBars.None;
        txt.Dock = DockStyle.Fill;
        txt.Height = altura;
        txt.Font = F_Texto;
        txt.Margin = new Padding(0, 0, 0, 4);
        txt.BackColor = C_Superficie;
        txt.ForeColor = C_TextoPrimario;
        txt.BorderStyle = BorderStyle.FixedSingle;
        return txt;
    }

    public static Label CriarLabelInfo(string texto)
    {
        var label = new Label();
        label.Text = texto;
        label.ForeColor = C_TextoSecundario;
        label.Font = F_TextoPequeno;
        label.AutoSize = true;
        label.Dock = DockStyle.Fill;
        label.Margin = new Padding(0, 0, 0, 4);
        return label;
    }
}
