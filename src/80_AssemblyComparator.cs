internal static class AssemblyComparator
{
    private const bool IncludeOkDetails = false;

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
        AppendAssemblyComparison(sb, ass1, ass2, model);

        ReportWindow.ShowReport(sb.ToString());
    }

    private static void AppendAssemblyComparison(StringBuilder sb, Assembly ass1, Assembly ass2, Model model)
    {
        sb.AppendLine("RESULTADO COMPARADOR (NUMERACAO)");
        sb.AppendLine("Conjunto 1: " + Formatters.FormatValue(ass1.Name));
        sb.AppendLine("Conjunto 2: " + Formatters.FormatValue(ass2.Name));
        sb.AppendLine();

        sb.AppendLine("SERIE DE NUMERACAO:");
        bool seriesEqual = AppendNumberingSeriesComparison(sb, ass1, ass2);
        sb.AppendLine();

        if (!seriesEqual)
        {
            sb.AppendLine("Comparacao interrompida: series diferentes.");
            sb.AppendLine();
            return;
        }

        sb.AppendLine("PECA PRINCIPAL:");
        AppendMainPartComparison(sb, ass1, ass2);
        sb.AppendLine();

        sb.AppendLine("SECUNDARIAS:");
        AppendSecondariesComparison(sb, ass1, ass2, model);
        sb.AppendLine();

        sb.AppendLine("PROPRIEDADES DO CONJUNTO:");
        AppendAssemblyPropertiesComparison(sb, ass1, ass2);
        sb.AppendLine();
    }

    private static bool AppendNumberingSeriesComparison(StringBuilder sb, Assembly ass1, Assembly ass2)
    {
        string prefix1 = GetAssemblyNumberPrefix(ass1);
        string prefix2 = GetAssemblyNumberPrefix(ass2);
        string start1 = GetAssemblyNumberStart(ass1);
        string start2 = GetAssemblyNumberStart(ass2);

        int okCount = 0;
        int diffCount = 0;

        if (AppendCompareLine(sb, "Prefixo", prefix1, prefix2))
        {
            okCount++;
        }
        else
        {
            diffCount++;
        }

        if (AppendCompareLine(sb, "StartNumber", start1, start2))
        {
            okCount++;
        }
        else
        {
            diffCount++;
        }

        AppendSectionSummary(sb, okCount, diffCount);

        return diffCount == 0;
    }

    private static void AppendMainPartComparison(StringBuilder sb, Assembly ass1, Assembly ass2)
    {
        Part main1 = ass1.GetMainPart() as Part;
        Part main2 = ass2.GetMainPart() as Part;

        if (main1 == null || main2 == null)
        {
            sb.AppendLine("[X] Peca principal: nao encontrada em um dos conjuntos.");
            return;
        }

        int okCount = 0;
        int diffCount = 0;

        if (AppendCompareLine(sb, "Perfil", GetReportProperty(main1, "PROFILE"), GetReportProperty(main2, "PROFILE")))
        {
            okCount++;
        }
        else
        {
            diffCount++;
        }

        if (AppendCompareLine(sb, "Material", GetReportProperty(main1, "MATERIAL"), GetReportProperty(main2, "MATERIAL")))
        {
            okCount++;
        }
        else
        {
            diffCount++;
        }

        if (AppendCompareLine(sb, "Acabamento", GetPartFinish(main1), GetPartFinish(main2)))
        {
            okCount++;
        }
        else
        {
            diffCount++;
        }

        if (AppendCompareLine(sb, "Deformacao", GetReportProperty(main1, "DEFORMATION"), GetReportProperty(main2, "DEFORMATION")))
        {
            okCount++;
        }
        else
        {
            diffCount++;
        }

        if (AppendCompareLine(sb, "Nome (config)", GetPartName(main1), GetPartName(main2)))
        {
            okCount++;
        }
        else
        {
            diffCount++;
        }

        if (AppendCompareLine(sb, "Classe (info)", GetPartClass(main1), GetPartClass(main2)))
        {
            okCount++;
        }
        else
        {
            diffCount++;
        }

        AppendSectionSummary(sb, okCount, diffCount);
    }

    private static void AppendSecondariesComparison(StringBuilder sb, Assembly ass1, Assembly ass2, Model model)
    {
        int count1;
        int count2;
        Dictionary<string, int> map1 = BuildSecondarySignatureCounts(ass1, model, out count1);
        Dictionary<string, int> map2 = BuildSecondarySignatureCounts(ass2, model, out count2);

        int okCount = 0;
        int diffCount = 0;

        if (AppendCompareLine(sb, "Quantidade", count1, count2))
        {
            okCount++;
        }
        else
        {
            diffCount++;
        }

        List<string> diffLines = BuildSecondaryDiffLines(map1, map2);
        bool geomOk = diffLines.Count == 0;
        if (AppendStatusLine(sb, "Geometria/posicao relativa", geomOk))
        {
            okCount++;
        }
        else
        {
            diffCount++;
        }

        if (diffLines.Count > 0)
        {
            foreach (string line in diffLines)
            {
                sb.AppendLine(line);
            }
        }

        AppendSectionSummary(sb, okCount, diffCount);
    }

    private static Dictionary<string, int> BuildSecondarySignatureCounts(Assembly ass, Model model, out int count)
    {
        Dictionary<string, int> map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        count = 0;

        if (ass == null || model == null)
        {
            return map;
        }

        Part main = ass.GetMainPart() as Part;

        WorkPlaneHandler handler = model.GetWorkPlaneHandler();
        TransformationPlane originalPlane = handler.GetCurrentTransformationPlane();

        try
        {
            if (main != null)
            {
                handler.SetCurrentTransformationPlane(new TransformationPlane(main.GetCoordinateSystem()));
            }

            ArrayList secondaries = ass.GetSecondaries();
            foreach (ModelObject obj in secondaries)
            {
                Part part = obj as Part;
                if (part == null)
                {
                    continue;
                }

                count++;
                string signature = BuildSecondarySignature(part);
                int current;
                if (map.TryGetValue(signature, out current))
                {
                    map[signature] = current + 1;
                }
                else
                {
                    map.Add(signature, 1);
                }
            }
        }
        finally
        {
            handler.SetCurrentTransformationPlane(originalPlane);
        }

        return map;
    }

    private static string BuildSecondarySignature(Part part)
    {
        string profile = NormalizeKeyValue(GetReportProperty(part, "PROFILE"));
        string material = NormalizeKeyValue(GetReportProperty(part, "MATERIAL"));
        Box box = GetLocalBoundingBox(part);

        return string.Format("Perfil={0};Material={1};Box={2}", profile, material, FormatBox(box));
    }

    private struct Box
    {
        public Tekla.Structures.Geometry3d.Point Min;
        public Tekla.Structures.Geometry3d.Point Max;

        public Box(Tekla.Structures.Geometry3d.Point min, Tekla.Structures.Geometry3d.Point max)
        {
            Min = min;
            Max = max;
        }
    }

    private static Box GetLocalBoundingBox(Part part)
    {
        Solid solid = part.GetSolid();
        Tekla.Structures.Geometry3d.Point min = solid.MinimumPoint;
        Tekla.Structures.Geometry3d.Point max = solid.MaximumPoint;
        return new Box(min, max);
    }

    private static string FormatBox(Box box)
    {
        return string.Format(
            "Min({0},{1},{2}) Max({3},{4},{5})",
            FormatCoordinate(box.Min.X),
            FormatCoordinate(box.Min.Y),
            FormatCoordinate(box.Min.Z),
            FormatCoordinate(box.Max.X),
            FormatCoordinate(box.Max.Y),
            FormatCoordinate(box.Max.Z));
    }

    private static string FormatCoordinate(double value)
    {
        double rounded = RoundToTolerance(value);
        return string.Format("{0:F3}", rounded);
    }

    private static double RoundToTolerance(double value)
    {
        double tol = GeometryConstants.DISTANCE_EPSILON;
        if (tol <= 0.0)
        {
            tol = 0.001;
        }

        return Math.Round(value / tol) * tol;
    }

    private static List<string> BuildSecondaryDiffLines(Dictionary<string, int> map1, Dictionary<string, int> map2)
    {
        List<string> lines = new List<string>();
        SortedSet<string> keys = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (string key in map1.Keys)
        {
            keys.Add(key);
        }
        foreach (string key in map2.Keys)
        {
            keys.Add(key);
        }

        foreach (string key in keys)
        {
            int count1 = map1.ContainsKey(key) ? map1[key] : 0;
            int count2 = map2.ContainsKey(key) ? map2[key] : 0;

            if (count1 > count2)
            {
                lines.Add(string.Format("[X] Secundaria faltando no conjunto 2: {0} (x{1})", key, count1 - count2));
            }
            else if (count2 > count1)
            {
                lines.Add(string.Format("[X] Secundaria extra no conjunto 2: {0} (x{1})", key, count2 - count1));
            }
        }

        return lines;
    }

    private static bool AppendStatusLine(StringBuilder sb, string label, bool ok)
    {
        if (!ok || IncludeOkDetails)
        {
            string mark = ok ? "[OK]" : "[X]";
            string status = ok ? "Iguais" : "Diferentes";
            sb.AppendLine(string.Format("{0} {1}: {2}", mark, label, status));
        }

        return ok;
    }

    private static string GetAssemblyNumberPrefix(Assembly ass)
    {
        if (ass == null || ass.AssemblyNumber == null)
        {
            return "-";
        }

        return Formatters.FormatValue(ass.AssemblyNumber.Prefix);
    }

    private static string GetAssemblyNumberStart(Assembly ass)
    {
        if (ass == null || ass.AssemblyNumber == null)
        {
            return "-";
        }

        return ass.AssemblyNumber.StartNumber.ToString();
    }

    private static string GetPartFinish(Part part)
    {
        if (part == null)
        {
            return "-";
        }

        return Formatters.FormatValue(part.Finish);
    }

    private static string GetPartName(Part part)
    {
        if (part == null)
        {
            return "-";
        }

        return Formatters.FormatValue(part.Name);
    }

    private static string GetPartClass(Part part)
    {
        if (part == null)
        {
            return "-";
        }

        return part.Class.ToString();
    }

    private static bool AppendCompareLine(StringBuilder sb, string label, string leftValue, string rightValue)
    {
        bool ok = AreEqualNormalized(leftValue, rightValue);
        if (!ok || IncludeOkDetails)
        {
            string mark = ok ? "[OK]" : "[X]";
            sb.AppendLine(string.Format("{0} {1}: {2} | {3}", mark, label, leftValue, rightValue));
        }

        return ok;
    }

    private static bool AppendCompareLine(StringBuilder sb, string label, int leftCount, int rightCount)
    {
        string leftText = FormatQuantity(leftCount);
        string rightText = FormatQuantity(rightCount);
        bool ok = leftCount == rightCount;
        if (!ok || IncludeOkDetails)
        {
            string mark = ok ? "[OK]" : "[X]";
            sb.AppendLine(string.Format("{0} {1}: {2} | {3}", mark, label, leftText, rightText));
        }

        return ok;
    }

    private static void AppendSectionSummary(StringBuilder sb, int okCount, int diffCount)
    {
        if (IncludeOkDetails)
        {
            return;
        }

        if (diffCount == 0 && okCount > 0)
        {
            sb.AppendLine(string.Format("[OK] Todos iguais ({0} linhas ocultas).", okCount));
            return;
        }

        if (okCount > 0)
        {
            sb.AppendLine(string.Format("[OK] Linhas iguais ocultas: {0}", okCount));
        }
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
        int okCount = 0;
        int diffCount = 0;

        if (AppendAssemblyPropertyLine(sb, ass1, ass2, "AREA")) okCount++; else diffCount++;
        if (AppendAssemblyPropertyLine(sb, ass1, ass2, "ASSEMBLY_PREFIX")) okCount++; else diffCount++;
        if (AppendAssemblyPropertyLine(sb, ass1, ass2, "WIDTH")) okCount++; else diffCount++;
        if (AppendAssemblyPropertyLine(sb, ass1, ass2, "HEIGHT")) okCount++; else diffCount++;
        if (AppendAssemblyPropertyLine(sb, ass1, ass2, "LENGHT")) okCount++; else diffCount++;
        if (AppendAssemblyPropertyLine(sb, ass1, ass2, "LENGHT_GROSS")) okCount++; else diffCount++;
        if (AppendAssemblyPropertyLine(sb, ass1, ass2, "MATERIAL_TYPE")) okCount++; else diffCount++;
        if (AppendAssemblyPropertyLine(sb, ass1, ass2, "VOLUME")) okCount++; else diffCount++;
        if (AppendAssemblyPropertyLine(sb, ass1, ass2, "WEIGHT")) okCount++; else diffCount++;
        if (AppendAssemblyPropertyLine(sb, ass1, ass2, "WEIGHT_GROSS")) okCount++; else diffCount++;
        if (AppendAssemblyPropertyLine(sb, ass1, ass2, "WEIGHT_NET")) okCount++; else diffCount++;

        AppendSectionSummary(sb, okCount, diffCount);
    }

    private static bool AppendAssemblyPropertyLine(StringBuilder sb, Assembly ass1, Assembly ass2, string propertyName)
    {
        string leftValue = GetAssemblyProperty(ass1, propertyName);
        string rightValue = GetAssemblyProperty(ass2, propertyName);
        return AppendCompareLine(sb, propertyName, leftValue, rightValue);
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

    private static string NormalizeKeyValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "-")
        {
            return "N/A";
        }

        return value.Trim().ToUpperInvariant().Replace(" ", "_");
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
