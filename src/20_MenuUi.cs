internal static class MenuUi
{
    public static void Show(Tekla.Macros.Runtime.IMacroRuntime runtime)
    {
        using (var form = new Form())
        using (var toolTip = new ToolTip())
        {
            // Janela compacta e fixa — tudo cabe sem scroll
            form.Text = "MarnaTeklaOS";
            form.StartPosition = FormStartPosition.CenterScreen;
            form.Size = new Size(380, 480);
            form.MinimumSize = new Size(320, 420);
            form.MaximumSize = new Size(560, 560);
            form.Font = DesignSystem.F_Texto;
            form.BackColor = DesignSystem.C_Superficie;
            form.FormBorderStyle = FormBorderStyle.Sizable;
            form.MaximizeBox = false;
            form.TopMost = true;
            form.KeyPreview = true;

            toolTip.InitialDelay = 400;
            toolTip.ReshowDelay = 200;
            toolTip.AutoPopDelay = 8000;
            toolTip.ShowAlways = true;

            // ═══════════════════════════════════
            // HEADER — faixa escura, título
            // ═══════════════════════════════════
            var header = new TableLayoutPanel();
            header.Dock = DockStyle.Top;
            header.Height = 48;
            header.ColumnCount = 1;
            header.RowCount = 1;
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            header.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            header.BackColor = DesignSystem.C_Cabecalho;
            header.Padding = new Padding(16, 0, 16, 0);

            var lblTitle = new Label();
            lblTitle.Text = "MarnaTeklaOS";
            lblTitle.ForeColor = DesignSystem.C_TextoClaro;
            lblTitle.Font = DesignSystem.F_Titulo;
            lblTitle.AutoSize = true;
            lblTitle.Anchor = AnchorStyles.Left;
            lblTitle.Margin = new Padding(0);

            header.Controls.Add(lblTitle, 0, 0);

            // ═══════════════════════════════════
            // BODY — espaçamento generoso, sem rótulos de seção
            // ═══════════════════════════════════
            var body = new Panel();
            body.Dock = DockStyle.Fill;
            body.BackColor = DesignSystem.C_Superficie;
            body.Padding = new Padding(16, 14, 16, 8);

            var bodyLayout = new TableLayoutPanel();
            bodyLayout.Dock = DockStyle.Fill;
            bodyLayout.ColumnCount = 1;
            bodyLayout.RowCount = 10;
            bodyLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            // Linhas: campo | botão | sep | campo | botão | sep | btn | btn | btn | sep+btn
            bodyLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));  // txtSelecao
            bodyLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));  // btnSelecionar
            bodyLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 14F));  // separador
            bodyLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));  // txtDesenho
            bodyLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));  // btnDesenho
            bodyLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 14F));  // separador
            bodyLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));  // btnComparar
            bodyLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));  // btnNumeracao
            bodyLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));  // btnVistas3D
            bodyLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // espaço + reparo

            // --- Bloco A: Seleção por nome ---

            var txtSelectionInput = new TextBox();
            txtSelectionInput.Multiline = true;
            txtSelectionInput.ScrollBars = ScrollBars.None;
            txtSelectionInput.Dock = DockStyle.Fill;
            txtSelectionInput.Font = DesignSystem.F_Texto;
            txtSelectionInput.BackColor = DesignSystem.C_Fundo;
            txtSelectionInput.ForeColor = DesignSystem.C_TextoPrimario;
            txtSelectionInput.BorderStyle = BorderStyle.FixedSingle;
            txtSelectionInput.Margin = new Padding(0, 0, 0, 2);
            toolTip.SetToolTip(txtSelectionInput, "Nomes de conjuntos separados por virgula — PP1, PP2, VR1\nCtrl+Enter para executar.");

            var btnSelectParts = DesignSystem.CriarBotao("Selecionar conjuntos", false);
            btnSelectParts.Margin = new Padding(0, 0, 0, 0);
            toolTip.SetToolTip(btnSelectParts, "Seleciona os conjuntos informados no campo acima.");

            bodyLayout.Controls.Add(txtSelectionInput, 0, 0);
            bodyLayout.Controls.Add(btnSelectParts, 0, 1);
            bodyLayout.Controls.Add(BuildSeparator(), 0, 2);

            // --- Bloco B: Vínculo desenho → modelo ---

            var txtDrawingMark = new TextBox();
            txtDrawingMark.Dock = DockStyle.Fill;
            txtDrawingMark.Font = DesignSystem.F_Texto;
            txtDrawingMark.BackColor = DesignSystem.C_Fundo;
            txtDrawingMark.ForeColor = DesignSystem.C_TextoPrimario;
            txtDrawingMark.BorderStyle = BorderStyle.FixedSingle;
            txtDrawingMark.Margin = new Padding(0, 0, 0, 2);
            toolTip.SetToolTip(txtDrawingMark, "Marca do desenho conforme o Document Manager.\nEnter para executar.");

            var btnSelectFromDraw = DesignSystem.CriarBotao("Selecionar peca pelo desenho", false);
            btnSelectFromDraw.Margin = new Padding(0);
            toolTip.SetToolTip(btnSelectFromDraw, "Busca o desenho e seleciona a peca base no modelo 3D.");

            bodyLayout.Controls.Add(txtDrawingMark, 0, 3);
            bodyLayout.Controls.Add(btnSelectFromDraw, 0, 4);
            bodyLayout.Controls.Add(BuildSeparator(), 0, 5);

            // --- Bloco C: Verificação (sem inputs) ---

            var btnCompare = DesignSystem.CriarBotao("Comparar dois conjuntos selecionados", false);
            btnCompare.Margin = new Padding(0, 0, 0, 3);
            toolTip.SetToolTip(btnCompare, "Selecione dois conjuntos no modelo antes de usar.\nCompara numeracao, pecas e propriedades.");

            var btnNumberCheck = DesignSystem.CriarBotao("Verificar numeracao de prefixos", false);
            btnNumberCheck.Margin = new Padding(0, 0, 0, 0);
            toolTip.SetToolTip(btnNumberCheck, "Selecione pecas no modelo antes de usar.\nVerifica gaps e conjuntos sem posicao.");

            bodyLayout.Controls.Add(btnCompare, 0, 6);
            bodyLayout.Controls.Add(btnNumberCheck, 0, 7);

            // --- Bloco C2: Vistas 3D ---

            var btnVistas3D = DesignSystem.CriarBotao("Criar vistas 3D", false);
            btnVistas3D.Margin = new Padding(0, 0, 0, 0);
            toolTip.SetToolTip(btnVistas3D, "Selecione uma peca no modelo antes de usar.\nCria um desenho com 4 perspectivas isometricas.");

            bodyLayout.Controls.Add(btnVistas3D, 0, 8);

            // --- Bloco D: Reparo (ação destrutiva — alinhada ao rodapé) ---
            var bottomRow = new TableLayoutPanel();
            bottomRow.Dock = DockStyle.Fill;
            bottomRow.ColumnCount = 1;
            bottomRow.RowCount = 2;
            bottomRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            bottomRow.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            bottomRow.Margin = new Padding(0);

            var btnRepair = DesignSystem.CriarBotao("Diagnosticar e reparar modelo", false);
            btnRepair.Margin = new Padding(0);
            toolTip.SetToolTip(btnRepair, "Aciona o comando nativo de reparo de modelo no Tekla.\nUse apenas quando necessario.");

            bottomRow.Controls.Add(new Panel(), 0, 0); // espaçador
            bottomRow.Controls.Add(btnRepair, 0, 1);
            bodyLayout.Controls.Add(bottomRow, 0, 9);

            body.Controls.Add(bodyLayout);

            // ═══════════════════════════════════
            // FOOTER — status + fechar
            // ═══════════════════════════════════
            var footer = new Panel();
            footer.Dock = DockStyle.Bottom;
            footer.Height = 34;
            footer.BackColor = DesignSystem.C_Fundo;
            footer.Padding = new Padding(16, 0, 16, 0);

            var footerLayout = new TableLayoutPanel();
            footerLayout.Dock = DockStyle.Fill;
            footerLayout.ColumnCount = 2;
            footerLayout.RowCount = 1;
            footerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            footerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64F));
            footerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            footerLayout.Margin = new Padding(0);

            var lblStatus = new Label();
            lblStatus.Text = "";
            lblStatus.AutoEllipsis = true;
            lblStatus.Dock = DockStyle.Fill;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            lblStatus.ForeColor = DesignSystem.C_TextoSecundario;
            lblStatus.Font = DesignSystem.F_TextoPequeno;
            lblStatus.Margin = new Padding(0);

            var btnClose = new Button();
            btnClose.Text = "Fechar";
            btnClose.Dock = DockStyle.Fill;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.BackColor = DesignSystem.C_Fundo;
            btnClose.ForeColor = DesignSystem.C_TextoSecundario;
            btnClose.Font = DesignSystem.F_TextoPequeno;
            btnClose.Cursor = Cursors.Hand;
            btnClose.DialogResult = DialogResult.OK;
            btnClose.Margin = new Padding(0);

            footerLayout.Controls.Add(lblStatus, 0, 0);
            footerLayout.Controls.Add(btnClose, 1, 0);
            footer.Controls.Add(footerLayout);

            // ═══════════════════════════════════
            // STATUS HELPERS
            // ═══════════════════════════════════
            Action<string, Color> setStatus = delegate(string text, Color color)
            {
                lblStatus.Text = text;
                lblStatus.ForeColor = color;
            };

            // ═══════════════════════════════════
            // EVENT WIRING
            // ═══════════════════════════════════
            txtDrawingMark.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter) { btnSelectFromDraw.PerformClick(); e.SuppressKeyPress = true; }
            };

            txtSelectionInput.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.Control && e.KeyCode == Keys.Enter) { btnSelectParts.PerformClick(); e.SuppressKeyPress = true; }
            };

            Action<string, string, string, Action> run = delegate(string busy, string ok, string fail, Action action)
            {
                var prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                setStatus(busy, DesignSystem.C_TextoSecundario);

                try
                {
                    action();
                    setStatus(ok, DesignSystem.C_Sucesso);
                }
                catch (Exception ex)
                {
                    setStatus(fail, DesignSystem.C_Erro);
                    MessageBox.Show(fail + "\n\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor.Current = prev;
                }
            };

            btnSelectParts.Click += delegate
            {
                run("Selecionando...", "Selecao concluida.", "Falha ao selecionar.",
                    delegate { AssemblySelectionHelper.SelectAssemblies(txtSelectionInput.Text); });
            };

            btnSelectFromDraw.Click += delegate
            {
                run("Buscando...", "Peca selecionada.", "Falha no vinculo desenho-modelo.",
                    delegate { DrawingModelLinker.SelectModelPartFromDrawingMark(txtDrawingMark.Text); });
            };

            btnCompare.Click += delegate
            {
                run("Comparando...", "Comparacao concluida.", "Falha na comparacao.",
                    delegate { AssemblyComparator.CompareSelectedAssemblies(); });
            };

            btnNumberCheck.Click += delegate
            {
                run("Verificando...", "Verificacao concluida.", "Falha na verificacao.",
                    delegate
                    {
                        string report = NumberingChecker.CheckNumberingFromSelection();
                        if (!string.IsNullOrEmpty(report))
                            ReportWindow.ShowReport(report, "Verificacao de Numeracao");
                    });
            };

            btnRepair.Click += delegate
            {
                run("Reparando...", "Reparo enviado.", "Falha ao acionar reparo.",
                    delegate { TeklaCommands.RunModelRepair(runtime); });
            };

            btnVistas3D.Click += delegate
            {
                run("Criando vistas 3D...", "Desenho criado.", "Falha ao criar vistas 3D.",
                    delegate { IsometricViewCreator.CreateIsometricDrawing(); });
            };

            // ═══════════════════════════════════
            // MONTAR
            // ═══════════════════════════════════
            form.Controls.Add(body);
            form.Controls.Add(footer);
            form.Controls.Add(header); // header por ultimo = renderiza no topo (DockStyle.Top)
            form.AcceptButton = btnClose;
            form.CancelButton = btnClose;

            form.ShowDialog();
        }
    }

    // Linha separadora fina — espaçamento visual entre blocos
    private static Panel BuildSeparator()
    {
        var sep = new Panel();
        sep.Dock = DockStyle.Fill;
        sep.BackColor = Color.Transparent;
        sep.Margin = new Padding(0, 5, 0, 5);
        var line = new Panel();
        line.Height = 1;
        line.Dock = DockStyle.Bottom;
        line.BackColor = DesignSystem.C_BordaLeve;
        sep.Controls.Add(line);
        return sep;
    }
}
