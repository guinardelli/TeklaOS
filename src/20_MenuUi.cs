internal static class MenuUi
{
    // --- Paleta de Cores (Design System) ---
    private static readonly Color C_FundoForm = Color.FromArgb(240, 242, 245);
    private static readonly Color C_CardFundo = Color.White;
    private static readonly Color C_Cabecalho = Color.FromArgb(0, 80, 150);
    private static readonly Color C_TextoPrimario = Color.FromArgb(30, 30, 30);
    private static readonly Color C_TextoSecundario = Color.FromArgb(100, 110, 120);
    private static readonly Color C_TextoClaro = Color.White;
    private static readonly Color C_TextoCabecalhoSec = Color.FromArgb(200, 220, 255);
    private static readonly Color C_Borda = Color.FromArgb(220, 224, 230);
    private static readonly Color C_Transparente = Color.Transparent;

    // Cores de Acao
    private static readonly Color C_BotaoHover = Color.FromArgb(245, 248, 255);
    private static readonly Color C_DestaqueAzul = Color.FromArgb(0, 120, 215);
    private static readonly Color C_DestaqueVermelho = Color.FromArgb(220, 53, 69);
    private static readonly Color C_FundoVermelhoSuave = Color.FromArgb(255, 245, 245);
    private static readonly Color C_FundoVermelhoHover = Color.FromArgb(255, 230, 230);

    // Tipografia
    private static readonly Font F_Titulo = new Font("Segoe UI", 12F, FontStyle.Bold);
    private static readonly Font F_Texto = new Font("Segoe UI", 10F, FontStyle.Regular);
    private static readonly Font F_Secao = new Font("Segoe UI", 10F, FontStyle.Bold);

    public static void Show(Tekla.Macros.Runtime.IMacroRuntime runtime)
    {
        using (var form = new Form())
        {
            // 1. Configuracoes da Janela
            form.Text = "MarnaTeklaOS - Painel de Controle";
            form.StartPosition = FormStartPosition.CenterScreen;
            form.Size = new Size(480, 600);
            form.MinimumSize = new Size(440, 520);
            form.Font = F_Texto;
            form.BackColor = C_FundoForm;
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.MaximizeBox = false;
            form.TopMost = true;

            var mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.ColumnCount = 1;
            mainLayout.RowCount = 3;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 92F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));

            // 2. Cabecalho (Header)
            var headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Fill;
            headerPanel.BackColor = C_Cabecalho;
            headerPanel.Padding = new Padding(16, 12, 16, 12);

            var headerTextLayout = new TableLayoutPanel();
            headerTextLayout.Dock = DockStyle.Fill;
            headerTextLayout.ColumnCount = 1;
            headerTextLayout.RowCount = 2;
            headerTextLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            headerTextLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var lblTitle = new Label();
            lblTitle.Text = "MarnaTeklaOS";
            lblTitle.ForeColor = C_TextoClaro;
            lblTitle.Font = F_Titulo;
            lblTitle.AutoSize = true;
            lblTitle.Dock = DockStyle.Fill;

            var lblSub = new Label();
            lblSub.Text = "Utilitarios para Tekla Structures";
            lblSub.ForeColor = C_TextoCabecalhoSec;
            lblSub.Font = F_Texto;
            lblSub.AutoSize = true;
            lblSub.Dock = DockStyle.Fill;

            headerTextLayout.Controls.Add(lblTitle, 0, 0);
            headerTextLayout.Controls.Add(lblSub, 0, 1);
            headerPanel.Controls.Add(headerTextLayout);

            // 3. Dashboard de Conteudo
            var contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Padding = new Padding(16);
            contentPanel.AutoScroll = true;

            var dashboardLayout = new TableLayoutPanel();
            dashboardLayout.Dock = DockStyle.Top;
            dashboardLayout.AutoSize = true;
            dashboardLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            dashboardLayout.ColumnCount = 1;
            dashboardLayout.RowCount = 0;
            dashboardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            contentPanel.SizeChanged += (s, e) =>
            {
                dashboardLayout.Width = Math.Max(0, contentPanel.ClientSize.Width - contentPanel.Padding.Horizontal);
            };
            dashboardLayout.Width = Math.Max(0, contentPanel.ClientSize.Width - contentPanel.Padding.Horizontal);

            var grpReports = CriarGrupoDashboard("Relatorios e consultas");
            var reportsLayout = CriarLayoutVertical();

            var btnGeral = CriarBotaoDashboard("Gerar relatorio do modelo", false);
            btnGeral.Click += delegate {
                string r = ReportBuilder.BuildReport();
                if(!string.IsNullOrEmpty(r)) ReportWindow.ShowReport(r);
            };

            var btnSel = CriarBotaoDashboard("Ver pecas selecionadas", false);
            btnSel.Click += delegate {
                string r = ReportBuilder.BuildSelectedPartsReport();
                if(!string.IsNullOrEmpty(r)) ReportWindow.ShowReport(r);
            };

            AdicionarLinha(reportsLayout, btnGeral);
            AdicionarLinha(reportsLayout, btnSel);
            grpReports.Controls.Add(reportsLayout);
            AdicionarLinha(dashboardLayout, grpReports);

            var grpSelection = CriarGrupoDashboard("Selecao de pecas");
            var selectionLayout = CriarLayoutVertical();

            var txtSelectionInput = new TextBox();
            txtSelectionInput.Dock = DockStyle.Fill;
            txtSelectionInput.Height = 30;
            txtSelectionInput.Font = F_Texto;
            txtSelectionInput.Margin = new Padding(0, 0, 0, 8);
            txtSelectionInput.BackColor = C_CardFundo;

var btnSelectParts = CriarBotaoDashboard("Selecionar pecas", false);
btnSelectParts.MinimumSize = new Size(200, 36);
            btnSelectParts.Click += delegate {
                AssemblySelectionHelper.SelectAssemblies(txtSelectionInput.Text);
            };

            var selectionTooltip = new ToolTip();
            selectionTooltip.SetToolTip(btnSelectParts, "Digite os nomes dos conjuntos separados por virgula (ex.: PP1,PP2,VR1).");

            var selectionActions = new FlowLayoutPanel();
            selectionActions.FlowDirection = FlowDirection.LeftToRight;
            selectionActions.AutoSize = true;
            selectionActions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            selectionActions.Dock = DockStyle.Top;
            selectionActions.Padding = new Padding(0);
            selectionActions.Margin = new Padding(0, 0, 0, 16);
            selectionActions.WrapContents = false;

            var btnSelectionHelp = new Button();
            btnSelectionHelp.Text = "[?]";
            btnSelectionHelp.Height = 36;
            btnSelectionHelp.Width = 36;
            btnSelectionHelp.FlatStyle = FlatStyle.Flat;
            btnSelectionHelp.FlatAppearance.BorderSize = 1;
            btnSelectionHelp.FlatAppearance.BorderColor = C_Borda;
            btnSelectionHelp.BackColor = C_CardFundo;
            btnSelectionHelp.ForeColor = C_DestaqueAzul;
            btnSelectionHelp.Font = F_Texto;
            btnSelectionHelp.Cursor = Cursors.Hand;
            btnSelectionHelp.Margin = new Padding(6, 0, 0, 0);
            btnSelectionHelp.Click += delegate {
                MessageBox.Show(
                    "Digite os nomes dos conjuntos separados por virgula (ex.: PP1,PP2,VR1).",
                    "Como usar",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            };

            selectionActions.Controls.Add(btnSelectParts);
            selectionActions.Controls.Add(btnSelectionHelp);

            AdicionarLinha(selectionLayout, txtSelectionInput);
            AdicionarLinha(selectionLayout, selectionActions);
            grpSelection.Controls.Add(selectionLayout);
            AdicionarLinha(dashboardLayout, grpSelection);

            var grpActions = CriarGrupoDashboard("Acoes do modelo");
            var actionsLayout = CriarLayoutVertical();

            var btnRepair = CriarBotaoDashboard("Diagnosticar e reparar modelo", true);
            btnRepair.Click += delegate { TeklaCommands.RunModelRepair(runtime); };
            btnRepair.MinimumSize = new Size(200, 36);

            var repairTooltip = new ToolTip();
            repairTooltip.SetToolTip(btnRepair, "Use esta opcao caso o modelo apresente lentidao ou erros de numeracao.");

            var repairActions = new FlowLayoutPanel();
            repairActions.FlowDirection = FlowDirection.LeftToRight;
            repairActions.AutoSize = true;
            repairActions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            repairActions.Dock = DockStyle.Top;
            repairActions.Padding = new Padding(0);
            repairActions.Margin = new Padding(0, 0, 0, 16);
            repairActions.WrapContents = false;

            var btnRepairHelp = new Button();
            btnRepairHelp.Text = "[?]";
            btnRepairHelp.Height = 36;
            btnRepairHelp.Width = 36;
            btnRepairHelp.FlatStyle = FlatStyle.Flat;
            btnRepairHelp.FlatAppearance.BorderSize = 1;
            btnRepairHelp.FlatAppearance.BorderColor = C_Borda;
            btnRepairHelp.BackColor = C_CardFundo;
            btnRepairHelp.ForeColor = C_DestaqueAzul;
            btnRepairHelp.Font = F_Texto;
            btnRepairHelp.Cursor = Cursors.Hand;
            btnRepairHelp.Margin = new Padding(6, 0, 0, 0);
            btnRepairHelp.Click += delegate {
                MessageBox.Show(
                    "Use esta opcao caso o modelo apresente lentidao ou erros de numeracao.",
                    "Como usar",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            };

            repairActions.Controls.Add(btnRepair);
            repairActions.Controls.Add(btnRepairHelp);

            AdicionarLinha(actionsLayout, repairActions);

            var btnCompare = CriarBotaoDashboard("Comparar conjuntos", false);
            btnCompare.Click += delegate { AssemblyComparator.CompareSelectedAssemblies(); };

            var compareTooltip = new ToolTip();
            compareTooltip.SetToolTip(btnCompare, "Selecione exatamente dois conjuntos (ex: PP1 e PP2) e clique para comparar as pecas lado a lado.");

            AdicionarLinha(actionsLayout, btnCompare);
            grpActions.Controls.Add(actionsLayout);
            AdicionarLinha(dashboardLayout, grpActions);

            contentPanel.Controls.Add(dashboardLayout);

            // 4. Rodape
            var footerPanel = new Panel();
            footerPanel.Dock = DockStyle.Fill;
            footerPanel.BackColor = C_CardFundo;
            footerPanel.Padding = new Padding(16, 8, 16, 8);

            var chkTransp = new CheckBox();
            chkTransp.Text = "Modo Transparente";
            chkTransp.AutoSize = true;
            chkTransp.Cursor = Cursors.Hand;
            chkTransp.Dock = DockStyle.Left;
            chkTransp.ForeColor = C_TextoSecundario;
            chkTransp.BackColor = C_CardFundo;
            chkTransp.Font = F_Texto;
            chkTransp.CheckedChanged += delegate { form.Opacity = chkTransp.Checked ? 0.85 : 1.0; };

            var btnClose = new Button();
            btnClose.Text = "Fechar";
            btnClose.AutoSize = false;
            btnClose.Size = new Size(90, 30);
            btnClose.Dock = DockStyle.Right;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 1;
            btnClose.FlatAppearance.BorderColor = C_Borda;
            btnClose.BackColor = C_CardFundo;
            btnClose.Cursor = Cursors.Hand;
            btnClose.ForeColor = C_TextoPrimario;
            btnClose.Font = F_Texto;
            btnClose.DialogResult = DialogResult.OK;

            footerPanel.Controls.Add(chkTransp);
            footerPanel.Controls.Add(btnClose);

            mainLayout.Controls.Add(headerPanel, 0, 0);
            mainLayout.Controls.Add(contentPanel, 0, 1);
            mainLayout.Controls.Add(footerPanel, 0, 2);
            form.Controls.Add(mainLayout);
            form.AcceptButton = btnClose;
            form.CancelButton = btnClose;

            form.ShowDialog();
        }
    }

    // --- Helpers UI ---
    private static GroupBox CriarGrupoDashboard(string titulo)
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

    private static TableLayoutPanel CriarLayoutVertical()
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

    private static void AdicionarLinha(TableLayoutPanel layout, Control control)
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

    private static Label CriarLabelInfo(string texto)
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

    private static Button CriarBotaoDashboard(string texto, bool ehPerigo)
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

}

