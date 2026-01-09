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
                Part onlyPart2 = PickRepresentative(list2);
                string conjuntoRight = GetConjuntoLabel(GetPartPropertyOrDash(onlyPart2, "CAST_UNIT_POS"), pos);
                details.AppendLine(string.Format("[X] Conjunto: - | {0}", conjuntoRight));
                details.AppendLine("- Status: Somente conjunto 2");
                AppendCompareLine(details, "Quantidade", 0, list2.Count);
                AppendCompareLine(details, "Nome", "-", GetPartPropertyOrDash(onlyPart2, "NAME"));
                AppendCompareLine(details, "Perfil", "-", GetPartPropertyOrDash(onlyPart2, "PROFILE"));
                AppendCompareLine(details, "Material", "-", GetPartPropertyOrDash(onlyPart2, "MATERIAL"));
                details.AppendLine();
                continue;
            }

            if (!has2)
            {
                only1++;
                diff++;
                Part onlyPart1 = PickRepresentative(list1);
                string conjuntoLeft = GetConjuntoLabel(GetPartPropertyOrDash(onlyPart1, "CAST_UNIT_POS"), pos);
                details.AppendLine(string.Format("[X] Conjunto: {0} | -", conjuntoLeft));
                details.AppendLine("- Status: Somente conjunto 1");
                AppendCompareLine(details, "Quantidade", list1.Count, 0);
                AppendCompareLine(details, "Nome", GetPartPropertyOrDash(onlyPart1, "NAME"), "-");
                AppendCompareLine(details, "Perfil", GetPartPropertyOrDash(onlyPart1, "PROFILE"), "-");
                AppendCompareLine(details, "Material", GetPartPropertyOrDash(onlyPart1, "MATERIAL"), "-");
                details.AppendLine();
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
            string castUnit1 = GetReportProperty(part1, "CAST_UNIT_POS");
            string castUnit2 = GetReportProperty(part2, "CAST_UNIT_POS");

            bool propDiff = !AreEqualNormalized(name1, name2)
                || !AreEqualNormalized(profile1, profile2)
                || !AreEqualNormalized(material1, material2)
                || !AreEqualNormalized(castUnit1, castUnit2);

            if (countMismatch || propDiff)
            {
                diff++;
                string status = countMismatch ? "Quantidade diferente" : "Diferente";
                string conjuntoLeft = GetConjuntoLabel(castUnit1, pos);
                string conjuntoRight = GetConjuntoLabel(castUnit2, pos);
                string conjuntoMark = AreEqualNormalized(conjuntoLeft, conjuntoRight) ? "[OK]" : "[X]";
                details.AppendLine(string.Format("{0} Conjunto: {1} | {2}", conjuntoMark, conjuntoLeft, conjuntoRight));
                details.AppendLine("- Status: " + status);
                AppendCompareLine(details, "Quantidade", count1, count2);
                AppendCompareLine(details, "Nome", name1, name2);
                AppendCompareLine(details, "Perfil", profile1, profile2);
                AppendCompareLine(details, "Material", material1, material2);
                details.AppendLine();
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

        sb.AppendLine("PROPRIEDADES DO CONJUNTO:");
        AppendAssemblyPropertiesComparison(sb, ass1, ass2);
        sb.AppendLine();

        sb.AppendLine("RESULTADO COMPARADOR:");

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
        if (string.IsNullOrWhiteSpace(position) || position == "-")
        {
            string name = NormalizeKeyValue(GetReportProperty(part, "NAME"));
            string profile = NormalizeKeyValue(GetReportProperty(part, "PROFILE"));
            string material = NormalizeKeyValue(GetReportProperty(part, "MATERIAL"));
            return string.Format("SEM_POSICAO_{0}_{1}_{2}", name, profile, material);
        }

        return position.Trim();
    }

    private static string NormalizeKeyValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "-")
        {
            return "N/A";
        }

        return value.Trim().ToUpperInvariant().Replace(" ", "_");
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

    private static void AppendCompareLine(StringBuilder sb, string label, string leftValue, string rightValue)
    {
        string mark = AreEqualNormalized(leftValue, rightValue) ? "[OK]" : "[X]";
        sb.AppendLine(string.Format("{0} {1}: {2} | {3}", mark, label, leftValue, rightValue));
    }

    private static void AppendCompareLine(StringBuilder sb, string label, int leftCount, int rightCount)
    {
        string leftText = FormatQuantity(leftCount);
        string rightText = FormatQuantity(rightCount);
        string mark = leftCount == rightCount ? "[OK]" : "[X]";
        sb.AppendLine(string.Format("{0} {1}: {2} | {3}", mark, label, leftText, rightText));
    }

    private static string FormatQuantity(int count)
    {
        if (count == 1)
        {
            return "1 peca";
        }

        return string.Format("{0} pecas", count);
    }

    private static void AppendAssemblyPropertiesComparison(StringBuilder sb, Assembly ass1, Assembly ass2)
    {
        AppendAssemblyPropertyLine(sb, ass1, ass2, "AREA");
        AppendAssemblyPropertyLine(sb, ass1, ass2, "ASSEMBLY_PREFIX");
        AppendAssemblyPropertyLine(sb, ass1, ass2, "WIDTH");
        AppendAssemblyPropertyLine(sb, ass1, ass2, "HEIGHT");
        AppendAssemblyPropertyLine(sb, ass1, ass2, "LENGHT");
        AppendAssemblyPropertyLine(sb, ass1, ass2, "LENGHT_GROSS");
        AppendAssemblyPropertyLine(sb, ass1, ass2, "MATERIAL_TYPE");
        AppendAssemblyPropertyLine(sb, ass1, ass2, "VOLUME");
        AppendAssemblyPropertyLine(sb, ass1, ass2, "WEIGHT");
        AppendAssemblyPropertyLine(sb, ass1, ass2, "WEIGHT_GROSS");
        AppendAssemblyPropertyLine(sb, ass1, ass2, "WEIGHT_NET");
    }

    private static void AppendAssemblyPropertyLine(StringBuilder sb, Assembly ass1, Assembly ass2, string propertyName)
    {
        string leftValue = GetAssemblyProperty(ass1, propertyName);
        string rightValue = GetAssemblyProperty(ass2, propertyName);
        AppendCompareLine(sb, propertyName, leftValue, rightValue);
    }

    private static string GetAssemblyProperty(Assembly ass, string propertyName)
    {
        string value = GetReportProperty(ass, propertyName);
        if (value == "-")
        {
            if (string.Equals(propertyName, "LENGHT", StringComparison.OrdinalIgnoreCase))
            {
                value = GetReportProperty(ass, "LENGTH");
            }
            else if (string.Equals(propertyName, "LENGHT_GROSS", StringComparison.OrdinalIgnoreCase))
            {
                value = GetReportProperty(ass, "LENGTH_GROSS");
            }
        }

        return value;
    }

    private static string GetConjuntoLabel(string castUnitPos, string positionKey)
    {
        if (castUnitPos == "-" && !string.IsNullOrWhiteSpace(positionKey))
        {
            if (!positionKey.StartsWith("SEM_POSICAO_", StringComparison.OrdinalIgnoreCase))
            {
                return positionKey.Trim();
            }
        }

        return castUnitPos;
    }

    private static string GetPartPropertyOrDash(Part part, string propertyName)
    {
        if (part == null)
        {
            return "-";
        }

        return GetReportProperty(part, propertyName);
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

    private static string GetReportProperty(ModelObject obj, string propertyName)
    {
        if (obj == null)
        {
            return "-";
        }

        string stringValue = null;
        if (obj.GetReportProperty(propertyName, ref stringValue))
        {
            return Formatters.FormatValue(stringValue);
        }

        double doubleValue = 0.0;
        if (obj.GetReportProperty(propertyName, ref doubleValue))
        {
            return Formatters.FormatValue(string.Format("{0:F1}", doubleValue));
        }

        return "-";
    }
}
