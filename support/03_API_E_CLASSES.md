3. API Principal e Classes Essenciais
Hierarquia de Objetos (Model)
Model: Ponto de entrada (new Model()).

ModelObject: Classe base para tudo (ID, UDAs).

Part: Base para Beam, ContourPlate, PolyBeam.

Assembly: Conjunto de partes soldadas/parafusadas.

Métodos de Seleção e Busca (2023)
Utilize Tekla.Structures.Model.UI para interagir com a seleção do usuário.

// Exemplo: Obter objetos selecionados na UI
public List<Part> GetSelectedParts()
{
    var model = new Tekla.Structures.Model.Model();
    var selector = model.GetModelObjectSelector();
    var enumObjects = selector.GetSelectedObjects();
    
    var parts = new List<Part>();
    while (enumObjects.MoveNext())
    {
        if (enumObjects.Current is Part part)
        {
            parts.Add(part);
        }
    }
    return parts;
}