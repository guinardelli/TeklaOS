internal static class AssemblySelectionHelper
{
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

    public static void SelectAssemblies(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            MessageBox.Show("Informe ao menos um nome de conjunto.");
            return;
        }

        var model = new Model();
        if (!model.GetConnectionStatus())
        {
            MessageBox.Show("Nao foi possivel conectar ao modelo. Abra um modelo e tente novamente.");
            return;
        }

        var tokens = input.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        var requestedMapping = new System.Collections.Generic.Dictionary<string, string>(Comparer);
        var requestedOrder = new System.Collections.Generic.List<string>();

        foreach (var token in tokens)
        {
            var trimmed = token.Trim();
            var normalized = NormalizeKey(trimmed);
            if (normalized == null || requestedMapping.ContainsKey(normalized))
            {
                continue;
            }

            requestedMapping[normalized] = trimmed;
            requestedOrder.Add(normalized);
        }

        if (requestedMapping.Count == 0)
        {
            MessageBox.Show("Informe ao menos um nome de conjunto.");
            return;
        }

        var selection = new System.Collections.ArrayList();
        var selectionIds = new System.Collections.Generic.HashSet<long>();
        var foundNames = new System.Collections.Generic.HashSet<string>(Comparer);

        TrySelectAssembliesByFilters(model, requestedOrder, selection, selectionIds, foundNames);

        var missingNormalized = new System.Collections.Generic.HashSet<string>(Comparer);
        foreach (var normalized in requestedMapping.Keys)
        {
            if (!foundNames.Contains(normalized))
            {
                missingNormalized.Add(normalized);
            }
        }

        if (missingNormalized.Count > 0)
        {
            var enumerator = model.GetModelObjectSelector().GetAllObjectsWithType(ModelObject.ModelObjectEnum.ASSEMBLY);
            if (enumerator == null)
            {
                MessageBox.Show("Nao foi possivel obter os conjuntos do modelo.");
                return;
            }

            var previousCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                int processed = 0;
                while (enumerator.MoveNext())
                {
                    processed++;
                    if (processed % 200 == 0)
                    {
                        Application.DoEvents();
                    }

                    var assembly = enumerator.Current as Assembly;
                    if (assembly == null)
                    {
                        continue;
                    }

                    foreach (var candidate in GetNormalizedAssemblyKeys(assembly))
                    {
                        if (!missingNormalized.Contains(candidate))
                        {
                            continue;
                        }

                        AddAssemblyToSelection(selection, selectionIds, assembly);
                        foundNames.Add(candidate);
                        missingNormalized.Remove(candidate);
                        break;
                    }

                    if (missingNormalized.Count == 0)
                    {
                        break;
                    }
                }
            }
            finally
            {
                Cursor.Current = previousCursor;
            }
        }

        if (selection.Count == 0)
        {
            MessageBox.Show("Nenhum conjunto encontrado com os nomes informados.");
            return;
        }

        var selector = new ModelUI.ModelObjectSelector();
        selector.Select(selection, false);

        if (foundNames.Count < requestedMapping.Count)
        {
            var missingBuilder = new StringBuilder();
            foreach (var normalized in requestedOrder)
            {
                if (foundNames.Contains(normalized))
                {
                    continue;
                }

                if (missingBuilder.Length > 0)
                {
                    missingBuilder.Append(", ");
                }

                missingBuilder.Append(requestedMapping[normalized]);
            }

            MessageBox.Show("Nao foram encontrados os seguintes conjuntos: " + missingBuilder.ToString());
        }
    }

    private static System.Collections.Generic.IEnumerable<string> GetNormalizedAssemblyKeys(Assembly assembly)
    {
        var seen = new System.Collections.Generic.HashSet<string>(Comparer);

        foreach (var candidate in GetAssemblyCandidateValues(assembly))
        {
            var normalized = NormalizeKey(candidate);
            if (normalized != null && seen.Add(normalized))
            {
                yield return normalized;
            }
        }
    }

    private static System.Collections.Generic.IEnumerable<string> GetAssemblyCandidateValues(Assembly assembly)
    {
        if (!string.IsNullOrWhiteSpace(assembly.Name))
        {
            yield return assembly.Name;
        }

        var number = assembly.AssemblyNumber;
        if (number != null)
        {
            var numberString = number.ToString();
            if (!string.IsNullOrWhiteSpace(numberString))
            {
                yield return numberString;
            }
        }

        var identifier = assembly.Identifier;
        if (identifier != null)
        {
            yield return identifier.ID.ToString();
        }

        foreach (var reportValue in TryGetReportValues(assembly))
        {
            yield return reportValue;
        }
    }

    private static System.Collections.Generic.IEnumerable<string> TryGetReportValues(Assembly assembly)
    {
        foreach (var propertyName in new[] { "POSITION", "ASSEMBLY_POSITION", "ASSEMBLY_POS", "POSITION_NAME", "ASSEMBLY_NUMBER", "POSITION_ID" })
        {
            var reportValue = TryGetReportProperty(assembly, propertyName);
            if (!string.IsNullOrWhiteSpace(reportValue))
            {
                yield return reportValue;
            }
        }
    }

    private static string TryGetReportProperty(Assembly assembly, string propertyName)
    {
        try
        {
            string stringValue = null;
            if (assembly.GetReportProperty(propertyName, ref stringValue) && !string.IsNullOrWhiteSpace(stringValue))
            {
                return stringValue;
            }

            double doubleValue = 0;
            if (assembly.GetReportProperty(propertyName, ref doubleValue))
            {
                return doubleValue.ToString();
            }

            int intValue = 0;
            if (assembly.GetReportProperty(propertyName, ref intValue))
            {
                return intValue.ToString();
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static string NormalizeKey(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var trimmed = input.Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }

    private static readonly Func<string, BinaryFilterExpressionCollection>[] AssemblyFilterFactories =
        new Func<string, BinaryFilterExpressionCollection>[]
        {
            value => BuildAssemblyStringFilter(() => new AssemblyFilterExpressions.Name(), value),
            value => BuildAssemblyStringFilter(() => new AssemblyFilterExpressions.PositionNumber(), value)
        };

    private static void TrySelectAssembliesByFilters(Model model, System.Collections.Generic.IEnumerable<string> normalizedKeys, System.Collections.ArrayList selection, System.Collections.Generic.HashSet<long> selectionIds, System.Collections.Generic.HashSet<string> foundNames)
    {
        if (model == null || normalizedKeys == null)
        {
            return;
        }

        var selector = model.GetModelObjectSelector();
        if (selector == null)
        {
            return;
        }

        foreach (var normalized in normalizedKeys)
        {
            if (string.IsNullOrWhiteSpace(normalized) || foundNames.Contains(normalized))
            {
                continue;
            }

            if (TryGatherAssembliesByFilter(selector, normalized, selection, selectionIds))
            {
                foundNames.Add(normalized);
            }
        }
    }

    private static bool TryGatherAssembliesByFilter(ModelObjectSelector selector, string normalized, System.Collections.ArrayList selection, System.Collections.Generic.HashSet<long> selectionIds)
    {
        foreach (var factory in AssemblyFilterFactories)
        {
            var filter = factory(normalized);
            if (filter == null)
            {
                continue;
            }

            var enumerator = selector.GetObjectsByFilter(filter);
            if (enumerator == null)
            {
                continue;
            }

            var found = false;
            while (enumerator.MoveNext())
            {
                var assembly = enumerator.Current as Assembly;
                if (assembly == null)
                {
                    continue;
                }

                AddAssemblyToSelection(selection, selectionIds, assembly);
                found = true;
            }

            if (found)
            {
                return true;
            }
        }

        return false;
    }

    private static void AddAssemblyToSelection(System.Collections.ArrayList selection, System.Collections.Generic.HashSet<long> selectionIds, Assembly assembly)
    {
        if (assembly == null)
        {
            return;
        }

        var identifier = assembly.Identifier;
        if (identifier != null && identifier.ID != 0)
        {
            if (selectionIds.Add(identifier.ID))
            {
                selection.Add(assembly);
            }

            return;
        }

        if (!selection.Contains(assembly))
        {
            selection.Add(assembly);
        }
    }

    private static BinaryFilterExpressionCollection BuildAssemblyStringFilter(Func<StringFilterExpression> propertyFactory, string normalized)
    {
        if (propertyFactory == null || string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        var propertyExpression = propertyFactory();
        if (propertyExpression == null)
        {
            return null;
        }

        var collection = new BinaryFilterExpressionCollection();

        var typeFilter = new BinaryFilterExpression(
            new ObjectFilterExpressions.Type(),
            NumericOperatorType.IS_EQUAL,
            new NumericConstantFilterExpression((int)TeklaStructuresDatabaseTypeEnum.ASSEMBLY));
        collection.Add(new BinaryFilterExpressionItem(typeFilter, BinaryFilterOperatorType.BOOLEAN_AND));

        var stringFilter = new BinaryFilterExpression(
            propertyExpression,
            StringOperatorType.IS_EQUAL,
            new StringConstantFilterExpression(normalized));
        collection.Add(new BinaryFilterExpressionItem(stringFilter, BinaryFilterOperatorType.BOOLEAN_AND));

        return collection;
    }
}
