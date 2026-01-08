6. Relatórios e Ferramentas de Manutenção
Leitura de UDAs (User Defined Attributes)
Essencial para relatórios personalizados.

public string GetPartStatus(Part part)
{
    string status = "";
    // Leitura segura de string UDA
    if (part.GetUserProperty("STATUS_PROJETO", ref status))
    {
        return status;
    }
    return "N/A";
}

Exemplo: Exportação Simples para CSV (Relatório)

public void GerarRelatorioMaterial(List<Part> partes, string caminhoArquivo)
{
    using (System.IO.StreamWriter file = new System.IO.StreamWriter(caminhoArquivo))
    {
        file.WriteLine("ID;Perfil;Material;Peso");
        foreach (var p in partes)
        {
            double peso = 0.0;
            p.GetReportProperty("WEIGHT", ref peso); // Pega propriedade calculada
            file.WriteLine($"{p.Identifier.ID};{p.Profile.ProfileString};{p.Material.MaterialString};{peso:F2}");
        }
    }
}

Manutenção: Limpeza de Objetos
Exemplo de lógica para deletar objetos inválidos (ex: beams sem comprimento).

public void CleanInvalidBeams()
{
    var model = new Model();
    var selector = model.GetModelObjectSelector();
    var objects = selector.GetAllObjectsWithType(ModelObject.ModelObjectEnum.BEAM);

    while (objects.MoveNext())
    {
        if (objects.Current is Beam beam)
        {
            double length = 0;
            beam.GetReportProperty("LENGTH", ref length);
            if (length < 10.0) // Critério de erro
            {
                beam.Delete();
            }
        }
    }
    model.CommitChanges();
}