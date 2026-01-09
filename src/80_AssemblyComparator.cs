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
        sb.AppendLine("Comparacao de Conjuntos");
        sb.AppendLine();

        AppendAssemblyParts(sb, ass1, "Conjunto 1: " + Formatters.FormatValue(ass1.Name));
        sb.AppendLine();

        AppendAssemblyParts(sb, ass2, "Conjunto 2: " + Formatters.FormatValue(ass2.Name));

        ReportWindow.ShowReport(sb.ToString());
    }

    private static void AppendAssemblyParts(StringBuilder sb, Assembly ass, string title)
    {
        sb.AppendLine(title);

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

        if (parts.Count == 0)
        {
            sb.AppendLine("Nenhuma peca encontrada.");
            sb.AppendLine();
            return;
        }

        sb.AppendLine("Posicao | Nome Pecas Principal | Perfil Pecas Principal | Pecas Principal Material | Largura | Altura | Comprimento | Volume | Peso | Local Nivel Sup | Local Nivel Inf | Global Nivel Sup | Global Nivel Inf");
        sb.AppendLine("--------------------------------------------------------------------------------------------------------------------------------");

        foreach (Part part in parts)
        {
            StringBuilder rowSb = new StringBuilder();
            rowSb.Append(GetReportProperty(part, "POSITION"));
            rowSb.Append(" | ");
            rowSb.Append(GetReportProperty(part, "NAME"));
            rowSb.Append(" | ");
            rowSb.Append(GetReportProperty(part, "PROFILE"));
            rowSb.Append(" | ");
            rowSb.Append(GetReportProperty(part, "MATERIAL"));
            rowSb.Append(" | ");
            rowSb.Append(GetReportProperty(part, "WIDTH"));
            rowSb.Append(" | ");
            rowSb.Append(GetReportProperty(part, "HEIGHT"));
            rowSb.Append(" | ");
            rowSb.Append(GetReportProperty(part, "LENGTH"));
            rowSb.Append(" | ");
            rowSb.Append(GetReportProperty(part, "VOLUME"));
            rowSb.Append(" | ");
            rowSb.Append(GetReportProperty(part, "WEIGHT"));
            rowSb.Append(" | ");
            rowSb.Append(GetReportProperty(part, "Z_MAX_LOCAL"));
            rowSb.Append(" | ");
            rowSb.Append(GetReportProperty(part, "Z_MIN_LOCAL"));
            rowSb.Append(" | ");
            rowSb.Append(GetReportProperty(part, "Z_MAX_GLOBAL"));
            rowSb.Append(" | ");
            rowSb.Append(GetReportProperty(part, "Z_MIN_GLOBAL"));
            sb.AppendLine(rowSb.ToString());
        }
        sb.AppendLine();
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
