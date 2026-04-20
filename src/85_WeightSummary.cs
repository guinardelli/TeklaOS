internal static class WeightSummary
{
    public static string BuildWeightByPhaseReport()
    {
        Model model = ModelHelper.GetConnectedModel();
        if (model == null) return null;

        var enumerator = model.GetModelObjectSelector().GetAllObjectsWithType(ModelObject.ModelObjectEnum.ASSEMBLY);
        if (enumerator == null)
        {
            MessageBox.Show("Nao foi possivel obter os conjuntos do modelo.");
            return null;
        }

        // Fase -> { count, weightNet, weightGross }
        var phaseData = new SortedDictionary<int, PhaseInfo>();
        int totalCount = 0;

        var previousCursor = Cursor.Current;
        Cursor.Current = Cursors.WaitCursor;
        try
        {
            while (enumerator.MoveNext())
            {
                Assembly assembly = enumerator.Current as Assembly;
                if (assembly == null) continue;

                int phaseNumber = GetPhaseNumber(assembly);
                double weightNet = GetDoubleProperty(assembly, "WEIGHT_NET");
                double weightGross = GetDoubleProperty(assembly, "WEIGHT_GROSS");

                PhaseInfo info;
                if (!phaseData.TryGetValue(phaseNumber, out info))
                {
                    info = new PhaseInfo();
                    phaseData[phaseNumber] = info;
                }

                info.Count++;
                info.WeightNet += weightNet;
                info.WeightGross += weightGross;
                totalCount++;
            }
        }
        finally
        {
            Cursor.Current = previousCursor;
        }

        if (totalCount == 0)
        {
            MessageBox.Show("Nenhum conjunto encontrado no modelo.");
            return null;
        }

        return FormatReport(phaseData, totalCount);
    }

    public static string BuildWeightByPhaseReportSelected()
    {
        Model model = ModelHelper.GetConnectedModel();
        if (model == null) return null;

        ModelUI.ModelObjectSelector uiSelector = new ModelUI.ModelObjectSelector();
        ModelObjectEnumerator enumSelected = uiSelector.GetSelectedObjects();
        if (enumSelected == null)
        {
            MessageBox.Show("Nenhum objeto selecionado.");
            return null;
        }

        var phaseData = new SortedDictionary<int, PhaseInfo>();
        int totalCount = 0;

        while (enumSelected.MoveNext())
        {
            Assembly assembly = enumSelected.Current as Assembly;
            if (assembly == null) continue;

            int phaseNumber = GetPhaseNumber(assembly);
            double weightNet = GetDoubleProperty(assembly, "WEIGHT_NET");
            double weightGross = GetDoubleProperty(assembly, "WEIGHT_GROSS");

            PhaseInfo info;
            if (!phaseData.TryGetValue(phaseNumber, out info))
            {
                info = new PhaseInfo();
                phaseData[phaseNumber] = info;
            }

            info.Count++;
            info.WeightNet += weightNet;
            info.WeightGross += weightGross;
            totalCount++;
        }

        if (totalCount == 0)
        {
            MessageBox.Show("Nenhum conjunto selecionado.");
            return null;
        }

        return FormatReport(phaseData, totalCount);
    }

    private static string FormatReport(SortedDictionary<int, PhaseInfo> phaseData, int totalCount)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== RESUMO DE PESO POR FASE ===");
        sb.AppendLine(string.Format("Data: {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm")));
        sb.AppendLine();

        double grandWeightNet = 0;
        double grandWeightGross = 0;

        foreach (var kvp in phaseData)
        {
            int phase = kvp.Key;
            PhaseInfo info = kvp.Value;

            sb.AppendLine(string.Format("--- Fase {0} ---", phase));
            sb.AppendLine(string.Format("  Conjuntos:   {0}", info.Count));
            sb.AppendLine(string.Format("  Peso liquido:  {0} kg", FormatWeight(info.WeightNet)));
            sb.AppendLine(string.Format("  Peso bruto:    {0} kg", FormatWeight(info.WeightGross)));
            sb.AppendLine();

            grandWeightNet += info.WeightNet;
            grandWeightGross += info.WeightGross;
        }

        sb.AppendLine("=== TOTAL GERAL ===");
        sb.AppendLine(string.Format("Conjuntos: {0}", totalCount));
        sb.AppendLine(string.Format("Peso liquido total:  {0} kg  ({1} t)", FormatWeight(grandWeightNet), FormatTon(grandWeightNet)));
        sb.AppendLine(string.Format("Peso bruto total:    {0} kg  ({1} t)", FormatWeight(grandWeightGross), FormatTon(grandWeightGross)));
        sb.AppendLine(string.Format("Fases: {0}", phaseData.Count));

        return sb.ToString();
    }

    private static int GetPhaseNumber(ModelObject obj)
    {
        if (obj == null) return 0;

        Phase phase;
        if (obj.GetPhase(out phase))
        {
            return phase.PhaseNumber;
        }
        return 0;
    }

    private static double GetDoubleProperty(ModelObject obj, string propertyName)
    {
        if (obj == null) return 0.0;

        double value = 0.0;
        if (obj.GetReportProperty(propertyName, ref value))
        {
            return value;
        }
        return 0.0;
    }

    private static string FormatWeight(double kg)
    {
        return string.Format("{0:N1}", kg);
    }

    private static string FormatTon(double kg)
    {
        return string.Format("{0:N2}", kg / 1000.0);
    }

    private class PhaseInfo
    {
        public int Count;
        public double WeightNet;
        public double WeightGross;
    }
}
