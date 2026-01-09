3. API Principal e Classes Essenciais

## Hierarquia de Objetos (Model)

- **Model**: Ponto de entrada (`new Model()`).
- **ModelObject**: Classe base para tudo (ID, UDAs).
- **Part**: Base para Beam, ContourPlate, PolyBeam.
- **Assembly**: Conjunto de partes soldadas/parafusadas.

## Duas Classes ModelObjectSelector (IMPORTANTE!)

Existem **duas classes diferentes** com o mesmo nome:

| Classe | Namespace | Propósito |
|--------|-----------|-----------|
| `ModelObjectSelector` | `Tekla.Structures.Model` | Buscar objetos com filtros (`GetObjectsByFilter`) |
| `ModelObjectSelector` | `Tekla.Structures.Model.UI` | Seleção visual na interface (`Select`) |

```csharp
// Alias recomendado no header
using ModelUI = Tekla.Structures.Model.UI;

// Buscar com filtro (Model)
ModelObjectSelector modelSelector = model.GetModelObjectSelector();
var enumerator = modelSelector.GetObjectsByFilter(filter);

// Selecionar na UI (Model.UI)
var uiSelector = new ModelUI.ModelObjectSelector();
uiSelector.Select(arrayList);
```

> **Ver também:** [09_FILTERING_API.md](09_FILTERING_API.md) para documentação completa sobre filtros.

## Métodos de Seleção e Busca (2023)

Utilize `Tekla.Structures.Model.UI` para interagir com a seleção do usuário.

```csharp
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
```