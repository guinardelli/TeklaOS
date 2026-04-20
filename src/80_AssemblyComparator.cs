internal static class AssemblyComparator
{
    private const bool IncludeOkDetails = true;
    private const double NumericTolerance = 0.005;

    public static void CompareSelectedAssemblies()
    {
        Model model = ModelHelper.GetConnectedModel();
        if (model == null) return;

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

        if (ass1.Identifier.ID == ass2.Identifier.ID)
        {
            MessageBox.Show("Você selecionou a mesma peça duas vezes. Selecione duas peças diferentes para comparar.");
            return;
        }

        StringBuilder sb = new StringBuilder();
        int totalOk;
        int totalDiff;
        AppendAssemblyComparison(sb, ass1, ass2, model, out totalOk, out totalDiff);

        ReportWindow.ShowReport(sb.ToString(), "Comparacao de Conjuntos");
    }

    private static void AppendAssemblyComparison(StringBuilder sb, Assembly ass1, Assembly ass2, Model model, out int totalOk, out int totalDiff)
    {
        totalOk = 0;
        totalDiff = 0;

        string pos1 = ModelHelper.GetReportProperty(ass1, "ASSEMBLY_POS");
        string pos2 = ModelHelper.GetReportProperty(ass2, "ASSEMBLY_POS");

        sb.AppendLine("=== COMPARADOR DE CONJUNTOS ===");
        sb.AppendLine();
        sb.AppendLine(string.Format("Conjunto 1: {0} (Pos: {1})", Formatters.FormatValue(ass1.Name), pos1));
        sb.AppendLine(string.Format("Conjunto 2: {0} (Pos: {1})", Formatters.FormatValue(ass2.Name), pos2));
        sb.AppendLine(string.Format("Data: {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm")));
        sb.AppendLine();

        sb.AppendLine("=== SERIE DE NUMERACAO ===");
        int secOk;
        int secDiff;
        bool seriesEqual = AppendNumberingSeriesComparison(sb, ass1, ass2, out secOk, out secDiff);
        totalOk += secOk;
        totalDiff += secDiff;
        sb.AppendLine();

        if (!seriesEqual)
        {
            sb.AppendLine("[X] Comparacao interrompida: series de numeracao diferentes.");
            sb.AppendLine();
            AppendVerdict(sb, totalOk, totalDiff);
            return;
        }

        sb.AppendLine("=== PECA PRINCIPAL ===");
        AppendMainPartComparison(sb, ass1, ass2, out secOk, out secDiff);
        totalOk += secOk;
        totalDiff += secDiff;
        sb.AppendLine();

        sb.AppendLine("=== SECUNDARIAS ===");
        AppendSecondariesComparison(sb, ass1, ass2, model, out secOk, out secDiff);
        totalOk += secOk;
        totalDiff += secDiff;
        sb.AppendLine();

        sb.AppendLine("=== PROPRIEDADES DO CONJUNTO ===");
        AppendAssemblyPropertiesComparison(sb, ass1, ass2, out secOk, out secDiff);
        totalOk += secOk;
        totalDiff += secDiff;
        sb.AppendLine();

        AppendVerdict(sb, totalOk, totalDiff);
    }

    private static void AppendVerdict(StringBuilder sb, int okCount, int diffCount)
    {
        sb.AppendLine("=== RESUMO ===");
        sb.AppendLine(string.Format("Verificacoes: {0} total", okCount + diffCount));
        sb.AppendLine(string.Format("[OK] Iguais: {0}", okCount));
        if (diffCount > 0)
        {
            sb.AppendLine(string.Format("[X] Diferentes: {0}", diffCount));
        }
        sb.AppendLine();

        if (diffCount == 0)
        {
            sb.AppendLine(">>> APROVADO - Conjuntos equivalentes. <<<");
        }
        else
        {
            sb.AppendLine(string.Format(">>> REPROVADO - {0} diferenca(s) encontrada(s). <<<", diffCount));
        }
    }

    private static bool AppendNumberingSeriesComparison(StringBuilder sb, Assembly ass1, Assembly ass2, out int okCount, out int diffCount)
    {
        okCount = 0;
        diffCount = 0;

        string prefix1 = GetAssemblyNumberPrefix(ass1);
        string prefix2 = GetAssemblyNumberPrefix(ass2);
        string start1 = GetAssemblyNumberStart(ass1);
        string start2 = GetAssemblyNumberStart(ass2);

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

    private static void AppendMainPartComparison(StringBuilder sb, Assembly ass1, Assembly ass2, out int okCount, out int diffCount)
    {
        okCount = 0;
        diffCount = 0;

        Part main1 = ass1.GetMainPart() as Part;
        Part main2 = ass2.GetMainPart() as Part;

        if (main1 == null || main2 == null)
        {
            sb.AppendLine("[X] Peca principal: nao encontrada em um dos conjuntos.");
            diffCount++;
            return;
        }

        if (AppendCompareLine(sb, "Perfil", ModelHelper.GetReportProperty(main1, "PROFILE"), ModelHelper.GetReportProperty(main2, "PROFILE")))
        {
            okCount++;
        }
        else
        {
            diffCount++;
        }

        if (AppendCompareLine(sb, "Material", ModelHelper.GetReportProperty(main1, "MATERIAL"), ModelHelper.GetReportProperty(main2, "MATERIAL")))
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

        if (AppendCompareLine(sb, "Deformacao", ModelHelper.GetReportProperty(main1, "DEFORMATION"), ModelHelper.GetReportProperty(main2, "DEFORMATION")))
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

        string partPrefix1 = GetPartNumberPrefix(main1);
        string partPrefix2 = GetPartNumberPrefix(main2);
        if (AppendCompareLine(sb, "Num. Peca Prefixo", partPrefix1, partPrefix2))
        {
            okCount++;
        }
        else
        {
            diffCount++;
        }

        string partStart1 = GetPartNumberStart(main1);
        string partStart2 = GetPartNumberStart(main2);
        if (AppendCompareLine(sb, "Num. Peca StartNumber", partStart1, partStart2))
        {
            okCount++;
        }
        else
        {
            diffCount++;
        }

        AppendSectionSummary(sb, okCount, diffCount);
    }

    private static void AppendSecondariesComparison(StringBuilder sb, Assembly ass1, Assembly ass2, Model model, out int okCount, out int diffCount)
    {
        okCount = 0;
        diffCount = 0;

        int count1;
        int count2;
        Dictionary<string, int> map1 = BuildSecondarySignatureCounts(ass1, model, out count1);
        Dictionary<string, int> map2 = BuildSecondarySignatureCounts(ass2, model, out count2);

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
        string profile = NormalizeKeyValue(ModelHelper.GetReportProperty(part, "PROFILE"));
        string material = NormalizeKeyValue(ModelHelper.GetReportProperty(part, "MATERIAL"));
        string finish = NormalizeKeyValue(GetPartFinish(part));
        string partClass = NormalizeKeyValue(GetPartClass(part));
        Box box = GetLocalBoundingBox(part);

        return string.Format("Perfil={0};Material={1};Acabamento={2};Classe={3};Box={4}", profile, material, finish, partClass, FormatBox(box));
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
        if (solid == null || solid.MinimumPoint == null || solid.MaximumPoint == null)
        {
            var zero = new Tekla.Structures.Geometry3d.Point(0, 0, 0);
            return new Box(zero, zero);
        }
        return new Box(solid.MinimumPoint, solid.MaximumPoint);
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
                lines.Add(string.Format("[X] Secundaria faltando no conjunto 2 (x{0}):", count1 - count2));
                AppendSignatureReadable(lines, key);
            }
            else if (count2 > count1)
            {
                lines.Add(string.Format("[X] Secundaria extra no conjunto 2 (x{0}):", count2 - count1));
                AppendSignatureReadable(lines, key);
            }
        }

        return lines;
    }

    private static void AppendSignatureReadable(List<string> lines, string signature)
    {
        string[] parts = signature.Split(';');
        foreach (string part in parts)
        {
            int eqIndex = part.IndexOf('=');
            if (eqIndex > 0)
            {
                string label = part.Substring(0, eqIndex).Trim();
                string value = part.Substring(eqIndex + 1).Trim();
                if (string.Equals(label, "Box", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                lines.Add(string.Format("      {0}: {1}", label, value));
            }
        }
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

    private static string GetPartNumberPrefix(Part part)
    {
        if (part == null || part.PartNumber == null)
        {
            return "-";
        }

        return Formatters.FormatValue(part.PartNumber.Prefix);
    }

    private static string GetPartNumberStart(Part part)
    {
        if (part == null || part.PartNumber == null)
        {
            return "-";
        }

        return part.PartNumber.StartNumber.ToString();
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

    private static bool AppendCompareLineWithTolerance(StringBuilder sb, string label, string leftValue, string rightValue)
    {
        bool exactMatch = AreEqualNormalized(leftValue, rightValue);

        if (exactMatch)
        {
            if (IncludeOkDetails)
            {
                sb.AppendLine(string.Format("[OK] {0}: {1} | {2}", label, leftValue, rightValue));
            }
            return true;
        }

        double leftNum;
        double rightNum;
        bool leftIsNum = double.TryParse(leftValue, out leftNum);
        bool rightIsNum = double.TryParse(rightValue, out rightNum);

        if (leftIsNum && rightIsNum)
        {
            bool withinTolerance = AreEqualWithTolerance(leftNum, rightNum);
            if (withinTolerance)
            {
                sb.AppendLine(string.Format("[~] {0}: {1} | {2} (dentro da tolerancia)", label, leftValue, rightValue));
                return true;
            }
        }

        sb.AppendLine(string.Format("[X] {0}: {1} | {2}", label, leftValue, rightValue));
        return false;
    }

    private static bool AreEqualWithTolerance(double a, double b)
    {
        if (Math.Abs(a - b) < 0.001)
        {
            return true;
        }

        double maxAbs = Math.Max(Math.Abs(a), Math.Abs(b));
        if (maxAbs < 0.001)
        {
            return true;
        }

        double relDiff = Math.Abs(a - b) / maxAbs;
        return relDiff <= NumericTolerance;
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

    private static void AppendAssemblyPropertiesComparison(StringBuilder sb, Assembly ass1, Assembly ass2, out int okCount, out int diffCount)
    {
        okCount = 0;
        diffCount = 0;

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
        return AppendCompareLineWithTolerance(sb, propertyName, leftValue, rightValue);
    }

    private static string GetAssemblyProperty(Assembly ass, string propertyName)
    {
        string value = ModelHelper.GetReportProperty(ass, propertyName);
        if (value == "-")
        {
            if (string.Equals(propertyName, "LENGHT", StringComparison.OrdinalIgnoreCase))
            {
                value = ModelHelper.GetReportProperty(ass, "LENGTH");
            }
            else if (string.Equals(propertyName, "LENGHT_GROSS", StringComparison.OrdinalIgnoreCase))
            {
                value = ModelHelper.GetReportProperty(ass, "LENGTH_GROSS");
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
}
