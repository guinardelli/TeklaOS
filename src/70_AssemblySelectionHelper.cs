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

        var enumerator = model.GetModelObjectSelector().GetAllObjectsWithType(ModelObject.ModelObjectEnum.ASSEMBLY);
        if (enumerator == null)
        {
            MessageBox.Show("Nao foi possivel obter os conjuntos do modelo.");
            return;
        }

        var selection = new System.Collections.ArrayList();
        var foundNames = new System.Collections.Generic.HashSet<string>(Comparer);

        while (enumerator.MoveNext())
        {
            var assembly = enumerator.Current as Assembly;
            if (assembly == null)
            {
                continue;
            }

            foreach (var candidate in GetNormalizedAssemblyKeys(assembly))
            {
                if (!requestedMapping.ContainsKey(candidate))
                {
                    continue;
                }

                if (!selection.Contains(assembly))
                {
                    selection.Add(assembly);
                }

                foundNames.Add(candidate);
                break;
            }

            if (foundNames.Count == requestedMapping.Count)
            {
                break;
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
        var values = new System.Collections.Generic.List<string>();

        foreach (var propertyName in new[] { "POSITION", "ASSEMBLY_POSITION", "ASSEMBLY_POS", "POSITION_NAME", "ASSEMBLY_NUMBER", "POSITION_ID" })
        {
            var reportValue = TryGetReportProperty(assembly, propertyName);
            if (!string.IsNullOrWhiteSpace(reportValue))
            {
                values.Add(reportValue);
            }
        }

        try
        {
            var names = new System.Collections.ArrayList();
            var types = new System.Collections.ArrayList();
            var rawValues = new System.Collections.ArrayList();
            var props = new System.Collections.Hashtable();

            if (assembly.GetAllReportProperties(names, types, rawValues, ref props))
            {
                foreach (System.Collections.DictionaryEntry entry in props)
                {
                    string key = entry.Key as string;
                    if (key == null)
                    {
                        continue;
                    }

                    if (key.IndexOf("POSITION", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        key.IndexOf("ASSEMBLY_POS", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (entry.Value != null)
                        {
                            var text = entry.Value.ToString();
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                values.Add(text);
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // Ignore missing report properties.
        }

        return values;
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
}
