internal static class ReportBuilder
{
    public static string BuildReport()
    {
        var model = new Model();
        if (!model.GetConnectionStatus())
        {
            MessageBox.Show("Relatorio do Modelo\n\nNao foi possivel conectar ao modelo Tekla. Abra um modelo e tente novamente.");
            return null;
        }

        var info = model.GetInfo();
        var projectInfo = model.GetProjectInfo();

        var sb = new StringBuilder();
        sb.AppendLine("Relatorio do Modelo");
        sb.AppendLine();
        sb.AppendLine("Modelo");
        sb.AppendLine(string.Format("Nome: {0}", Formatters.FormatValue(info.ModelName)));
        sb.AppendLine(string.Format("Caminho: {0}", Formatters.FormatValue(info.ModelPath)));
        sb.AppendLine(string.Format("Fase atual: {0}", info.CurrentPhase));
        sb.AppendLine(string.Format("Modelo compartilhado: {0}", Formatters.FormatBool(info.SharedModel)));
        sb.AppendLine(string.Format("Modelo single user: {0}", Formatters.FormatBool(info.SingleUserModel)));
        sb.AppendLine(string.Format("North direction: {0}", Formatters.FormatValue(info.NorthDirection)));
        sb.AppendLine();
        sb.AppendLine("Projeto");
        if (projectInfo == null)
        {
            sb.AppendLine("Nao foi possivel obter ProjectInfo.");
        }
        else
        {
            sb.AppendLine(string.Format("Nome: {0}", Formatters.FormatValue(projectInfo.Name)));
            sb.AppendLine(string.Format("Numero: {0}", Formatters.FormatValue(projectInfo.ProjectNumber)));
            sb.AppendLine(string.Format("Descricao: {0}", Formatters.FormatValue(projectInfo.Description)));
            sb.AppendLine(string.Format("Inicio: {0}", Formatters.FormatDate(projectInfo.StartDate)));
            sb.AppendLine(string.Format("Termino: {0}", Formatters.FormatDate(projectInfo.EndDate)));
            sb.AppendLine(string.Format("Designer: {0}", Formatters.FormatValue(projectInfo.Designer)));
            sb.AppendLine(string.Format("Objeto: {0}", Formatters.FormatValue(projectInfo.Object)));
            sb.AppendLine(string.Format("Localizacao: {0}", Formatters.FormatValue(projectInfo.Location)));
            sb.AppendLine(string.Format("Endereco: {0}", Formatters.FormatValue(projectInfo.Address)));
            sb.AppendLine(string.Format("Caixa postal: {0}", Formatters.FormatValue(projectInfo.PostalBox)));
            sb.AppendLine(string.Format("Cidade: {0}", Formatters.FormatValue(projectInfo.Town)));
            sb.AppendLine(string.Format("Regiao: {0}", Formatters.FormatValue(projectInfo.Region)));
            sb.AppendLine(string.Format("CEP: {0}", Formatters.FormatValue(projectInfo.PostalCode)));
            sb.AppendLine(string.Format("Pais: {0}", Formatters.FormatValue(projectInfo.Country)));
            sb.AppendLine(string.Format("Construtora: {0}", Formatters.FormatValue(projectInfo.Builder)));
            sb.AppendLine(string.Format("Info1: {0}", Formatters.FormatValue(projectInfo.Info1)));
            sb.AppendLine(string.Format("Info2: {0}", Formatters.FormatValue(projectInfo.Info2)));
            sb.AppendLine(string.Format("GUID: {0}", Formatters.FormatValue(projectInfo.GUID)));
        }

        return sb.ToString();
    }

    public static string BuildSelectedPartsReport()
    {
        var model = new Model();
        if (!model.GetConnectionStatus())
        {
            MessageBox.Show("Relatorio do Modelo\n\nNao foi possivel conectar ao modelo Tekla. Abra um modelo e tente novamente.");
            return null;
        }

        var selector = new ModelUI.ModelObjectSelector();
        var selected = selector.GetSelectedObjects();
        if (selected == null)
        {
            MessageBox.Show("Nenhum objeto selecionado.");
            return null;
        }

        var details = new StringBuilder();
        int totalSelected = 0;
        int partsCount = 0;

        while (selected.MoveNext())
        {
            totalSelected++;
            var part = selected.Current as Part;
            if (part == null)
            {
                continue;
            }

            partsCount++;
            details.AppendLine(string.Format("Peca {0}", partsCount));
            details.AppendLine(string.Format("Tipo: {0}", Formatters.FormatValue(part.GetType().Name)));
            details.AppendLine(string.Format("Nome: {0}", Formatters.FormatValue(part.Name)));
            details.AppendLine(string.Format("Perfil: {0}", Formatters.FormatValue(part.Profile == null ? null : part.Profile.ProfileString)));
            details.AppendLine(string.Format("Material: {0}", Formatters.FormatValue(part.Material == null ? null : part.Material.MaterialString)));
            details.AppendLine(string.Format("Classe: {0}", Formatters.FormatValue(part.Class)));
            details.AppendLine(string.Format("Acabamento: {0}", Formatters.FormatValue(part.Finish)));
            details.AppendLine(string.Format("Fase: {0}", Formatters.FormatPhaseNumber(part)));
            details.AppendLine(string.Format("GUID: {0}", Formatters.FormatValue(part.Identifier == null ? null : part.Identifier.GUID.ToString())));
            details.AppendLine();
        }

        if (totalSelected == 0)
        {
            MessageBox.Show("Nenhum objeto selecionado.");
            return null;
        }

        if (partsCount == 0)
        {
            MessageBox.Show("Nao ha pecas selecionadas. Selecione pecas e tente novamente.");
            return null;
        }

        var sb = new StringBuilder();
        sb.AppendLine("Pecas selecionadas");
        sb.AppendLine(string.Format("Total de objetos selecionados: {0}", totalSelected));
        sb.AppendLine(string.Format("Total de pecas: {0}", partsCount));
        sb.AppendLine();
        sb.Append(details.ToString());

        return sb.ToString();
    }
}
