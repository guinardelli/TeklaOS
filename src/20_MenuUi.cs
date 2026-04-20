internal static class MenuUi
{
    public static void Show(Tekla.Macros.Runtime.IMacroRuntime runtime)
    {
        using (var form = new Form())
        using (var toolTip = new ToolTip())
        {
            // 1. Configuracoes da Janela
            form.Text = "MarnaTeklaOS - Painel de Controle";
            form.StartPosition = FormStartPosition.CenterScreen;
            form.Size = new Size(520, 760);
            form.MinimumSize = new Size(460, 620);
            form.Font = DesignSystem.F_Texto;
            form.BackColor = DesignSystem.C_FundoForm;
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.MaximizeBox = false;
            form.TopMost = true;

            toolTip.InitialDelay = 250;
            toolTip.ReshowDelay = 120;
            toolTip.AutoPopDelay = 12000;
            toolTip.ShowAlways = true;

            var mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.ColumnCount = 1;
            mainLayout.RowCount = 3;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 92F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));

            // 2. Cabecalho
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

            // 3. Conteudo principal
            var contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Padding = new Padding(12);
            contentPanel.BackColor = DesignSystem.C_FundoForm;

            var contentLayout = new TableLayoutPanel();
            contentLayout.Dock = DockStyle.Fill;
            contentLayout.ColumnCount = 1;
            contentLayout.RowCount = 3;
            contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            contentLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            contentLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            contentLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            contentPanel.Controls.Add(contentLayout);

            var filterRowsByGroup = new Dictionary<GroupBox, List<Control>>();
            var filterKeywordsByRow = new Dictionary<Control, string>();
            var groupToTab = new Dictionary<GroupBox, TabPage>();

            Action<GroupBox, Control, string> registerFilterRow = delegate(GroupBox group, Control rowControl, string searchKeywords)
            {
                if (group == null || rowControl == null)
                {
                    return;
                }

                List<Control> controls;
                if (!filterRowsByGroup.TryGetValue(group, out controls))
                {
                    controls = new List<Control>();
                    filterRowsByGroup[group] = controls;
                }

                controls.Add(rowControl);

                string raw = string.Format("{0} {1} {2}", searchKeywords, rowControl.Text, group.Text);
                filterKeywordsByRow[rowControl] = NormalizeSearchForFilter(raw);
            };

            // Busca
            var grpSearch = DesignSystem.CriarGrupoDashboard("Busca de acoes");
            StyleGroup(grpSearch, Color.FromArgb(248, 249, 252), DesignSystem.C_DestaqueAzul);
            var searchLayout = DesignSystem.CriarLayoutVertical();
            var lblSearchInfo = DesignSystem.CriarLabelInfo("Digite para filtrar botoes e funcoes no painel.");
            var txtSearch = new TextBox();
            txtSearch.Multiline = false;
            txtSearch.Dock = DockStyle.Fill;
            txtSearch.Height = 28;
            txtSearch.Font = DesignSystem.F_Texto;
            txtSearch.Margin = new Padding(0, 0, 0, 0);
            txtSearch.BackColor = DesignSystem.C_CardFundo;
            txtSearch.Text = string.Empty;
            toolTip.SetToolTip(txtSearch, "Exemplos: material, numeracao, selecao, reparo, peso.");

            DesignSystem.AdicionarLinha(searchLayout, lblSearchInfo);
            DesignSystem.AdicionarLinha(searchLayout, txtSearch);
            grpSearch.Controls.Add(searchLayout);
            contentLayout.Controls.Add(grpSearch, 0, 0);

            var lblNoResults = DesignSystem.CriarLabelInfo("Nenhuma acao encontrada para o filtro informado.");
            lblNoResults.ForeColor = DesignSystem.C_DestaqueVermelho;
            lblNoResults.Visible = false;
            lblNoResults.Margin = new Padding(4, 4, 4, 8);
            contentLayout.Controls.Add(lblNoResults, 0, 1);

            // Guias
            var tabs = new TabControl();
            tabs.Dock = DockStyle.Fill;
            tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabs.SizeMode = TabSizeMode.Fixed;
            tabs.ItemSize = new Size(120, 28);
            tabs.Font = DesignSystem.F_Texto;
            tabs.Padding = new System.Drawing.Point(14, 4);

            var tabHeaderColors = new Color[]
            {
                Color.FromArgb(226, 236, 252),
                Color.FromArgb(228, 245, 232),
                Color.FromArgb(241, 233, 251),
                Color.FromArgb(253, 236, 226),
                Color.FromArgb(224, 246, 246),
                Color.FromArgb(252, 232, 232)
            };

            tabs.DrawItem += delegate(object sender, DrawItemEventArgs e)
            {
                if (e.Index < 0 || e.Index >= tabs.TabPages.Count)
                {
                    return;
                }

                bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                Color baseColor = e.Index < tabHeaderColors.Length ? tabHeaderColors[e.Index] : Color.White;
                Color fillColor = selected ? ShiftColor(baseColor, -16) : baseColor;

                using (var brush = new SolidBrush(fillColor))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }

                Rectangle border = e.Bounds;
                border.Width -= 1;
                border.Height -= 1;
                using (var pen = new Pen(DesignSystem.C_Borda))
                {
                    e.Graphics.DrawRectangle(pen, border);
                }

                TextRenderer.DrawText(
                    e.Graphics,
                    tabs.TabPages[e.Index].Text,
                    DesignSystem.F_Texto,
                    e.Bounds,
                    DesignSystem.C_TextoPrimario,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            };

            contentLayout.Controls.Add(tabs, 0, 2);

            // Criacao das abas
            TableLayoutPanel layoutConsultas;
            TabPage tabConsultas = CreateTabPageWithLayout(tabs, "Consultas", Color.FromArgb(245, 248, 255), out layoutConsultas);

            TableLayoutPanel layoutSelecao;
            TabPage tabSelecao = CreateTabPageWithLayout(tabs, "Selecao", Color.FromArgb(247, 252, 247), out layoutSelecao);

            TableLayoutPanel layoutDesenhos;
            TabPage tabDesenhos = CreateTabPageWithLayout(tabs, "Desenhos", Color.FromArgb(250, 246, 255), out layoutDesenhos);

            TableLayoutPanel layoutModelo;
            TabPage tabModelo = CreateTabPageWithLayout(tabs, "Modelo", Color.FromArgb(255, 248, 243), out layoutModelo);

            TableLayoutPanel layoutProducao;
            TabPage tabProducao = CreateTabPageWithLayout(tabs, "Producao", Color.FromArgb(244, 252, 252), out layoutProducao);

            TableLayoutPanel layoutQualidade;
            TabPage tabQualidade = CreateTabPageWithLayout(tabs, "Qualidade", Color.FromArgb(255, 246, 246), out layoutQualidade);

            // Grupo: Relatorios e consultas
            var grpReports = DesignSystem.CriarGrupoDashboard("Relatorios e consultas");
            StyleGroup(grpReports, Color.FromArgb(250, 252, 255), DesignSystem.C_DestaqueAzul);
            groupToTab[grpReports] = tabConsultas;
            var reportsLayout = DesignSystem.CriarLayoutVertical();

            var btnGeral = DesignSystem.CriarBotaoDashboard("Gerar relatorio do modelo", false);
            toolTip.SetToolTip(btnGeral, "Exibe informacoes do modelo e do projeto.");

            var btnSel = DesignSystem.CriarBotaoDashboard("Ver pecas selecionadas", false);
            toolTip.SetToolTip(btnSel, "Exibe um relatorio das pecas atualmente selecionadas.");

            var btnMaterialAll = DesignSystem.CriarBotaoDashboard("Resumo de materiais (modelo inteiro)", false);
            toolTip.SetToolTip(btnMaterialAll, "Agrupa pecas por material e perfil para o modelo inteiro.");

            var btnMaterialSel = DesignSystem.CriarBotaoDashboard("Resumo de materiais (selecao)", false);
            toolTip.SetToolTip(btnMaterialSel, "Agrupa pecas por material e perfil apenas da selecao atual.");

            DesignSystem.AdicionarLinha(reportsLayout, btnGeral);
            registerFilterRow(grpReports, btnGeral, "relatorio modelo projeto informacoes");

            DesignSystem.AdicionarLinha(reportsLayout, btnSel);
            registerFilterRow(grpReports, btnSel, "pecas selecionadas relatorio");

            DesignSystem.AdicionarLinha(reportsLayout, btnMaterialAll);
            registerFilterRow(grpReports, btnMaterialAll, "material resumo modelo inteiro compras orcamento");

            DesignSystem.AdicionarLinha(reportsLayout, btnMaterialSel);
            registerFilterRow(grpReports, btnMaterialSel, "material resumo selecao compras orcamento");

            grpReports.Controls.Add(reportsLayout);
            DesignSystem.AdicionarLinha(layoutConsultas, grpReports);

            // Grupo: Selecao de pecas
            var grpSelection = DesignSystem.CriarGrupoDashboard("Selecao de pecas");
            StyleGroup(grpSelection, Color.FromArgb(248, 255, 248), Color.FromArgb(38, 125, 68));
            groupToTab[grpSelection] = tabSelecao;
            var selectionLayout = DesignSystem.CriarLayoutVertical();

            var txtSelectionInput = new TextBox();
            txtSelectionInput.Multiline = true;
            txtSelectionInput.ScrollBars = ScrollBars.Vertical;
            txtSelectionInput.Dock = DockStyle.Fill;
            txtSelectionInput.Height = 60;
            txtSelectionInput.Font = DesignSystem.F_Texto;
            txtSelectionInput.Margin = new Padding(0, 0, 0, 8);
            txtSelectionInput.BackColor = DesignSystem.C_CardFundo;
            toolTip.SetToolTip(txtSelectionInput, "Digite os nomes dos conjuntos separados por virgula. Ex.: PP1,PP2,VR1");

            var btnSelectParts = DesignSystem.CriarBotaoDashboard("Selecionar pecas", false);
            toolTip.SetToolTip(btnSelectParts, "Seleciona automaticamente os conjuntos informados no campo acima.");

            DesignSystem.AdicionarLinha(selectionLayout, txtSelectionInput);
            registerFilterRow(grpSelection, txtSelectionInput, "selecao selecionar pecas conjuntos");

            DesignSystem.AdicionarLinha(selectionLayout, btnSelectParts);
            registerFilterRow(grpSelection, btnSelectParts, "selecao selecionar pecas conjuntos");

            grpSelection.Controls.Add(selectionLayout);
            DesignSystem.AdicionarLinha(layoutSelecao, grpSelection);

            // Grupo: Vinculo Modelo e Desenho
            var grpDrawings = DesignSystem.CriarGrupoDashboard("Vinculo Modelo e Desenho");
            StyleGroup(grpDrawings, Color.FromArgb(252, 249, 255), Color.FromArgb(94, 67, 165));
            groupToTab[grpDrawings] = tabDesenhos;
            var drawingsLayout = DesignSystem.CriarLayoutVertical();

            var lblDrawingInfo = DesignSystem.CriarLabelInfo("Marca ou nome do desenho (ex: PP1):");
            DesignSystem.AdicionarLinha(drawingsLayout, lblDrawingInfo);
            registerFilterRow(grpDrawings, lblDrawingInfo, "desenho marca nome vinculo");

            var txtDrawingMark = new TextBox();
            txtDrawingMark.Multiline = false;
            txtDrawingMark.Dock = DockStyle.Fill;
            txtDrawingMark.Height = 28;
            txtDrawingMark.Font = DesignSystem.F_Texto;
            txtDrawingMark.Margin = new Padding(0, 0, 0, 8);
            txtDrawingMark.BackColor = DesignSystem.C_CardFundo;
            toolTip.SetToolTip(txtDrawingMark, "Digite a marca do desenho conforme Document Manager.");
            DesignSystem.AdicionarLinha(drawingsLayout, txtDrawingMark);
            registerFilterRow(grpDrawings, txtDrawingMark, "desenho marca nome vinculo");

            var btnSelectFromDraw = DesignSystem.CriarBotaoDashboard("Selecionar peca base do modelo", false);
            toolTip.SetToolTip(btnSelectFromDraw, "Busca o desenho no Document Manager e seleciona a peca base no modelo 3D.");
            DesignSystem.AdicionarLinha(drawingsLayout, btnSelectFromDraw);
            registerFilterRow(grpDrawings, btnSelectFromDraw, "desenho vinculo selecionar peca base document manager");

            grpDrawings.Controls.Add(drawingsLayout);
            DesignSystem.AdicionarLinha(layoutDesenhos, grpDrawings);

            // Grupo: Acoes do modelo
            var grpActions = DesignSystem.CriarGrupoDashboard("Acoes do modelo");
            StyleGroup(grpActions, Color.FromArgb(255, 249, 245), Color.FromArgb(184, 86, 22));
            groupToTab[grpActions] = tabModelo;
            var actionsLayout = DesignSystem.CriarLayoutVertical();

            var btnRepair = DesignSystem.CriarBotaoDashboard("Diagnosticar e reparar modelo", true);
            toolTip.SetToolTip(btnRepair, "Aciona o comando nativo de reparo de modelo e banco de dados no Tekla.");
            DesignSystem.AdicionarLinha(actionsLayout, btnRepair);
            registerFilterRow(grpActions, btnRepair, "diagnosticar reparar modelo banco dados");

            var btnCompare = DesignSystem.CriarBotaoDashboard("Comparar conjuntos", false);
            toolTip.SetToolTip(btnCompare, "Compare dois conjuntos selecionados: numeracao, pecas e propriedades.");
            DesignSystem.AdicionarLinha(actionsLayout, btnCompare);
            registerFilterRow(grpActions, btnCompare, "comparar conjuntos propriedades");

            grpActions.Controls.Add(actionsLayout);
            DesignSystem.AdicionarLinha(layoutModelo, grpActions);

            // Grupo: Producao e logistica
            var grpProduction = DesignSystem.CriarGrupoDashboard("Producao e logistica");
            StyleGroup(grpProduction, Color.FromArgb(245, 253, 253), Color.FromArgb(20, 116, 116));
            groupToTab[grpProduction] = tabProducao;
            var productionLayout = DesignSystem.CriarLayoutVertical();

            var btnWeightAll = DesignSystem.CriarBotaoDashboard("Resumo de peso (modelo inteiro)", false);
            toolTip.SetToolTip(btnWeightAll, "Resumo de peso liquido e bruto por fase para o modelo inteiro.");
            DesignSystem.AdicionarLinha(productionLayout, btnWeightAll);
            registerFilterRow(grpProduction, btnWeightAll, "peso resumo fase modelo inteiro producao logistica");

            var btnWeightSel = DesignSystem.CriarBotaoDashboard("Resumo de peso (selecao)", false);
            toolTip.SetToolTip(btnWeightSel, "Resumo de peso liquido e bruto por fase para a selecao atual.");
            DesignSystem.AdicionarLinha(productionLayout, btnWeightSel);
            registerFilterRow(grpProduction, btnWeightSel, "peso resumo fase selecao producao logistica");

            grpProduction.Controls.Add(productionLayout);
            DesignSystem.AdicionarLinha(layoutProducao, grpProduction);

            // Grupo: Controle de qualidade
            var grpQuality = DesignSystem.CriarGrupoDashboard("Controle de qualidade");
            StyleGroup(grpQuality, Color.FromArgb(255, 248, 248), DesignSystem.C_DestaqueVermelho);
            groupToTab[grpQuality] = tabQualidade;
            var qualityLayout = DesignSystem.CriarLayoutVertical();

            var btnNumberCheck = DesignSystem.CriarBotaoDashboard("Verificar numeracao dos prefixos selecionados", false);
            toolTip.SetToolTip(btnNumberCheck, "Verifica gaps de numeracao e conjuntos sem posicao para os prefixos da selecao.");
            DesignSystem.AdicionarLinha(qualityLayout, btnNumberCheck);
            registerFilterRow(grpQuality, btnNumberCheck, "verificar numeracao prefixos qualidade gaps posicao");

            grpQuality.Controls.Add(qualityLayout);
            DesignSystem.AdicionarLinha(layoutQualidade, grpQuality);

            // Rodape
            var footerPanel = new Panel();
            footerPanel.Dock = DockStyle.Fill;
            footerPanel.BackColor = DesignSystem.C_CardFundo;
            footerPanel.Padding = new Padding(16, 8, 16, 8);

            var footerLayout = new TableLayoutPanel();
            footerLayout.Dock = DockStyle.Fill;
            footerLayout.ColumnCount = 3;
            footerLayout.RowCount = 1;
            footerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            footerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            footerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            footerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var chkTransp = new CheckBox();
            chkTransp.Text = "Modo Transparente";
            chkTransp.AutoSize = true;
            chkTransp.Cursor = Cursors.Hand;
            chkTransp.ForeColor = DesignSystem.C_TextoSecundario;
            chkTransp.BackColor = DesignSystem.C_CardFundo;
            chkTransp.Font = DesignSystem.F_Texto;
            chkTransp.Margin = new Padding(0, 6, 10, 0);
            chkTransp.CheckedChanged += delegate { form.Opacity = chkTransp.Checked ? 0.85 : 1.0; };

            var lblStatus = new Label();
            lblStatus.Text = "Pronto.";
            lblStatus.AutoEllipsis = true;
            lblStatus.Dock = DockStyle.Fill;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            lblStatus.ForeColor = DesignSystem.C_TextoSecundario;
            lblStatus.Font = DesignSystem.F_Texto;
            lblStatus.Margin = new Padding(0, 6, 10, 0);

            var btnClose = new Button();
            btnClose.Text = "Fechar";
            btnClose.AutoSize = false;
            btnClose.Size = new Size(90, 30);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 1;
            btnClose.FlatAppearance.BorderColor = DesignSystem.C_Borda;
            btnClose.BackColor = DesignSystem.C_CardFundo;
            btnClose.Cursor = Cursors.Hand;
            btnClose.ForeColor = DesignSystem.C_TextoPrimario;
            btnClose.Font = DesignSystem.F_Texto;
            btnClose.DialogResult = DialogResult.OK;
            btnClose.Margin = new Padding(0);

            footerLayout.Controls.Add(chkTransp, 0, 0);
            footerLayout.Controls.Add(lblStatus, 1, 0);
            footerLayout.Controls.Add(btnClose, 2, 0);
            footerPanel.Controls.Add(footerLayout);

            Action<string> setInfoStatus = delegate(string text)
            {
                lblStatus.Text = text;
                lblStatus.ForeColor = DesignSystem.C_DestaqueAzul;
            };

            Action<string> setSuccessStatus = delegate(string text)
            {
                lblStatus.Text = text;
                lblStatus.ForeColor = Color.ForestGreen;
            };

            Action<string> setErrorStatus = delegate(string text)
            {
                lblStatus.Text = text;
                lblStatus.ForeColor = DesignSystem.C_DestaqueVermelho;
            };

            Action applySearchFilter = delegate
            {
                string query = NormalizeSearchForFilter(txtSearch.Text);
                int visibleCount = 0;
                var visibleTabs = new HashSet<TabPage>();

                foreach (KeyValuePair<GroupBox, List<Control>> groupEntry in filterRowsByGroup)
                {
                    GroupBox group = groupEntry.Key;
                    List<Control> rows = groupEntry.Value;
                    bool groupVisible = false;

                    for (int i = 0; i < rows.Count; i++)
                    {
                        Control row = rows[i];
                        string keywords;
                        if (!filterKeywordsByRow.TryGetValue(row, out keywords))
                        {
                            keywords = string.Empty;
                        }

                        bool rowVisible = MatchesFilterQuery(keywords, query);
                        row.Visible = rowVisible;
                        if (rowVisible)
                        {
                            groupVisible = true;
                            visibleCount++;
                        }
                    }

                    group.Visible = groupVisible;

                    if (groupVisible)
                    {
                        TabPage ownerTab;
                        if (groupToTab.TryGetValue(group, out ownerTab))
                        {
                            visibleTabs.Add(ownerTab);
                        }
                    }
                }

                lblNoResults.Visible = !string.IsNullOrEmpty(query) && visibleCount == 0;

                if (!string.IsNullOrEmpty(query) && visibleTabs.Count > 0)
                {
                    if (tabs.SelectedTab == null || !visibleTabs.Contains(tabs.SelectedTab))
                    {
                        for (int i = 0; i < tabs.TabPages.Count; i++)
                        {
                            TabPage page = tabs.TabPages[i];
                            if (visibleTabs.Contains(page))
                            {
                                tabs.SelectedTab = page;
                                break;
                            }
                        }
                    }
                }
            };

            txtSearch.TextChanged += delegate { applySearchFilter(); };

            // Handlers com feedback de status
            btnGeral.Click += delegate {
                var prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                setInfoStatus("Gerando relatorio do modelo...");
                try
                {
                    string r = ReportBuilder.BuildReport();
                    if (!string.IsNullOrEmpty(r))
                    {
                        ReportWindow.ShowReport(r);
                        setSuccessStatus("Relatorio do modelo gerado.");
                    }
                    else
                    {
                        setSuccessStatus("Operacao concluida.");
                    }
                }
                catch (Exception ex)
                {
                    setErrorStatus("Falha ao gerar relatorio.");
                    MessageBox.Show("Erro ao gerar relatorio do modelo: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally { Cursor.Current = prev; }
            };

            btnSel.Click += delegate {
                var prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                setInfoStatus("Lendo pecas selecionadas...");
                try
                {
                    string r = ReportBuilder.BuildSelectedPartsReport();
                    if (!string.IsNullOrEmpty(r))
                    {
                        ReportWindow.ShowReport(r);
                        setSuccessStatus("Relatorio de selecao gerado.");
                    }
                    else
                    {
                        setSuccessStatus("Operacao concluida.");
                    }
                }
                catch (Exception ex)
                {
                    setErrorStatus("Falha ao ler selecao.");
                    MessageBox.Show("Erro ao ler pecas selecionadas: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally { Cursor.Current = prev; }
            };

            btnMaterialAll.Click += delegate {
                var prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                setInfoStatus("Gerando resumo de materiais do modelo...");
                try
                {
                    string r = MaterialSummary.BuildMaterialReport(false);
                    if (!string.IsNullOrEmpty(r))
                    {
                        ReportWindow.ShowReport(r, "Resumo de Materiais (Modelo)");
                        setSuccessStatus("Resumo de materiais do modelo gerado.");
                    }
                    else
                    {
                        setSuccessStatus("Operacao concluida.");
                    }
                }
                catch (Exception ex)
                {
                    setErrorStatus("Falha no resumo de materiais.");
                    MessageBox.Show("Erro ao gerar resumo de materiais: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally { Cursor.Current = prev; }
            };

            btnMaterialSel.Click += delegate {
                var prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                setInfoStatus("Gerando resumo de materiais da selecao...");
                try
                {
                    string r = MaterialSummary.BuildMaterialReport(true);
                    if (!string.IsNullOrEmpty(r))
                    {
                        ReportWindow.ShowReport(r, "Resumo de Materiais (Selecao)");
                        setSuccessStatus("Resumo de materiais da selecao gerado.");
                    }
                    else
                    {
                        setSuccessStatus("Operacao concluida.");
                    }
                }
                catch (Exception ex)
                {
                    setErrorStatus("Falha no resumo de materiais.");
                    MessageBox.Show("Erro ao gerar resumo de materiais: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally { Cursor.Current = prev; }
            };

            btnSelectParts.Click += delegate {
                var prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                setInfoStatus("Selecionando pecas informadas...");
                try
                {
                    AssemblySelectionHelper.SelectAssemblies(txtSelectionInput.Text);
                    setSuccessStatus("Selecao atualizada.");
                }
                catch (Exception ex)
                {
                    setErrorStatus("Falha ao selecionar pecas.");
                    MessageBox.Show("Erro ao selecionar pecas: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally { Cursor.Current = prev; }
            };

            btnSelectFromDraw.Click += delegate {
                var prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                setInfoStatus("Buscando peca base a partir do desenho...");
                try
                {
                    DrawingModelLinker.SelectModelPartFromDrawingMark(txtDrawingMark.Text);
                    setSuccessStatus("Vinculo desenho-modelo executado.");
                }
                catch (Exception ex)
                {
                    setErrorStatus("Falha no vinculo desenho-modelo.");
                    MessageBox.Show("Erro no vinculo desenho-modelo: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally { Cursor.Current = prev; }
            };

            btnRepair.Click += delegate {
                var prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                setInfoStatus("Enviando comando de reparo ao Tekla...");
                try
                {
                    TeklaCommands.RunModelRepair(runtime);
                    setSuccessStatus("Comando de reparo enviado.");
                }
                catch (Exception ex)
                {
                    setErrorStatus("Falha ao acionar reparo.");
                    MessageBox.Show("Erro ao acionar reparo: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally { Cursor.Current = prev; }
            };

            btnCompare.Click += delegate {
                var prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                setInfoStatus("Comparando conjuntos selecionados...");
                try
                {
                    AssemblyComparator.CompareSelectedAssemblies();
                    setSuccessStatus("Comparacao de conjuntos concluida.");
                }
                catch (Exception ex)
                {
                    setErrorStatus("Falha na comparacao de conjuntos.");
                    MessageBox.Show("Erro na comparacao de conjuntos: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally { Cursor.Current = prev; }
            };

            btnWeightAll.Click += delegate {
                var prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                setInfoStatus("Gerando resumo de peso do modelo...");
                try
                {
                    string r = WeightSummary.BuildWeightByPhaseReport();
                    if (!string.IsNullOrEmpty(r))
                    {
                        ReportWindow.ShowReport(r, "Resumo de Peso por Fase");
                        setSuccessStatus("Resumo de peso do modelo gerado.");
                    }
                    else
                    {
                        setSuccessStatus("Operacao concluida.");
                    }
                }
                catch (Exception ex)
                {
                    setErrorStatus("Falha no resumo de peso.");
                    MessageBox.Show("Erro ao gerar resumo de peso: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally { Cursor.Current = prev; }
            };

            btnWeightSel.Click += delegate {
                var prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                setInfoStatus("Gerando resumo de peso da selecao...");
                try
                {
                    string r = WeightSummary.BuildWeightByPhaseReportSelected();
                    if (!string.IsNullOrEmpty(r))
                    {
                        ReportWindow.ShowReport(r, "Resumo de Peso (Selecao)");
                        setSuccessStatus("Resumo de peso da selecao gerado.");
                    }
                    else
                    {
                        setSuccessStatus("Operacao concluida.");
                    }
                }
                catch (Exception ex)
                {
                    setErrorStatus("Falha no resumo de peso.");
                    MessageBox.Show("Erro ao gerar resumo de peso: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally { Cursor.Current = prev; }
            };

            btnNumberCheck.Click += delegate {
                var prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                setInfoStatus("Verificando numeracao dos prefixos...");
                try
                {
                    string r = NumberingChecker.CheckNumberingFromSelection();
                    if (!string.IsNullOrEmpty(r))
                    {
                        ReportWindow.ShowReport(r, "Verificacao de Numeracao");
                        setSuccessStatus("Verificacao de numeracao concluida.");
                    }
                    else
                    {
                        setSuccessStatus("Operacao concluida.");
                    }
                }
                catch (Exception ex)
                {
                    setErrorStatus("Falha na verificacao de numeracao.");
                    MessageBox.Show("Erro na verificacao de numeracao: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally { Cursor.Current = prev; }
            };

            mainLayout.Controls.Add(headerPanel, 0, 0);
            mainLayout.Controls.Add(contentPanel, 0, 1);
            mainLayout.Controls.Add(footerPanel, 0, 2);
            form.Controls.Add(mainLayout);
            form.AcceptButton = btnClose;
            form.CancelButton = btnClose;

            applySearchFilter();
            form.ShowDialog();
        }
    }

    private static TabPage CreateTabPageWithLayout(TabControl tabs, string title, Color backColor, out TableLayoutPanel layout)
    {
        var tab = new TabPage(title);
        tab.Padding = new Padding(0);
        tab.BackColor = backColor;

        var scrollPanel = new Panel();
        scrollPanel.Dock = DockStyle.Fill;
        scrollPanel.AutoScroll = true;
        scrollPanel.Padding = new Padding(12);
        scrollPanel.BackColor = backColor;

        var contentLayout = DesignSystem.CriarLayoutVertical();
        contentLayout.Dock = DockStyle.Top;

        scrollPanel.SizeChanged += delegate
        {
            contentLayout.Width = Math.Max(0, scrollPanel.ClientSize.Width - scrollPanel.Padding.Horizontal);
        };
        contentLayout.Width = Math.Max(0, scrollPanel.ClientSize.Width - scrollPanel.Padding.Horizontal);

        scrollPanel.Controls.Add(contentLayout);
        tab.Controls.Add(scrollPanel);
        tabs.TabPages.Add(tab);
        layout = contentLayout;

        return tab;
    }

    private static void StyleGroup(GroupBox group, Color backColor, Color titleColor)
    {
        if (group == null)
        {
            return;
        }

        group.BackColor = backColor;
        group.ForeColor = titleColor;
        group.Padding = new Padding(12, 10, 12, 12);
        group.Margin = new Padding(0, 0, 0, 12);
    }

    private static Color ShiftColor(Color color, int delta)
    {
        int r = Math.Max(0, Math.Min(255, color.R + delta));
        int g = Math.Max(0, Math.Min(255, color.G + delta));
        int b = Math.Max(0, Math.Min(255, color.B + delta));
        return Color.FromArgb(r, g, b);
    }

    private static bool MatchesFilterQuery(string keywords, string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            return true;
        }

        if (string.IsNullOrEmpty(keywords))
        {
            return false;
        }

        string[] terms = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < terms.Length; i++)
        {
            if (!keywords.Contains(terms[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static string NormalizeSearchForFilter(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var sb = new StringBuilder(text.Length);
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(char.ToUpperInvariant(c));
            }
            else if (char.IsWhiteSpace(c))
            {
                sb.Append(' ');
            }
        }

        string normalized = sb.ToString().Trim();
        while (normalized.Contains("  "))
        {
            normalized = normalized.Replace("  ", " ");
        }

        return normalized;
    }
}
