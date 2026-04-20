internal static class MenuUi
{
    public static void Show(Tekla.Macros.Runtime.IMacroRuntime runtime)
    {
        using (var form = new Form())
        {
            // 1. Configuracoes da Janela
            form.Text = "MarnaTeklaOS - Painel de Controle";
            form.StartPosition = FormStartPosition.CenterScreen;
            form.Size = new Size(480, 750);
            form.MinimumSize = new Size(440, 600);
            form.Font = DesignSystem.F_Texto;
            form.BackColor = DesignSystem.C_FundoForm;
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
            headerPanel.BackColor = DesignSystem.C_Cabecalho;
            headerPanel.Padding = new Padding(16, 12, 16, 12);

            var headerTextLayout = new TableLayoutPanel();
            headerTextLayout.Dock = DockStyle.Fill;
            headerTextLayout.ColumnCount = 1;
            headerTextLayout.RowCount = 2;
            headerTextLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            headerTextLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var lblTitle = new Label();
            lblTitle.Text = "MarnaTeklaOS";
            lblTitle.ForeColor = DesignSystem.C_TextoClaro;
            lblTitle.Font = DesignSystem.F_Titulo;
            lblTitle.AutoSize = true;
            lblTitle.Dock = DockStyle.Fill;

            var lblSub = new Label();
            lblSub.Text = "Utilitarios para Tekla Structures";
            lblSub.ForeColor = DesignSystem.C_TextoCabecalhoSec;
            lblSub.Font = DesignSystem.F_Texto;
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

            var grpReports = DesignSystem.CriarGrupoDashboard("Relatorios e consultas");
            var reportsLayout = DesignSystem.CriarLayoutVertical();

            var btnGeral = DesignSystem.CriarBotaoDashboard("Gerar relatorio do modelo", false);
            btnGeral.Click += delegate {
                var prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    string r = ReportBuilder.BuildReport();
                    if(!string.IsNullOrEmpty(r)) ReportWindow.ShowReport(r);
                }
                finally { Cursor.Current = prev; }
            };

            var btnSel = DesignSystem.CriarBotaoDashboard("Ver pecas selecionadas", false);
            btnSel.Click += delegate {
                var prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    string r = ReportBuilder.BuildSelectedPartsReport();
                    if(!string.IsNullOrEmpty(r)) ReportWindow.ShowReport(r);
                }
                finally { Cursor.Current = prev; }
            };

            var btnMaterialAll = DesignSystem.CriarBotaoDashboard("Resumo de materiais (modelo inteiro)", false);
            btnMaterialAll.Click += delegate {
                var prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    string r = MaterialSummary.BuildMaterialReport(false);
                    if (!string.IsNullOrEmpty(r)) ReportWindow.ShowReport(r, "Resumo de Materiais (Modelo)");
                }
                finally { Cursor.Current = prev; }
            };

            var btnMaterialSel = DesignSystem.CriarBotaoDashboard("Resumo de materiais (selecao)", false);
            btnMaterialSel.Click += delegate {
                var prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    string r = MaterialSummary.BuildMaterialReport(true);
                    if (!string.IsNullOrEmpty(r)) ReportWindow.ShowReport(r, "Resumo de Materiais (Selecao)");
                }
                finally { Cursor.Current = prev; }
            };

            var btnMaterialHelp = DesignSystem.CriarBotaoHelp("Gera um resumo da quantidade de pecas e pesos agrupados por material e perfil. Ideal para orcamento e compras.");
            var materialActions = DesignSystem.CriarLinhaComHelp(btnMaterialSel, btnMaterialHelp, 8);

            DesignSystem.AdicionarLinha(reportsLayout, btnGeral);
            DesignSystem.AdicionarLinha(reportsLayout, btnSel);
            DesignSystem.AdicionarLinha(reportsLayout, btnMaterialAll);
            DesignSystem.AdicionarLinha(reportsLayout, materialActions);
            grpReports.Controls.Add(reportsLayout);
            DesignSystem.AdicionarLinha(dashboardLayout, grpReports);

            var grpSelection = DesignSystem.CriarGrupoDashboard("Selecao de pecas");
            var selectionLayout = DesignSystem.CriarLayoutVertical();

            var txtSelectionInput = new TextBox();
            txtSelectionInput.Multiline = true;
            txtSelectionInput.ScrollBars = ScrollBars.Vertical;
            txtSelectionInput.Dock = DockStyle.Fill;
            txtSelectionInput.Height = 60;
            txtSelectionInput.Font = DesignSystem.F_Texto;
            txtSelectionInput.Margin = new Padding(0, 0, 0, 8);
            txtSelectionInput.BackColor = DesignSystem.C_CardFundo;

            var btnSelectParts = DesignSystem.CriarBotaoDashboard("Selecionar pecas", false);
            btnSelectParts.Click += delegate {
                AssemblySelectionHelper.SelectAssemblies(txtSelectionInput.Text);
            };

            var btnSelectionHelp = DesignSystem.CriarBotaoHelp("Digite os nomes dos conjuntos separados por virgula (ex.: PP1,PP2,VR1).");
            var selectionActions = DesignSystem.CriarLinhaComHelp(btnSelectParts, btnSelectionHelp, 8);

            DesignSystem.AdicionarLinha(selectionLayout, txtSelectionInput);
            DesignSystem.AdicionarLinha(selectionLayout, selectionActions);
            grpSelection.Controls.Add(selectionLayout);
            DesignSystem.AdicionarLinha(dashboardLayout, grpSelection);

            // --- Grupo: Desenhos ---
            var grpDrawings = DesignSystem.CriarGrupoDashboard("Vinculo Modelo e Desenho");
            var drawingsLayout = DesignSystem.CriarLayoutVertical();

            DesignSystem.AdicionarLinha(drawingsLayout, DesignSystem.CriarLabelInfo("Marca ou nome do desenho (ex: PP1):"));
            var txtDrawingMark = new TextBox();
            txtDrawingMark.Multiline = false;
            txtDrawingMark.Dock = DockStyle.Fill;
            txtDrawingMark.Height = 28;
            txtDrawingMark.Font = DesignSystem.F_Texto;
            txtDrawingMark.Margin = new Padding(0, 0, 0, 8);
            txtDrawingMark.BackColor = DesignSystem.C_CardFundo;
            DesignSystem.AdicionarLinha(drawingsLayout, txtDrawingMark);

            var btnSelectFromDraw = DesignSystem.CriarBotaoDashboard("Selecionar peca base do modelo", false);
            btnSelectFromDraw.Click += delegate { 
                var prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                try {
                    DrawingModelLinker.SelectModelPartFromDrawingMark(txtDrawingMark.Text); 
                } finally { Cursor.Current = prev; }
            };
            var btnSelectFromDrawHelp = DesignSystem.CriarBotaoHelp("Digite a marca do desenho na caixa acima e clique aqui.\n\nO programa vai buscar o desenho no Document Manager e selecionar no modelo 3D exatamente a peça-mãe que foi usada para gerar este desenho.");
            var drawAction1 = DesignSystem.CriarLinhaComHelp(btnSelectFromDraw, btnSelectFromDrawHelp, 8);

            var btnOpenDrawing = DesignSystem.CriarBotaoDashboard("Abrir desenho da peca modelo", false);
            btnOpenDrawing.Click += delegate { 
                var prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                try {
                    DrawingModelLinker.OpenDrawingFromSelectedModelPart();
                } finally { Cursor.Current = prev; }
            };
            var btnOpenDrawingHelp = DesignSystem.CriarBotaoHelp("Selecione um objeto 3D no modelo e clique aqui.\n\nO sistema vai buscar e abrir automaticamente o desenho do Document Manager que pertence àquela posição.");
            var drawAction2 = DesignSystem.CriarLinhaComHelp(btnOpenDrawing, btnOpenDrawingHelp, 8);

            DesignSystem.AdicionarLinha(drawingsLayout, drawAction1);
            DesignSystem.AdicionarLinha(drawingsLayout, drawAction2);

            grpDrawings.Controls.Add(drawingsLayout);
            DesignSystem.AdicionarLinha(dashboardLayout, grpDrawings);

            var grpActions = DesignSystem.CriarGrupoDashboard("Acoes do modelo");
            var actionsLayout = DesignSystem.CriarLayoutVertical();

            var btnRepair = DesignSystem.CriarBotaoDashboard("Diagnosticar e reparar modelo", true);
            btnRepair.Click += delegate { TeklaCommands.RunModelRepair(runtime); };

            var btnRepairHelp = DesignSystem.CriarBotaoHelp("Use esta opcao caso o modelo apresente lentidao ou erros de numeracao.");
            var repairActions = DesignSystem.CriarLinhaComHelp(btnRepair, btnRepairHelp, 8);

            DesignSystem.AdicionarLinha(actionsLayout, repairActions);

            var btnCompare = DesignSystem.CriarBotaoDashboard("Comparar conjuntos", false);
            btnCompare.Click += delegate { 
                var prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    AssemblyComparator.CompareSelectedAssemblies(); 
                }
                finally { Cursor.Current = prev; }
            };

            var btnCompareHelp = DesignSystem.CriarBotaoHelp("Selecione exatamente dois conjuntos (ex: PP1 e PP2) e clique para comparar as pecas lado a lado.");
            var compareActions = DesignSystem.CriarLinhaComHelp(btnCompare, btnCompareHelp, 8);

            DesignSystem.AdicionarLinha(actionsLayout, compareActions);
            grpActions.Controls.Add(actionsLayout);
            DesignSystem.AdicionarLinha(dashboardLayout, grpActions);

            // --- Grupo: Producao e Logistica ---
            var grpProduction = DesignSystem.CriarGrupoDashboard("Producao e logistica");
            var productionLayout = DesignSystem.CriarLayoutVertical();

            var btnWeightAll = DesignSystem.CriarBotaoDashboard("Resumo de peso (modelo inteiro)", false);
            btnWeightAll.Click += delegate {
                var prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    string r = WeightSummary.BuildWeightByPhaseReport();
                    if (!string.IsNullOrEmpty(r)) ReportWindow.ShowReport(r, "Resumo de Peso por Fase");
                }
                finally { Cursor.Current = prev; }
            };

            var btnWeightSel = DesignSystem.CriarBotaoDashboard("Resumo de peso (selecao)", false);
            btnWeightSel.Click += delegate {
                var prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    string r = WeightSummary.BuildWeightByPhaseReportSelected();
                    if (!string.IsNullOrEmpty(r)) ReportWindow.ShowReport(r, "Resumo de Peso (Selecao)");
                }
                finally { Cursor.Current = prev; }
            };

            var btnWeightHelp = DesignSystem.CriarBotaoHelp("Gera um resumo de peso liquido e bruto agrupado por fase. Use 'modelo inteiro' para ver tudo ou 'selecao' para ver apenas os conjuntos selecionados.");
            var weightActions = DesignSystem.CriarLinhaComHelp(btnWeightSel, btnWeightHelp, 8);

            DesignSystem.AdicionarLinha(productionLayout, btnWeightAll);
            DesignSystem.AdicionarLinha(productionLayout, weightActions);
            grpProduction.Controls.Add(productionLayout);
            DesignSystem.AdicionarLinha(dashboardLayout, grpProduction);

            // --- Grupo: Controle de Qualidade ---
            var grpQuality = DesignSystem.CriarGrupoDashboard("Controle de qualidade");
            var qualityLayout = DesignSystem.CriarLayoutVertical();

            var btnNumberCheck = DesignSystem.CriarBotaoDashboard("Verificar numeracao dos prefixos selecionados", false);
            btnNumberCheck.Click += delegate {
                var prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    string r = NumberingChecker.CheckNumberingFromSelection();
                    if (!string.IsNullOrEmpty(r)) ReportWindow.ShowReport(r, "Verificacao de Numeracao");
                }
                finally { Cursor.Current = prev; }
            };

            var btnNumberCheckHelp = DesignSystem.CriarBotaoHelp("Selecione uma ou mais pecas no modelo.\nO sistema identifica os prefixos (ex: PP, VR) e verifica todos os conjuntos com esses prefixos em busca de:\n- Posicoes vazias (modelo inteiro)\n- Gaps na sequencia (ex: PP1, PP2, PP4 sem PP3)");
            var qualityActions = DesignSystem.CriarLinhaComHelp(btnNumberCheck, btnNumberCheckHelp, 8);

            DesignSystem.AdicionarLinha(qualityLayout, qualityActions);
            grpQuality.Controls.Add(qualityLayout);
            DesignSystem.AdicionarLinha(dashboardLayout, grpQuality);

            contentPanel.Controls.Add(dashboardLayout);

            // 4. Rodape
            var footerPanel = new Panel();
            footerPanel.Dock = DockStyle.Fill;
            footerPanel.BackColor = DesignSystem.C_CardFundo;
            footerPanel.Padding = new Padding(16, 8, 16, 8);

            var chkTransp = new CheckBox();
            chkTransp.Text = "Modo Transparente";
            chkTransp.AutoSize = true;
            chkTransp.Cursor = Cursors.Hand;
            chkTransp.Dock = DockStyle.Left;
            chkTransp.ForeColor = DesignSystem.C_TextoSecundario;
            chkTransp.BackColor = DesignSystem.C_CardFundo;
            chkTransp.Font = DesignSystem.F_Texto;
            chkTransp.CheckedChanged += delegate { form.Opacity = chkTransp.Checked ? 0.85 : 1.0; };

            var btnClose = new Button();
            btnClose.Text = "Fechar";
            btnClose.AutoSize = false;
            btnClose.Size = new Size(90, 30);
            btnClose.Dock = DockStyle.Right;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 1;
            btnClose.FlatAppearance.BorderColor = DesignSystem.C_Borda;
            btnClose.BackColor = DesignSystem.C_CardFundo;
            btnClose.Cursor = Cursors.Hand;
            btnClose.ForeColor = DesignSystem.C_TextoPrimario;
            btnClose.Font = DesignSystem.F_Texto;
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
}


