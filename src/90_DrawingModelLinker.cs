internal static class DrawingModelLinker
{
    // Selecionar no modelo a partir da marca digitada do desenho
    public static void SelectModelPartFromDrawingMark(string mark)
    {
        if (string.IsNullOrWhiteSpace(mark))
        {
            MessageBox.Show("Por favor, digite o nome/marca do desenho.", "Caixa Vazia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        mark = mark.Trim();
        string cleanInput = NormalizeMark(mark);
        if (cleanInput.Length == 0)
        {
            MessageBox.Show("A marca informada nao contem caracteres validos.", "Entrada invalida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Model model = ModelHelper.GetConnectedModel();
        if (model == null)
        {
            return;
        }

        Tekla.Structures.Drawing.DrawingHandler drawingHandler = new Tekla.Structures.Drawing.DrawingHandler();
        Tekla.Structures.Drawing.DrawingEnumerator drawings = drawingHandler.GetDrawings();
        if (drawings == null)
        {
            MessageBox.Show("Nao foi possivel acessar a lista de desenhos.");
            return;
        }

        Tekla.Structures.Drawing.Drawing bestDrawing = null;
        Identifier bestIdentifier = null;
        int bestScore = 0;

        while (drawings.MoveNext())
        {
            Tekla.Structures.Drawing.Drawing drawing = drawings.Current as Tekla.Structures.Drawing.Drawing;
            if (drawing == null)
            {
                continue;
            }

            Identifier modelIdentifier = GetDrawingModelIdentifier(drawing);
            if (modelIdentifier == null)
            {
                continue;
            }

            int score = GetDrawingMatchScore(cleanInput, drawing.Mark, drawing.Name);
            if (score <= bestScore)
            {
                continue;
            }

            bestScore = score;
            bestDrawing = drawing;
            bestIdentifier = modelIdentifier;
        }

        if (bestIdentifier == null)
        {
            MessageBox.Show(
                string.Format("Desenho '{0}' nao encontrado na lista de desenhos.\n\nVerifique o 'Mark' ou 'Name' no Document Manager.", mark),
                "Nao encontrado",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        ModelObject modelObj = model.SelectModelObject(bestIdentifier);
        if (modelObj == null)
        {
            MessageBox.Show(
                string.Format("O desenho '{0}' foi localizado, mas a peca base nao foi encontrada no modelo.", mark),
                "Peca nao encontrada",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        var selector = new ModelUI.ModelObjectSelector();
        selector.Select(new ArrayList() { modelObj }, false);

        Tekla.Structures.Model.Operations.Operation.DisplayPrompt(
            string.Format("Desenho [{0}] -> Peca do modelo base selecionada!", Formatters.FormatValue(bestDrawing == null ? null : bestDrawing.Mark)));

        try
        {
            Tekla.Structures.Model.Operations.Operation.RunMacro("ZoomToSelected");
        }
        catch
        {
            // Nao bloquear o fluxo se a macro de zoom nao existir.
        }
    }

    // Selecao reversa: abrir desenho associado a partir de objeto selecionado no modelo
    public static void OpenDrawingFromSelectedModelPart()
    {
        if (ModelHelper.GetConnectedModel() == null)
        {
            return;
        }

        var selector = new ModelUI.ModelObjectSelector();
        var selectedEnum = selector.GetSelectedObjects();

        if (selectedEnum == null || selectedEnum.GetSize() == 0)
        {
            MessageBox.Show("Voce precisa selecionar um objeto no modelo 3D primeiro.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!selectedEnum.MoveNext())
        {
            MessageBox.Show("Nao foi possivel ler a selecao atual.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        ModelObject selectedObj = selectedEnum.Current as ModelObject;
        if (selectedObj == null)
        {
            MessageBox.Show("Selecione uma peca ou conjunto valido no modelo.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Dictionary<string, string> markCandidates = BuildMarkCandidates(selectedObj);
        if (markCandidates.Count == 0)
        {
            MessageBox.Show("Nao foi possivel determinar uma marca valida para busca do desenho.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Tekla.Structures.Drawing.DrawingHandler drawingHandler = new Tekla.Structures.Drawing.DrawingHandler();
        Tekla.Structures.Drawing.DrawingEnumerator drawings = drawingHandler.GetDrawings();
        if (drawings == null)
        {
            MessageBox.Show("Nao foi possivel acessar a lista de desenhos.");
            return;
        }

        Tekla.Structures.Drawing.Drawing bestDrawing = null;
        int bestScore = 0;

        while (drawings.MoveNext())
        {
            Tekla.Structures.Drawing.Drawing drawing = drawings.Current as Tekla.Structures.Drawing.Drawing;
            if (drawing == null)
            {
                continue;
            }

            int localBestScore = 0;
            foreach (KeyValuePair<string, string> candidate in markCandidates)
            {
                int score = GetDrawingMatchScore(candidate.Key, drawing.Mark, drawing.Name);
                if (score > localBestScore)
                {
                    localBestScore = score;
                }
            }

            if (localBestScore <= bestScore)
            {
                continue;
            }

            bestScore = localBestScore;
            bestDrawing = drawing;
        }

        if (bestDrawing == null)
        {
            var attempted = new List<string>(markCandidates.Values);
            MessageBox.Show(
                string.Format("Nenhum desenho encontrado para as marcas: {0}\n\nVerifique no Document Manager se o desenho ja foi criado.", string.Join(", ", attempted.ToArray())),
                "Nao encontrado",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        drawingHandler.SetActiveDrawing(bestDrawing, true);
    }

    private static Identifier GetDrawingModelIdentifier(Tekla.Structures.Drawing.Drawing drawing)
    {
        if (drawing == null)
        {
            return null;
        }

        Tekla.Structures.Drawing.AssemblyDrawing assemblyDrawing = drawing as Tekla.Structures.Drawing.AssemblyDrawing;
        if (assemblyDrawing != null)
        {
            return assemblyDrawing.AssemblyIdentifier;
        }

        Tekla.Structures.Drawing.SinglePartDrawing singlePartDrawing = drawing as Tekla.Structures.Drawing.SinglePartDrawing;
        if (singlePartDrawing != null)
        {
            return singlePartDrawing.PartIdentifier;
        }

        Tekla.Structures.Drawing.CastUnitDrawing castUnitDrawing = drawing as Tekla.Structures.Drawing.CastUnitDrawing;
        if (castUnitDrawing != null)
        {
            return castUnitDrawing.CastUnitIdentifier;
        }

        return null;
    }

    private static Dictionary<string, string> BuildMarkCandidates(ModelObject selectedObj)
    {
        var candidates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (selectedObj == null)
        {
            return candidates;
        }

        Assembly assembly = selectedObj as Assembly;
        if (assembly != null)
        {
            AddMarkCandidate(candidates, ModelHelper.GetReportProperty(assembly, "ASSEMBLY_POS"));
            AddMarkCandidate(candidates, BuildNumberingText(assembly.AssemblyNumber));
            return candidates;
        }

        Part part = selectedObj as Part;
        if (part == null)
        {
            return candidates;
        }

        AddMarkCandidate(candidates, ModelHelper.GetReportProperty(part, "PART_POS"));
        AddMarkCandidate(candidates, BuildNumberingText(part.PartNumber));

        Assembly parentAssembly = part.GetAssembly();
        if (parentAssembly != null)
        {
            AddMarkCandidate(candidates, ModelHelper.GetReportProperty(parentAssembly, "ASSEMBLY_POS"));
            AddMarkCandidate(candidates, BuildNumberingText(parentAssembly.AssemblyNumber));
        }

        return candidates;
    }

    private static void AddMarkCandidate(Dictionary<string, string> candidates, string value)
    {
        if (candidates == null || string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        string trimmed = value.Trim();
        if (trimmed == "-")
        {
            return;
        }

        string normalized = NormalizeMark(trimmed);
        if (normalized.Length == 0)
        {
            return;
        }

        if (!candidates.ContainsKey(normalized))
        {
            candidates.Add(normalized, trimmed);
        }
    }

    private static string BuildNumberingText(NumberingSeries numbering)
    {
        if (numbering == null || string.IsNullOrWhiteSpace(numbering.Prefix))
        {
            return null;
        }

        return string.Format("{0}{1}", numbering.Prefix.Trim(), numbering.StartNumber);
    }

    private static int GetDrawingMatchScore(string normalizedInput, string drawingMark, string drawingName)
    {
        if (string.IsNullOrEmpty(normalizedInput))
        {
            return 0;
        }

        int best = 0;

        // Match exato da marca principal do desenho (ex.: PM.13 em "[PM.13 - 1]").
        string primaryMark = ExtractPrimaryMark(drawingMark);
        if (IsExactNormalizedMatch(normalizedInput, primaryMark))
        {
            best = 220;
        }

        // Match exato da marca completa (com sufixos, quando informado).
        if (IsExactNormalizedMatch(normalizedInput, drawingMark) && best < 180)
        {
            best = 180;
        }

        // Match exato do nome do desenho como fallback.
        if (IsExactNormalizedMatch(normalizedInput, drawingName) && best < 140)
        {
            best = 140;
        }

        return best;
    }

    private static bool IsExactNormalizedMatch(string normalizedInput, string text)
    {
        if (string.IsNullOrEmpty(normalizedInput) || string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        string normalizedText = NormalizeMark(text);
        if (normalizedText.Length == 0)
        {
            return false;
        }

        return string.Equals(normalizedInput, normalizedText, StringComparison.Ordinal);
    }

    private static string ExtractPrimaryMark(string mark)
    {
        if (string.IsNullOrWhiteSpace(mark))
        {
            return string.Empty;
        }

        string clean = mark.Trim();
        if (clean.StartsWith("[", StringComparison.Ordinal) && clean.EndsWith("]", StringComparison.Ordinal) && clean.Length > 1)
        {
            clean = clean.Substring(1, clean.Length - 2).Trim();
        }

        int separatorIndex = clean.IndexOf(" - ", StringComparison.Ordinal);
        if (separatorIndex > 0)
        {
            return clean.Substring(0, separatorIndex).Trim();
        }

        return clean;
    }

    private static string NormalizeMark(string mark)
    {
        if (string.IsNullOrWhiteSpace(mark))
        {
            return string.Empty;
        }

        var sb = new StringBuilder(mark.Length);
        for (int i = 0; i < mark.Length; i++)
        {
            char c = mark[i];
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(char.ToUpperInvariant(c));
            }
        }

        return sb.ToString();
    }
}
