// ═══════════════════════════════════════════════════════════════
// 95_IsometricViewCreator.cs — Cria desenho com 4 vistas 3D
// Cria um GADrawing com 4 perspectivas isometricas da peca/
// assembly selecionado no modelo.
//
// LIMITACOES CONHECIDAS DA TEKLA OPEN API:
// - A API de desenhos (Tekla.Structures.Drawing) nao oferece
//   um metodo direto "Create3dView" ou "CreateIsometricView".
// - A vista 3D e criada via construtor View() com CoordinateSystem
//   rotacionado para simular a projecao isometrica.
// - O resultado e uma projecao ortografica orientada — nao
//   uma perspectiva conica real. Para engenharia, isso e o
//   padrao e o comportamento esperado.
// - O layout 2x2 e posicionado manualmente via View.Origin.
// ═══════════════════════════════════════════════════════════════
internal static class IsometricViewCreator
{
    // Nomes romanos das 4 vistas
    private static readonly string[] ROMAN = new string[] { "I", "II", "III", "IV" };

    // Angulos de rotacao em Z para cada vista (graus)
    private static readonly double[] Z_ANGLES = new double[] { 45.0, 135.0, 225.0, 315.0 };

    // Angulo de elevacao em X (graus) — igual para todas
    private const double X_ANGLE = 25.0;

    // ───────────────────────────────────────────────
    // Ponto de entrada publico
    // ───────────────────────────────────────────────
    public static void CreateIsometricDrawing()
    {
        // 1. Obter a peca selecionada
        Part selectedPart = GetSelectedPart();
        if (selectedPart == null) return;

        // 2. Obter o assembly pai (ou a propria peca se nao tiver)
        Assembly assembly = selectedPart.GetAssembly();
        if (assembly == null)
        {
            MessageBox.Show(
                "Nao foi possivel obter o conjunto (Assembly) da peca selecionada.",
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Garantir que o assembly esta completamente populado no banco
        if (!assembly.Select())
        {
            MessageBox.Show(
                "Nao foi possivel carregar os dados do assembly.\nVerifique se a peca pertence a um conjunto valido.",
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // 3. Obter nome amigavel
        string displayName = GetPartDisplayName(selectedPart, assembly);

        // 4. Calcular bounding box do assembly
        AABB boundingBox = GetAssemblyBoundingBox(assembly);
        if (boundingBox == null)
        {
            MessageBox.Show(
                "Nao foi possivel calcular o volume da peca selecionada.",
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // 5. Criar o desenho (tenta AssemblyDrawing, fallback para SinglePartDrawing)
        Tekla.Structures.Drawing.Drawing drawing = CreateDrawing(assembly, selectedPart, displayName);
        if (drawing == null) return;

        // 6. Criar as 4 vistas isometricas
        bool viewsCreated = CreatePerspectiveViews(drawing, boundingBox, displayName);
        if (!viewsCreated)
        {
            MessageBox.Show(
                string.Format("O desenho '{0}' foi criado, mas houve falha ao inserir as vistas.\nAbra o desenho manualmente no Document Manager.", displayName),
                "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // 6b. Esconder todos os objetos que NAO pertencem ao assembly selecionado
        HideNonAssemblyObjects(drawing, assembly, selectedPart);

        // 7. Abrir o desenho
        OpenDrawing(drawing, displayName);
    }

    // ───────────────────────────────────────────────
    // Obter exatamente 1 Part da selecao
    // ───────────────────────────────────────────────
    private static Part GetSelectedPart()
    {
        Model model = ModelHelper.GetConnectedModel();
        if (model == null) return null;

        ModelUI.ModelObjectSelector selector = new ModelUI.ModelObjectSelector();
        ModelObjectEnumerator selected = selector.GetSelectedObjects();
        if (selected == null || selected.GetSize() == 0)
        {
            MessageBox.Show(
                "Selecione uma peca antes de executar a macro.",
                "Nenhuma selecao", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return null;
        }

        if (selected.GetSize() > 1)
        {
            MessageBox.Show(
                "Selecione apenas uma peca por vez.",
                "Multiplas pecas", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return null;
        }

        selected.MoveNext();
        ModelObject obj = selected.Current;

        // Se selecionou um Assembly, pegar a peca principal
        Assembly ass = obj as Assembly;
        if (ass != null)
        {
            Part mainPart = ass.GetMainPart() as Part;
            if (mainPart != null) return mainPart;
        }

        Part part = obj as Part;
        if (part != null) return part;

        MessageBox.Show(
            "O objeto selecionado nao e uma peca valida (Part).\nSelecione uma peca de concreto ou aco no modelo.",
            "Tipo invalido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return null;
    }

    // ───────────────────────────────────────────────
    // Nome amigavel para a peca
    // ───────────────────────────────────────────────
    private static string GetPartDisplayName(Part part, Assembly assembly)
    {
        // Tentar ASSEMBLY_POS primeiro (ex: "PP-1")
        string assemblyPos = ModelHelper.GetReportProperty(assembly, "ASSEMBLY_POS");
        if (assemblyPos != null && assemblyPos != "-" && assemblyPos.Trim().Length > 0)
        {
            return SanitizeName(assemblyPos.Trim());
        }

        // Tentar PART_POS
        string partPos = ModelHelper.GetReportProperty(part, "PART_POS");
        if (partPos != null && partPos != "-" && partPos.Trim().Length > 0)
        {
            return SanitizeName(partPos.Trim());
        }

        // Nome da peca
        if (!string.IsNullOrEmpty(part.Name))
        {
            return SanitizeName(part.Name.Trim());
        }

        // Fallback: ID
        if (part.Identifier != null)
        {
            return string.Format("ID_{0}", part.Identifier.ID);
        }

        return "Peca";
    }

    // ───────────────────────────────────────────────
    // Sanitizar nome para uso em titulos
    // ───────────────────────────────────────────────
    private static string SanitizeName(string name)
    {
        if (string.IsNullOrEmpty(name)) return "Peca";

        var sb = new StringBuilder(name.Length);
        for (int i = 0; i < name.Length; i++)
        {
            char c = name[i];
            if (char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.' || c == ' ')
            {
                sb.Append(c);
            }
        }

        string result = sb.ToString().Trim();
        return result.Length > 0 ? result : "Peca";
    }

    // ───────────────────────────────────────────────
    // Bounding box do assembly (todas as pecas)
    // ───────────────────────────────────────────────
    private static AABB GetAssemblyBoundingBox(Assembly assembly)
    {
        try
        {
            ArrayList parts = assembly.GetSubAssemblies();
            Part mainPart = assembly.GetMainPart() as Part;

            double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;
            bool hasData = false;

            // Processar a peca principal
            if (mainPart != null)
            {
                Solid solid = mainPart.GetSolid();
                if (solid != null)
                {
                    ExpandBounds(solid.MinimumPoint, solid.MaximumPoint,
                        ref minX, ref minY, ref minZ, ref maxX, ref maxY, ref maxZ);
                    hasData = true;
                }
            }

            // Processar secundarias do assembly
            ArrayList secondaries = assembly.GetSecondaries();
            if (secondaries != null)
            {
                for (int i = 0; i < secondaries.Count; i++)
                {
                    Part secPart = secondaries[i] as Part;
                    if (secPart == null) continue;

                    Solid solid = secPart.GetSolid();
                    if (solid == null) continue;

                    ExpandBounds(solid.MinimumPoint, solid.MaximumPoint,
                        ref minX, ref minY, ref minZ, ref maxX, ref maxY, ref maxZ);
                    hasData = true;
                }
            }

            if (!hasData) return null;

            // Adicionar margem de 10% em cada direcao
            double marginX = (maxX - minX) * 0.10;
            double marginY = (maxY - minY) * 0.10;
            double marginZ = (maxZ - minZ) * 0.10;
            double minMargin = 100.0; // mm minimo
            if (marginX < minMargin) marginX = minMargin;
            if (marginY < minMargin) marginY = minMargin;
            if (marginZ < minMargin) marginZ = minMargin;

            return new AABB(
                new Tekla.Structures.Geometry3d.Point(minX - marginX, minY - marginY, minZ - marginZ),
                new Tekla.Structures.Geometry3d.Point(maxX + marginX, maxY + marginY, maxZ + marginZ));
        }
        catch
        {
            return null;
        }
    }

    private static void ExpandBounds(Tekla.Structures.Geometry3d.Point min, Tekla.Structures.Geometry3d.Point max,
        ref double minX, ref double minY, ref double minZ,
        ref double maxX, ref double maxY, ref double maxZ)
    {
        if (min == null || max == null) return;
        if (min.X < minX) minX = min.X;
        if (min.Y < minY) minY = min.Y;
        if (min.Z < minZ) minZ = min.Z;
        if (max.X > maxX) maxX = max.X;
        if (max.Y > maxY) maxY = max.Y;
        if (max.Z > maxZ) maxZ = max.Z;
    }

    // ───────────────────────────────────────────────
    // Criar desenho — tenta os 3 tipos de desenho na ordem:
    //   1. CastUnitDrawing (concreto pre-fabricado)
    //   2. AssemblyDrawing (aco / assemblies reais)
    //   3. SinglePartDrawing (pecas simples)
    // ───────────────────────────────────────────────
    private static Tekla.Structures.Drawing.Drawing CreateDrawing(
        Assembly assembly, Part selectedPart, string displayName)
    {
        Tekla.Structures.Drawing.DrawingHandler drawingHandler =
            new Tekla.Structures.Drawing.DrawingHandler();

        // Fechar desenho ativo se houver
        try { drawingHandler.CloseActiveDrawing(); }
        catch { /* Ignorar se nao ha desenho ativo */ }

        // Garantir que o modelo esta sincronizado
        Model commitModel = new Model();
        commitModel.CommitChanges();

        // Garantir que a peca esta completamente carregada
        if (selectedPart != null) selectedPart.Select();

        string lastError = "";

        // Tentativa 1: CastUnitDrawing com assembly ID (para concreto pre-fabricado)
        // O CastUnit E o Assembly retornado por GetAssembly() em pecas de concreto
        try
        {
            if (assembly != null && assembly.Identifier != null && assembly.Identifier.ID > 0)
            {
                Tekla.Structures.Drawing.CastUnitDrawing cuDrawing =
                    new Tekla.Structures.Drawing.CastUnitDrawing(assembly.Identifier);
                cuDrawing.Name = string.Format("3D_{0}", displayName);

                if (cuDrawing.Insert())
                {
                    RemoveDefaultViews(cuDrawing);
                    return cuDrawing;
                }
                lastError = "CastUnitDrawing(assembly).Insert() retornou false";
            }
        }
        catch (Exception ex1)
        {
            lastError = string.Format("CastUnit(asm): {0}", ex1.Message);
        }

        // Tentativa 1b: CastUnitDrawing com part ID (variante)
        try
        {
            if (selectedPart != null && selectedPart.Identifier != null && selectedPart.Identifier.ID > 0)
            {
                Tekla.Structures.Drawing.CastUnitDrawing cuDrawing2 =
                    new Tekla.Structures.Drawing.CastUnitDrawing(selectedPart.Identifier);
                cuDrawing2.Name = string.Format("3D_{0}", displayName);

                if (cuDrawing2.Insert())
                {
                    RemoveDefaultViews(cuDrawing2);
                    return cuDrawing2;
                }
                lastError = "CastUnitDrawing(part).Insert() retornou false";
            }
        }
        catch (Exception ex1b)
        {
            lastError = string.Format("CastUnit(part): {0}", ex1b.Message);
        }

        // Tentativa 2: AssemblyDrawing (para pecas de aco)
        try
        {
            if (assembly != null && assembly.Identifier != null && assembly.Identifier.ID > 0)
            {
                Tekla.Structures.Drawing.AssemblyDrawing asmDrawing =
                    new Tekla.Structures.Drawing.AssemblyDrawing(assembly.Identifier);
                asmDrawing.Name = string.Format("3D_{0}", displayName);

                if (asmDrawing.Insert())
                {
                    RemoveDefaultViews(asmDrawing);
                    return asmDrawing;
                }
                lastError = "AssemblyDrawing.Insert() retornou false";
            }
        }
        catch (Exception ex2)
        {
            lastError = string.Format("Assembly: {0}", ex2.Message);
        }

        // Tentativa 3: SinglePartDrawing (ultimo recurso)
        try
        {
            if (selectedPart != null && selectedPart.Identifier != null && selectedPart.Identifier.ID > 0)
            {
                Tekla.Structures.Drawing.SinglePartDrawing spDrawing =
                    new Tekla.Structures.Drawing.SinglePartDrawing(selectedPart.Identifier);
                spDrawing.Name = string.Format("3D_{0}", displayName);

                if (spDrawing.Insert())
                {
                    RemoveDefaultViews(spDrawing);
                    return spDrawing;
                }
                lastError = "SinglePartDrawing.Insert() retornou false";
            }
        }
        catch (Exception ex3)
        {
            lastError = string.Format("SinglePart: {0}", ex3.Message);
        }

        MessageBox.Show(
            string.Format("Nao foi possivel criar nenhum tipo de desenho para esta peca.\n\nUltimo erro: {0}\n\nTipo da peca: {1}\nID peca: {2}\nID assembly: {3}",
                lastError,
                selectedPart != null ? selectedPart.GetType().Name : "null",
                selectedPart != null && selectedPart.Identifier != null ? selectedPart.Identifier.ID.ToString() : "?",
                assembly != null && assembly.Identifier != null ? assembly.Identifier.ID.ToString() : "?"),
            "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return null;
    }

    // ───────────────────────────────────────────────
    // Esconder objetos que NAO pertencem ao assembly alvo
    // Percorre cada vista do desenho e esconde pecas
    // cujo ID de modelo nao faz parte do assembly selecionado.
    // ───────────────────────────────────────────────
    private static void HideNonAssemblyObjects(
        Tekla.Structures.Drawing.Drawing drawing,
        Assembly assembly,
        Part selectedPart)
    {
        try
        {
            // 1. Coletar todos os IDs de pecas do assembly alvo
            System.Collections.Generic.HashSet<int> allowedIds = new System.Collections.Generic.HashSet<int>();

            // Peca principal
            if (selectedPart != null && selectedPart.Identifier != null)
            {
                allowedIds.Add(selectedPart.Identifier.ID);
            }

            // Peca principal do assembly (pode ser diferente se selecao veio por Assembly)
            if (assembly != null)
            {
                Part mainPart = assembly.GetMainPart() as Part;
                if (mainPart != null && mainPart.Identifier != null)
                {
                    allowedIds.Add(mainPart.Identifier.ID);
                }

                // Pecas secundarias
                ArrayList secondaries = assembly.GetSecondaries();
                if (secondaries != null)
                {
                    for (int i = 0; i < secondaries.Count; i++)
                    {
                        Part secPart = secondaries[i] as Part;
                        if (secPart != null && secPart.Identifier != null)
                        {
                            allowedIds.Add(secPart.Identifier.ID);
                        }
                    }
                }

                // Sub-assemblies (recursivo — nao profundo, apenas 1 nivel)
                ArrayList subAssemblies = assembly.GetSubAssemblies();
                if (subAssemblies != null)
                {
                    for (int i = 0; i < subAssemblies.Count; i++)
                    {
                        Assembly subAsm = subAssemblies[i] as Assembly;
                        if (subAsm == null) continue;

                        Part subMain = subAsm.GetMainPart() as Part;
                        if (subMain != null && subMain.Identifier != null)
                        {
                            allowedIds.Add(subMain.Identifier.ID);
                        }

                        ArrayList subSecs = subAsm.GetSecondaries();
                        if (subSecs != null)
                        {
                            for (int j = 0; j < subSecs.Count; j++)
                            {
                                Part sp = subSecs[j] as Part;
                                if (sp != null && sp.Identifier != null)
                                {
                                    allowedIds.Add(sp.Identifier.ID);
                                }
                            }
                        }
                    }
                }

                // O proprio assembly tambem
                if (assembly.Identifier != null)
                {
                    allowedIds.Add(assembly.Identifier.ID);
                }
            }

            if (allowedIds.Count == 0) return;

            // 2. Percorrer cada vista e esconder objetos nao pertencentes
            Tekla.Structures.Drawing.ContainerView sheet = drawing.GetSheet();
            if (sheet == null) return;

            Tekla.Structures.Drawing.DrawingObjectEnumerator sheetObjects = sheet.GetObjects();
            if (sheetObjects == null) return;

            while (sheetObjects.MoveNext())
            {
                Tekla.Structures.Drawing.View view =
                    sheetObjects.Current as Tekla.Structures.Drawing.View;
                if (view == null) continue;

                // Obter todos os objetos desta vista
                Tekla.Structures.Drawing.DrawingObjectEnumerator viewObjects =
                    view.GetObjects();
                if (viewObjects == null) continue;

                while (viewObjects.MoveNext())
                {
                    Tekla.Structures.Drawing.DrawingObject drawObj = viewObjects.Current;
                    if (drawObj == null) continue;

                    // Verificar se e uma Part de desenho
                    Tekla.Structures.Drawing.Part drawPart =
                        drawObj as Tekla.Structures.Drawing.Part;
                    if (drawPart == null) continue;

                    // Obter o Identifier do modelo vinculado
                    try
                    {
                        Tekla.Structures.Identifier modelId = drawPart.ModelIdentifier;
                        if (modelId == null) continue;

                        // Se NAO pertence ao assembly, remover da vista
                        if (!allowedIds.Contains(modelId.ID))
                        {
                            drawPart.Delete();
                        }
                    }
                    catch
                    {
                        // Ignorar objetos sem ModelIdentifier
                    }
                }
            }

            // Salvar as alteracoes
            drawing.CommitChanges();
        }
        catch
        {
            // Nao bloquear se a filtragem falhar — o desenho fica "sujo"
            // mas e melhor que nao ter desenho nenhum
        }
    }

    // ───────────────────────────────────────────────
    // Remover vistas auto-geradas do Assembly Drawing
    // ───────────────────────────────────────────────
    private static void RemoveDefaultViews(Tekla.Structures.Drawing.Drawing drawing)
    {
        try
        {
            Tekla.Structures.Drawing.ContainerView sheet = drawing.GetSheet();
            if (sheet == null) return;

            Tekla.Structures.Drawing.DrawingObjectEnumerator objects = sheet.GetObjects();
            if (objects == null) return;

            ArrayList viewsToDelete = new ArrayList();
            while (objects.MoveNext())
            {
                Tekla.Structures.Drawing.View view =
                    objects.Current as Tekla.Structures.Drawing.View;
                if (view != null)
                {
                    viewsToDelete.Add(view);
                }
            }

            for (int i = 0; i < viewsToDelete.Count; i++)
            {
                Tekla.Structures.Drawing.View v =
                    viewsToDelete[i] as Tekla.Structures.Drawing.View;
                if (v != null)
                {
                    v.Delete();
                }
            }

            drawing.Modify();
        }
        catch
        {
            // Nao bloquear se falhar ao remover vistas padrao
        }
    }

    // ───────────────────────────────────────────────
    // Criar as 4 vistas isometricas com rotacoes
    // ───────────────────────────────────────────────
    private static bool CreatePerspectiveViews(
        Tekla.Structures.Drawing.Drawing drawing,
        AABB boundingBox,
        string displayName)
    {
        try
        {
            Tekla.Structures.Drawing.ContainerView sheet = drawing.GetSheet();
            if (sheet == null)
            {
                MessageBox.Show("Nao foi possivel obter a folha do desenho.", "Erro",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Centro do bounding box — origem das vistas (coordenadas do modelo)
            Tekla.Structures.Geometry3d.Point center = new Tekla.Structures.Geometry3d.Point(
                (boundingBox.MinPoint.X + boundingBox.MaxPoint.X) / 2.0,
                (boundingBox.MinPoint.Y + boundingBox.MaxPoint.Y) / 2.0,
                (boundingBox.MinPoint.Z + boundingBox.MaxPoint.Z) / 2.0);

            // Semi-extensoes da peca — o AABB da vista e RELATIVO ao viewCS,
            // nao em coordenadas globais do modelo.
            // Usar a semi-diagonal maxima para garantir que a peca fique
            // visivel de qualquer angulo de rotacao.
            double halfX = (boundingBox.MaxPoint.X - boundingBox.MinPoint.X) / 2.0;
            double halfY = (boundingBox.MaxPoint.Y - boundingBox.MinPoint.Y) / 2.0;
            double halfZ = (boundingBox.MaxPoint.Z - boundingBox.MinPoint.Z) / 2.0;
            double maxHalf = halfX;
            if (halfY > maxHalf) maxHalf = halfY;
            if (halfZ > maxHalf) maxHalf = halfZ;
            // Margem extra de 20% para folga visual
            maxHalf = maxHalf * 1.2;
            if (maxHalf < 500.0) maxHalf = 500.0; // minimo 500mm

            AABB localBox = new AABB(
                new Tekla.Structures.Geometry3d.Point(-maxHalf, -maxHalf, -maxHalf),
                new Tekla.Structures.Geometry3d.Point(maxHalf, maxHalf, maxHalf));

            int viewsInserted = 0;

            for (int i = 0; i < 4; i++)
            {
                string viewName = string.Format("Perspectiva {0} - {1}", ROMAN[i], displayName);

                // Calcular sistema de coordenadas rotacionado
                // viewCS: origem no centro da peca (modelo), eixos rotacionados
                CoordinateSystem viewCS = CreateViewRotation(center, Z_ANGLES[i], X_ANGLE);
                // displayCS: mesma rotacao mas origem em (0,0,0) — projecao no papel
                CoordinateSystem displayCS = CreateViewRotation(
                    new Tekla.Structures.Geometry3d.Point(0, 0, 0), Z_ANGLES[i], X_ANGLE);

                try
                {
                    // Criar a vista — localBox e relativo ao viewCS
                    Tekla.Structures.Drawing.View view = new Tekla.Structures.Drawing.View(
                        sheet, viewCS, displayCS, localBox);

                    view.Name = viewName;

                    if (view.Insert())
                    {
                        viewsInserted++;
                    }
                }
                catch (Exception exView)
                {
                    // Log mas nao abortar — tentar as demais vistas
                    System.Diagnostics.Debug.WriteLine(
                        string.Format("Falha ao criar vista {0}: {1}", ROMAN[i], exView.Message));
                }
            }

            // Posicionar vistas automaticamente na folha
            if (viewsInserted > 0)
            {
                try
                {
                    drawing.PlaceViews();
                }
                catch
                {
                    // PlaceViews pode nao estar disponivel em todas as versoes
                    // As vistas ficam inseridas mas podem precisar de posicionamento manual
                }

                drawing.Modify();
            }

            if (viewsInserted == 0) return false;

            if (viewsInserted < 4)
            {
                MessageBox.Show(
                    string.Format("Apenas {0} de 4 vistas foram criadas com sucesso.\nAs demais podem ter falhado por limitacao da API.", viewsInserted),
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                string.Format("Erro ao criar as vistas: {0}", ex.Message),
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    // ───────────────────────────────────────────────
    // Criar CoordinateSystem para vista isometrica
    //
    // Logica de camera 3D:
    //   - Parte de uma VISTA FRONTAL (olhando ao longo de -Y)
    //     AxisX = (1,0,0) "direita", AxisY = (0,0,1) "cima"
    //   - Rotaciona o azimute (Z) para girar ao redor da peca
    //   - Inclina a elevacao (X) para olhar de cima
    //
    // zAngleDeg: rotacao em torno do eixo Z (azimute)
    // xAngleDeg: rotacao em torno do eixo X (elevacao)
    // ───────────────────────────────────────────────
    private static CoordinateSystem CreateViewRotation(Tekla.Structures.Geometry3d.Point origin, double zAngleDeg, double xAngleDeg)
    {
        double zRad = zAngleDeg * Math.PI / 180.0;
        double xRad = xAngleDeg * Math.PI / 180.0;

        // AxisX = vetor "Right" da camera
        // Parte de (1,0,0) e gira ao redor de Z pelo azimute
        // Nao e afetado pela elevacao (permanece horizontal)
        double axisXx = Math.Cos(zRad);
        double axisXy = Math.Sin(zRad);
        double axisXz = 0.0;

        // AxisY = vetor "Up" da camera
        // Parte de (0,0,1) e e inclinado pela elevacao
        // Rotacao do Up ao redor do eixo Right:
        //   Up_new = Up * cos(elev) + LookDir * sin(elev)
        //   onde LookDir (antes da elevacao) = (sin(z), -cos(z), 0)
        double axisYx = Math.Sin(zRad) * Math.Sin(xRad);
        double axisYy = -Math.Cos(zRad) * Math.Sin(xRad);
        double axisYz = Math.Cos(xRad);

        Vector axisX = new Vector(axisXx, axisXy, axisXz);
        Vector axisY = new Vector(axisYx, axisYy, axisYz);

        return new CoordinateSystem(origin, axisX, axisY);
    }

    // ───────────────────────────────────────────────
    // Abrir o desenho no editor
    // ───────────────────────────────────────────────
    private static void OpenDrawing(Tekla.Structures.Drawing.Drawing drawing, string displayName)
    {
        try
        {
            Tekla.Structures.Drawing.DrawingHandler drawingHandler =
                new Tekla.Structures.Drawing.DrawingHandler();

            bool opened = drawingHandler.SetActiveDrawing(drawing);

            if (!opened)
            {
                MessageBox.Show(
                    string.Format("O desenho '{0}' foi criado com sucesso mas nao pode ser aberto automaticamente.\nAbra-o pelo Document Manager.", displayName),
                    "Desenho criado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                string.Format("O desenho foi criado, mas houve erro ao abri-lo:\n{0}\n\nAbra pelo Document Manager.", ex.Message),
                "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
