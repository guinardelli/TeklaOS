internal static class AssemblyComparator
{
    public static void CompareSelectedAssemblies()
    {
        Model model = new Model();
        if (!model.GetConnectionStatus())
        {
            MessageBox.Show("Nao foi possivel conectar ao modelo Tekla. Abra um modelo e tente novamente.");
            return;
        }

        ModelUI.ModelObjectSelector uiSelector = new ModelUI.ModelObjectSelector();
        ModelObjectEnumerator enumSelected = uiSelector.GetSelectedObjects();
        if (enumSelected == null)
        {
            MessageBox.Show("Nenhum objeto selecionado.");
            return;
        }

        ArrayList assemblies = new ArrayList();
        while (enumSelected.MoveNext())
        {
            Assembly ass = enumSelected.Current as Assembly;
            if (ass != null)
            {
                assemblies.Add(ass);
            }
        }

        if (assemblies.Count != 2)
        {
            MessageBox.Show("Selecione exatamente dois conjuntos para comparar.");
            return;
        }

        Assembly ass1 = (Assembly)assemblies[0];
        Assembly ass2 = (Assembly)assemblies[1];

        StringBuilder sb = new StringBuilder();
        AppendAssemblyComparison(sb, ass1, ass2);

        ReportWindow.ShowReport(sb.ToString());
    }

    private static void AppendAssemblyComparison(StringBuilder sb, Assembly ass1, Assembly ass2)
    {
        sb.AppendLine("Comparacao de Conjuntos por POSICAO");
        sb.AppendLine("Conjunto 1: " + Formatters.FormatValue(ass1.Name));
        sb.AppendLine("Conjunto 2: " + Formatters.FormatValue(ass2.Name));
        sb.AppendLine();

        ArrayList parts1 = GetAssemblyParts(ass1);
        ArrayList parts2 = GetAssemblyParts(ass2);

        Dictionary<string, ArrayList> map1 = GroupPartsByPosition(parts1);
        Dictionary<string, ArrayList> map2 = GroupPartsByPosition(parts2);

        SortedSet<string> positions = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string pos in map1.Keys)
        {
            positions.Add(pos);
        }
        foreach (string pos in map2.Keys)
        {
            positions.Add(pos);
        }

        int only1 = 0;
        int only2 = 0;
        int diff = 0;
        int equal = 0;

        StringBuilder details = new StringBuilder();
        foreach (string pos in positions)
        {
            ArrayList list1;
            ArrayList list2;
            bool has1 = map1.TryGetValue(pos, out list1);
            bool has2 = map2.TryGetValue(pos, out list2);

            if (!has1)
            {
                only2++;
                diff++;
                details.AppendLine(string.Format("{0} | Somente conjunto 2 | - | - | - | - | - | - | - | -", pos));
                continue;
            }

            if (!has2)
            {
                only1++;
                diff++;
                details.AppendLine(string.Format("{0} | Somente conjunto 1 | - | - | - | - | - | - | - | -", pos));
                continue;
            }

            int count1 = list1.Count;
            int count2 = list2.Count;
            bool countMismatch = count1 != count2;

            Part part1 = PickRepresentative(list1);
            Part part2 = PickRepresentative(list2);

            string name1 = GetReportProperty(part1, "NAME");
            string name2 = GetReportProperty(part2, "NAME");
            string profile1 = GetReportProperty(part1, "PROFILE");
            string profile2 = GetReportProperty(part2, "PROFILE");
            string material1 = GetReportProperty(part1, "MATERIAL");
            string material2 = GetReportProperty(part2, "MATERIAL");
            string length1 = GetReportProperty(part1, "LENGTH");
            string length2 = GetReportProperty(part2, "LENGTH");

            bool propDiff = !AreEqualNormalized(name1, name2)
                || !AreEqualNormalized(profile1, profile2)
                || !AreEqualNormalized(material1, material2)
                || !AreEqualNormalized(length1, length2);

            if (countMismatch || propDiff)
            {
                diff++;
                string status = countMismatch
                    ? string.Format("Qtd difere (C1={0}, C2={1})", count1, count2)
                    : "Diferente";
                details.AppendLine(string.Format(
                    "{0} | {1} | {2} | {3} | {4} | {5} | {6} | {7} | {8} | {9}",
                    pos,
                    status,
                    name1,
                    name2,
                    profile1,
                    profile2,
                    material1,
                    material2,
                    length1,
                    length2));
            }
            else
            {
                equal++;
            }
        }

        sb.AppendLine("Resumo");
        sb.AppendLine(string.Format("Posicoes no conjunto 1: {0}", map1.Count));
        sb.AppendLine(string.Format("Posicoes no conjunto 2: {0}", map2.Count));
        sb.AppendLine(string.Format("Posicoes iguais: {0}", equal));
        sb.AppendLine(string.Format("Posicoes com diferencas: {0}", diff));
        sb.AppendLine(string.Format("Somente no conjunto 1: {0}", only1));
        sb.AppendLine(string.Format("Somente no conjunto 2: {0}", only2));
        sb.AppendLine();

        sb.AppendLine("Diferencas por posicao");
        sb.AppendLine("Posicao | Status | Nome C1 | Nome C2 | Perfil C1 | Perfil C2 | Material C1 | Material C2 | Comprimento C1 | Comprimento C2");
        sb.AppendLine("----------------------------------------------------------------------------------------------------------------------------");

        if (details.Length == 0)
        {
            sb.AppendLine("Nenhuma diferenca encontrada.");
        }
        else
        {
            sb.Append(details.ToString());
        }

        sb.AppendLine();
    }

    private static ArrayList GetAssemblyParts(Assembly ass)
    {
        ArrayList parts = new ArrayList();
        Part mainPart = ass.GetMainPart() as Part;
        if (mainPart != null)
        {
            parts.Add(mainPart);
        }

        ArrayList secondaryParts = ass.GetSecondaries();
        foreach (ModelObject secondary in secondaryParts)
        {
            Part part = secondary as Part;
            if (part != null)
            {
                parts.Add(part);
            }
        }

        return parts;
    }

    private static Dictionary<string, ArrayList> GroupPartsByPosition(ArrayList parts)
    {
        Dictionary<string, ArrayList> map = new Dictionary<string, ArrayList>(StringComparer.OrdinalIgnoreCase);
        foreach (Part part in parts)
        {
            if (part == null)
            {
                continue;
            }

            string key = GetPositionKey(part);
            ArrayList list;
            if (!map.TryGetValue(key, out list))
            {
                list = new ArrayList();
                map.Add(key, list);
            }
            list.Add(part);
        }
        return map;
    }

    private static string GetPositionKey(Part part)
    {
        string position = GetReportProperty(part, "POSITION");
        if (position == null)
        {
            position = string.Empty;
        }

        position = position.Trim();
        if (position.Length == 0 || position == "-")
        {
            position = string.Format("SEM_POSICAO_{0}", part.Identifier.ID);
        }

        return position;
    }

    private static Part PickRepresentative(ArrayList parts)
    {
        Part selected = null;
        long minId = long.MaxValue;
        foreach (Part part in parts)
        {
            if (part == null)
            {
                continue;
            }

            long id = part.Identifier.ID;
            if (selected == null || id < minId)
            {
                selected = part;
                minId = id;
            }
        }

        if (selected == null && parts.Count > 0)
        {
            selected = parts[0] as Part;
        }

        return selected;
    }

    private static bool AreEqualNormalized(string left, string right)
    {
        if (left == null)
        {
            left = string.Empty;
        }
        if (right == null)
        {
            right = string.Empty;
        }

        return string.Equals(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static string GetReportProperty(Part part, string propertyName)
    {
        string stringValue = null;
        if (part.GetReportProperty(propertyName, ref stringValue))
        {
            return Formatters.FormatValue(stringValue);
        }

        double doubleValue = 0.0;
        if (part.GetReportProperty(propertyName, ref doubleValue))
        {
            return Formatters.FormatValue(string.Format("{0:F1}", doubleValue));
        }

        return "-";
    }
}
