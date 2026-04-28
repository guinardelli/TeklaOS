# Lições Aprendidas: Tekla Open API (Integração Modelo & Desenho)

Este documento registra os principais "gotchas" (pegadinhas) e aprendizados obtidos durante a criação da ferramenta "Vínculo Modelo e Desenho" (`DrawingModelLinker.cs`), a fim de prevenir erros futuros.

## 1. Limitações do Compilador Nativo de Macros do Tekla
As _Macros_ do Tekla são compiladas em tempo de execução (`Tekla.Macros.Runtime`) e utilizam um compilador de **C# mais antigo (C# 5.0 ou inferior)**. 
- ❌ **Evite:** Interpolação de Strings (`$"texto {var}"`). 
  - ✔️ **Use:** `string.Format("texto {0}", var)`.
- ❌ **Evite:** Operador Null-conditional (`?.` e `?[]`). 
  - ✔️ **Use:** Blocos tradicionais `if (obj != null)`.
- ❌ **Evite:** Pattern Matching em condicionais (`if (obj is AssemblyDrawing ad)`). 
  - ✔️ **Use:** Cast tradicional `var ad = obj as AssemblyDrawing; if (ad != null)`

*(O uso dos construtores mais modernos gera erros inexplicáveis como `error CS1056: Caractere '$' inesperado` e `error CS1525: Termo de expressão inválido 'else'` no Tekla).*

## 2. Conflito Crítico de Namespaces: `Model` vs `Drawing`
Nunca jogue ambos os namespaces principais de forma global (`using Tekla.Structures.Model;` e `using Tekla.Structures.Drawing;`) no `00_Header.cs` de scripts mesclados.
Essas duas bibliotecas compartilham internamente as mesmas nomenclaturas de classe (ex: `ModelObject`, `Part`). O compilador retornará um estrondoso `error CS0104: Referência ambígua`.
- **A Solução:** Mantenha `using Tekla.Structures.Model;` global. Ao escrever o módulo focado na manipulação de desenho, referencie as classes de forma 100% explícita:
  - Ex: `Tekla.Structures.Drawing.DrawingHandler` e `Tekla.Structures.Drawing.AssemblyDrawing`.

## 3. A Ilusão da Classe `CastUnit`
Embora a API de **Document Manager** (`Tekla.Structures.Drawing`) separe lógicamente os formatos com `CastUnitDrawing`, na API orientada ao **Modelo 3D** não existe a classe `CastUnit`.
- Qualquer bloco de concreto pré-moldado moldado (`Cast Unit`) selecionado no ambiente 3D é simplesmente interpretado pela API como classe-mãe mãe **`Assembly`** com o subtipo enumerado correspondente (`Assembly.AssemblyTypeEnum.CAST_UNIT`).
- **Problema encontrado:** Declarar `CastUnit cu = obj as CastUnit;` gerou `error CS0246: O nome ou o tipo 'CastUnit' não pôde ser encontrado`.
- **A Solução:** Processe apenas via cast genérico `Assembly`.

## 4. Obtendo a String da Posição / Marca (Mark)
Na Tekla API o objeto puro de identificação `AssemblyNumber` não expõe a propriedade de texto pronto `.Mark` que os desenvolvedores costumam esperar! *(Se tentar dar um `.Mark`, recebe `error CS1061: NumberingSeries não contém definição para Mark`)*.
O `NumberingSeries` revela apenas `.StartNumber` e `.Prefix`.
- **A Solução Definida:** A maneira segura de obter a string final legível (ex: `"P-1"`) para se conectar ao `Drawing.Mark` é consultando as variáveis de sistema via `GetReportProperty()`:
  - Para assembies e pré-moldados: `ModelHelper.GetReportProperty(assembly, "ASSEMBLY_POS")`
  - Para peças unicas/avulsas: `ModelHelper.GetReportProperty(part, "PART_POS")`

## 5. Capturando todos os Documentos do Projeto (Drawings)
- ❌ `DrawingHandler.GetDrawingSelector().GetDrawings()` 
  - Isso jorrava o erro: *`Tekla.Structures.Drawing.UI.DrawingSelector não contém definição de GetDrawings()`*. O objeto Selector só serve para buscar o que o usuário **marcou na tela com o mouse**.
- ✔️ `DrawingHandler.GetDrawings()` 
  - Essa simples propriedade iteradora (`DrawingEnumerator`) puxa todos os desenhos presentes no _Document Manager_ com segurança.

## 6. GADrawing vs AssemblyDrawing para Vistas Customizadas
Ao criar vistas 3D/isometricas customizadas com `CoordinateSystem` rotacionado:
- ❌ **Evite `AssemblyDrawing`**: Cria automaticamente vistas padrao (frontal, lateral, topo) que interferem no layout.
- ✔️ **Use `GADrawing`**: Canvas vazio. Permite inserir apenas as vistas desejadas via `new View(sheet, viewCS, displayCS, AABB)`.
- O construtor `new GADrawing("standard")` recebe o nome do arquivo de atributos (`.ga`). Se nao existir, usa defaults internos.

## 7. Construtor de View para Vistas Customizadas
O construtor confirmado e funcional para vistas em desenhos e:
```csharp
var view = new Tekla.Structures.Drawing.View(sheet, viewCS, displayCS, restrictionBox);
view.Name = "nome";
view.Insert();
```
- `sheet`: obtido via `drawing.GetSheet()`
- `viewCS`: `CoordinateSystem` com vetores rotacionados
- `displayCS`: mesmo que `viewCS` mas com origem `(0,0,0)`
- `restrictionBox`: `AABB` com o volume do modelo a exibir
- Apos inserir, usar `drawing.PlaceViews()` (se disponivel) para layout automatico
- Se `PlaceViews()` nao existir, o usuario posiciona manualmente no editor

## 8. Rotacao de CoordinateSystem sem Matrix
Para compatibilidade com o compilador C# 5.0 de macros, a rotacao de vetores do `CoordinateSystem` deve ser feita por trigonometria manual (`Math.Sin`, `Math.Cos`), sem depender de `Tekla.Structures.Geometry3d.Matrix`.
- Rotacao em Z (azimute): aplica-se nos componentes X e Y
- Rotacao em X (elevacao): aplica-se nos componentes Y e Z
- Aplicar na ordem: primeiro R_z, depois R_x
