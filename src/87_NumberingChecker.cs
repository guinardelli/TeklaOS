internal static class NumberingChecker
{
    // Ponto de entrada: extrai prefixos da selecao e verifica o modelo para esses prefixos
    public static string CheckNumberingFromSelection()
    {
        Model model = ModelHelper.GetConnectedModel();
        if (model == null) return null;

        // 1. Ler selecao e extrair prefixos unicos
        //    Aceita Assembly selecionado diretamente (ferramenta conjunto)
        //    ou Part selecionada (ferramenta objetos em componente / selecionar partes)
        ModelUI.ModelObjectSelector uiSelector = new ModelUI.ModelObjectSelector();
        ModelObjectEnumerator enumSelected = uiSelector.GetSelectedObjects();
        if (enumSelected == null)
        {
            MessageBox.Show("Selecione ao menos uma peca no modelo.", "Verificar Numeracao", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return null;
        }

        var targetPrefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (enumSelected.MoveNext())
        {
            ModelObject obj = enumSelected.Current as ModelObject;
            if (obj == null) continue;

            // Tentar obter o Assembly: direto ou via Part pai
            Assembly assembly = obj as Assembly;
            if (assembly == null)
            {
                Part part = obj as Part;
                if (part != null)
                {
                    assembly = part.GetAssembly() as Assembly;
                }
            }

            if (assembly == null) continue;

            string position = ModelHelper.GetReportProperty(assembly, "ASSEMBLY_POS");
            if (string.IsNullOrWhiteSpace(position) || position == "-") continue;

            string prefix;
            int number;
            if (DecomposePosition(position, out prefix, out number))
            {
                targetPrefixes.Add(prefix);
            }
        }

        if (targetPrefixes.Count == 0)
        {
            MessageBox.Show("Nenhum prefixo identificado nas pecas selecionadas.\nVerifique se as pecas possuem posicao de conjunto definida.", "Verificar Numeracao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return null;
        }

        var prefixList = new List<string>(targetPrefixes);
        prefixList.Sort(StringComparer.OrdinalIgnoreCase);
        string prefixDisplay = string.Join(", ", prefixList.ToArray());

        // 2. Para cada prefixo, consultar o modelo via filtro STARTS_WITH (muito mais rapido)
        //    Em vez de varrer todos os assemblies, usa a API de filtro do Tekla
        var prefixUniqueNumbers = new Dictionary<string, SortedSet<int>>(StringComparer.OrdinalIgnoreCase);
        var prefixTotalCount   = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var emptyPositionIds   = new List<long>();
        int totalAssemblies    = 0;

        ModelObjectSelector modelSelector = model.GetModelObjectSelector();

        var previousCursor = Cursor.Current;
        Cursor.Current = Cursors.WaitCursor;
        try
        {
            foreach (string prefix in prefixList)
            {
                BinaryFilterExpressionCollection filter = BuildPrefixStartsWithFilter(prefix);
                if (filter == null) continue;

                ModelObjectEnumerator enumFiltered = modelSelector.GetObjectsByFilter(filter);
                if (enumFiltered == null) continue;

                while (enumFiltered.MoveNext())
                {
                    Assembly assembly = enumFiltered.Current as Assembly;
                    if (assembly == null) continue;

                    long id = assembly.Identifier != null ? assembly.Identifier.ID : 0;
                    string position = ModelHelper.GetReportProperty(assembly, "ASSEMBLY_POS");

                    if (string.IsNullOrWhiteSpace(position) || position == "-")
                    {
                        emptyPositionIds.Add(id);
                        continue;
                    }

                    string foundPrefix;
                    int number;
                    if (!DecomposePosition(position, out foundPrefix, out number)) continue;

                    // O filtro STARTS_WITH pode retornar "PPP1" quando busca "PP" - checar prefixo exato
                    if (!string.Equals(foundPrefix, prefix, StringComparison.OrdinalIgnoreCase)) continue;

                    totalAssemblies++;

                    int current;
                    prefixTotalCount.TryGetValue(prefix, out current);
                    prefixTotalCount[prefix] = current + 1;

                    SortedSet<int> uniqueNums;
                    if (!prefixUniqueNumbers.TryGetValue(prefix, out uniqueNums))
                    {
                        uniqueNums = new SortedSet<int>();
                        prefixUniqueNumbers[prefix] = uniqueNums;
                    }
                    uniqueNums.Add(number);
                }
            }

            // Busca separada de assemblies sem posicao (modelo inteiro - apenas uma vez)
            BinaryFilterExpressionCollection emptyFilter = BuildEmptyPositionFilter();
            if (emptyFilter != null)
            {
                ModelObjectEnumerator enumEmpty = modelSelector.GetObjectsByFilter(emptyFilter);
                if (enumEmpty != null)
                {
                    while (enumEmpty.MoveNext())
                    {
                        Assembly assembly = enumEmpty.Current as Assembly;
                        if (assembly == null) continue;
                        long id = assembly.Identifier != null ? assembly.Identifier.ID : 0;
                        emptyPositionIds.Add(id);
                    }
                }
            }
        }
        finally
        {
            Cursor.Current = previousCursor;
        }

        if (totalAssemblies == 0 && emptyPositionIds.Count == 0)
        {
            MessageBox.Show(string.Format("Nenhum conjunto encontrado para os prefixos: {0}", prefixDisplay));
            return null;
        }

        return FormatReport(prefixDisplay, totalAssemblies, emptyPositionIds, prefixUniqueNumbers, prefixTotalCount, prefixList);
    }

    // Filtro: ASSEMBLY cujo PositionNumber comeca com o prefixo dado
    private static BinaryFilterExpressionCollection BuildPrefixStartsWithFilter(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix)) return null;

        var collection = new BinaryFilterExpressionCollection();

        var typeFilter = new BinaryFilterExpression(
            new ObjectFilterExpressions.Type(),
            NumericOperatorType.IS_EQUAL,
            new NumericConstantFilterExpression((int)TeklaStructuresDatabaseTypeEnum.ASSEMBLY));
        collection.Add(new BinaryFilterExpressionItem(typeFilter, BinaryFilterOperatorType.BOOLEAN_AND));

        var posFilter = new BinaryFilterExpression(
            new AssemblyFilterExpressions.PositionNumber(),
            StringOperatorType.STARTS_WITH,
            new StringConstantFilterExpression(prefix));
        collection.Add(new BinaryFilterExpressionItem(posFilter, BinaryFilterOperatorType.BOOLEAN_AND));

        return collection;
    }

    // Filtro: ASSEMBLY cuja PositionNumber esta vazia
    private static BinaryFilterExpressionCollection BuildEmptyPositionFilter()
    {
        var collection = new BinaryFilterExpressionCollection();

        var typeFilter = new BinaryFilterExpression(
            new ObjectFilterExpressions.Type(),
            NumericOperatorType.IS_EQUAL,
            new NumericConstantFilterExpression((int)TeklaStructuresDatabaseTypeEnum.ASSEMBLY));
        collection.Add(new BinaryFilterExpressionItem(typeFilter, BinaryFilterOperatorType.BOOLEAN_AND));

        var emptyFilter = new BinaryFilterExpression(
            new AssemblyFilterExpressions.PositionNumber(),
            StringOperatorType.IS_EQUAL,
            new StringConstantFilterExpression(string.Empty));
        collection.Add(new BinaryFilterExpressionItem(emptyFilter, BinaryFilterOperatorType.BOOLEAN_AND));

        return collection;
    }

    private static string FormatReport(
        string prefixDisplay,
        int totalAssemblies,
        List<long> emptyIds,
        Dictionary<string, SortedSet<int>> prefixUniqueNumbers,
        Dictionary<string, int> prefixTotalCount,
        List<string> sortedPrefixes)
    {
        StringBuilder sb = new StringBuilder();
        int gapCount  = 0;
        int errorCount = 0;

        sb.AppendLine("=== VERIFICADOR DE NUMERACAO ===");
        sb.AppendLine(string.Format("Data: {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm")));
        sb.AppendLine(string.Format("Prefixos verificados: {0}", prefixDisplay));
        sb.AppendLine(string.Format("Total de conjuntos encontrados: {0}", totalAssemblies));
        sb.AppendLine();

        // 1. Sem posicao (modelo inteiro)
        sb.AppendLine("--- Conjuntos sem posicao ---");
        if (emptyIds.Count == 0)
        {
            sb.AppendLine("[OK] Nenhum conjunto sem posicao encontrado.");
        }
        else
        {
            sb.AppendLine(string.Format("[X] {0} conjunto(s) sem posicao de conjunto:", emptyIds.Count));
            int showMax = Math.Min(emptyIds.Count, 15);
            for (int i = 0; i < showMax; i++)
            {
                sb.AppendLine(string.Format("    - ID: {0}", emptyIds[i]));
            }
            if (emptyIds.Count > showMax)
            {
                sb.AppendLine(string.Format("    ... e mais {0} conjuntos.", emptyIds.Count - showMax));
            }
            errorCount++;
        }
        sb.AppendLine();

        // 2. Gaps por prefixo
        sb.AppendLine("--- Gaps na sequencia de numeracao ---");
        bool anyGap = false;

        foreach (string prefix in sortedPrefixes)
        {
            SortedSet<int> uniqueNums;
            if (!prefixUniqueNumbers.TryGetValue(prefix, out uniqueNums) || uniqueNums.Count == 0)
            {
                sb.AppendLine(string.Format("[~] Serie \"{0}\": nenhum conjunto encontrado com esse prefixo.", prefix));
                continue;
            }

            if (uniqueNums.Count == 1)
            {
                int only = 0;
                foreach (int n in uniqueNums) { only = n; }
                sb.AppendLine(string.Format("[OK] Serie \"{0}\": apenas 1 posicao ({0}{1}). Sem gaps.", prefix, only));
                continue;
            }

            int min = 0, max = 0;
            bool first = true;
            foreach (int n in uniqueNums)
            {
                if (first) { min = n; first = false; }
                max = n;
            }

            var gaps = new List<int>();
            for (int n = min + 1; n < max; n++)
            {
                if (!uniqueNums.Contains(n))
                {
                    gaps.Add(n);
                    if (gaps.Count >= 50) break;
                }
            }

            if (gaps.Count > 0)
            {
                anyGap = true;
                gapCount += gaps.Count;
                sb.AppendLine(string.Format("[X] Serie \"{0}\": {1} posicao(oes) faltando (de {0}{2} a {0}{3}):",
                    prefix, gaps.Count, min, max));
                sb.AppendLine(string.Format("    Faltam: {0}", JoinInts(gaps, 20)));
            }
            else
            {
                sb.AppendLine(string.Format("[OK] Serie \"{0}\": sem gaps ({0}{1} a {0}{2}).", prefix, min, max));
            }
        }
        sb.AppendLine();

        // 3. Resumo das series
        sb.AppendLine("--- Resumo das series ---");
        foreach (string prefix in sortedPrefixes)
        {
            SortedSet<int> uniqueNums;
            int total;
            prefixUniqueNumbers.TryGetValue(prefix, out uniqueNums);
            prefixTotalCount.TryGetValue(prefix, out total);
            if (uniqueNums == null || uniqueNums.Count == 0) continue;

            int min = 0, max = 0;
            bool first = true;
            foreach (int n in uniqueNums)
            {
                if (first) { min = n; first = false; }
                max = n;
            }

            sb.AppendLine(string.Format("[OK] {0}: {1} posicoes unicas | {2} conjuntos no total | intervalo {0}{3} a {0}{4}",
                prefix, uniqueNums.Count, total, min, max));
        }
        sb.AppendLine();

        // 4. Veredicto
        sb.AppendLine("=== RESUMO ===");
        if (errorCount == 0 && gapCount == 0)
        {
            sb.AppendLine("[OK] Nenhum problema encontrado.");
            sb.AppendLine();
            sb.AppendLine(">>> APROVADO - Numeracao consistente. <<<");
        }
        else
        {
            if (errorCount > 0)
                sb.AppendLine(string.Format("[X] Conjuntos sem posicao: {0}", emptyIds.Count));
            if (gapCount > 0)
                sb.AppendLine(string.Format("[X] Posicoes faltando na sequencia: {0}", gapCount));
            sb.AppendLine();
            sb.AppendLine(string.Format(">>> REPROVADO - {0} problema(s) encontrado(s). <<<",
                errorCount + (gapCount > 0 ? 1 : 0)));
        }

        return sb.ToString();
    }

    private static bool DecomposePosition(string position, out string prefix, out int number)
    {
        prefix = null;
        number = 0;
        if (string.IsNullOrWhiteSpace(position)) return false;

        int splitIndex = -1;
        for (int i = position.Length - 1; i >= 0; i--)
        {
            if (!char.IsDigit(position[i]))
            {
                splitIndex = i;
                break;
            }
        }

        if (splitIndex < 0 || splitIndex >= position.Length - 1) return false;

        prefix  = position.Substring(0, splitIndex + 1).Trim();
        string numPart = position.Substring(splitIndex + 1);
        if (string.IsNullOrWhiteSpace(prefix)) return false;

        return int.TryParse(numPart, out number);
    }

    private static string JoinInts(List<int> values, int max)
    {
        StringBuilder sb = new StringBuilder();
        int count = Math.Min(values.Count, max);
        for (int i = 0; i < count; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(values[i].ToString());
        }
        if (values.Count > max)
            sb.Append(string.Format(" ... (+{0} outros)", values.Count - max));
        return sb.ToString();
    }
}
