# 9. API de Filtering do Tekla Structures

Este documento detalha o uso da API de filtros do Tekla Structures para seleção programática de objetos no modelo.

## Namespaces Necessários

```csharp
using Tekla.Structures;                        // TeklaStructuresDatabaseTypeEnum
using Tekla.Structures.Model;                  // Model, ModelObjectSelector
using Tekla.Structures.Filtering;              // FilterExpression, BinaryFilterExpression, etc.
using Tekla.Structures.Filtering.Categories;   // AssemblyFilterExpressions, ObjectFilterExpressions, etc.
using ModelUI = Tekla.Structures.Model.UI;     // UI.ModelObjectSelector (para seleção visual)
```

**IMPORTANTE:** Não confundir os namespaces:
- `Tekla.Structures.Filtering.Categories` (CORRETO para classes de filtro)
- `Tekla.Structures.Filtering.Expressions` (NÃO EXISTE - causa erro CS0234)

---

## Duas Classes ModelObjectSelector

Existem **duas classes diferentes** com o mesmo nome em namespaces distintos:

| Classe | Namespace | Propósito | Métodos Principais |
|--------|-----------|-----------|-------------------|
| `ModelObjectSelector` | `Tekla.Structures.Model` | Buscar objetos com filtros | `GetObjectsByFilter()`, `GetAllObjects()` |
| `ModelObjectSelector` | `Tekla.Structures.Model.UI` | Seleção visual na interface | `Select()`, `GetSelectedObjects()` |

### Exemplo de Uso Correto

```csharp
var model = new Model();

// Para BUSCAR objetos com filtro (usa Tekla.Structures.Model.ModelObjectSelector)
ModelObjectSelector modelSelector = model.GetModelObjectSelector();
var enumerator = modelSelector.GetObjectsByFilter(filterExpression);

// Para SELECIONAR visualmente na UI (usa Tekla.Structures.Model.UI.ModelObjectSelector)
var uiSelector = new ModelUI.ModelObjectSelector();
uiSelector.Select(arrayListDeObjetos);
```

---

## Hierarquia de FilterExpression

```
FilterExpression (base abstrata)
├── DataFilterExpression
│   ├── StringFilterExpression
│   │   ├── AssemblyFilterExpressions.Name
│   │   ├── AssemblyFilterExpressions.Prefix
│   │   ├── AssemblyFilterExpressions.PositionNumber
│   │   ├── PartFilterExpressions.Name
│   │   └── ComponentFilterExpressions.Name
│   ├── NumericFilterExpression
│   │   ├── ObjectFilterExpressions.Type
│   │   ├── AssemblyFilterExpressions.StartNumber
│   │   └── AssemblyFilterExpressions.Level
│   ├── BooleanFilterExpression
│   └── DateTimeFilterExpression
├── BinaryFilterExpression
└── BinaryFilterExpressionCollection
```

---

## Classes de Filtro por Categoria

### AssemblyFilterExpressions (Classes Aninhadas)

| Classe | Herança | Descrição |
|--------|---------|-----------|
| `Name` | StringFilterExpression | Nome do conjunto |
| `Prefix` | StringFilterExpression | Prefixo da marca |
| `PositionNumber` | StringFilterExpression | Número de posição |
| `Series` | StringFilterExpression | Série |
| `Type` | StringFilterExpression | Tipo |
| `Guid` | StringFilterExpression | GUID |
| `StartNumber` | NumericFilterExpression | Número inicial |
| `Level` | NumericFilterExpression | Nível |
| `IdNumber` | NumericFilterExpression | ID |
| `Phase` | NumericFilterExpression | Fase |
| `CustomString` | StringFilterExpression | UDA string |
| `CustomNumber` | NumericFilterExpression | UDA numérico |
| `CustomBoolean` | BooleanFilterExpression | UDA booleano |
| `CustomDateTime` | DateTimeFilterExpression | UDA data/hora |

### ObjectFilterExpressions

| Classe | Herança | Descrição |
|--------|---------|-----------|
| `Type` | NumericFilterExpression | Tipo de objeto (usa TeklaStructuresDatabaseTypeEnum) |
| `Phase` | NumericFilterExpression | Fase do objeto |
| `Guid` | StringFilterExpression | GUID do objeto |

---

## TeklaStructuresDatabaseTypeEnum

Este enum define os tipos de objetos para filtros. Está em `Tekla.Structures` (não em Filtering).

### Valores Principais

| Enum | Valor | Descrição |
|------|-------|-----------|
| `PART` | 2 | Peça |
| `CONNECTION` | 3 | Conexão |
| `COMPONENT` | 4 | Componente |
| `GRID` | 7 | Grid |
| `BOLT` | 10 | Parafuso |
| `WELDING` | 13 | Solda |
| `ASSEMBLY` | 15 | Conjunto |
| `REBAR` | 47 | Armadura |

---

## Criando BinaryFilterExpression

O `BinaryFilterExpression` possui **4 construtores** para diferentes tipos de dados:

### 1. Para Strings

```csharp
var nameFilter = new BinaryFilterExpression(
    new AssemblyFilterExpressions.Name(),      // StringFilterExpression
    StringOperatorType.IS_EQUAL,               // Operador
    new StringConstantFilterExpression("PP1")  // Valor
);
```

### 2. Para Números

```csharp
var typeFilter = new BinaryFilterExpression(
    new ObjectFilterExpressions.Type(),                              // NumericFilterExpression
    NumericOperatorType.IS_EQUAL,                                    // Operador
    new NumericConstantFilterExpression((int)TeklaStructuresDatabaseTypeEnum.ASSEMBLY)  // CAST obrigatório!
);
```

**IMPORTANTE:** `NumericConstantFilterExpression` espera `int`, não o enum diretamente. Use cast: `(int)TeklaStructuresDatabaseTypeEnum.ASSEMBLY`

### 3. Para Booleanos

```csharp
var boolFilter = new BinaryFilterExpression(
    new SomeFilterExpressions.SomeBool(),       // BooleanFilterExpression
    BooleanOperatorType.IS_EQUAL,               // Operador
    new BooleanConstantFilterExpression(true)   // Valor
);
```

### 4. Para DateTime

```csharp
var dateFilter = new BinaryFilterExpression(
    new SomeFilterExpressions.SomeDate(),         // DateTimeFilterExpression
    DateTimeOperatorType.IS_AFTER,                // Operador
    new DateTimeConstantFilterExpression(date)    // Valor
);
```

---

## Operadores Disponíveis

### StringOperatorType

- `IS_EQUAL` - Igual
- `IS_NOT_EQUAL` - Diferente
- `STARTS_WITH` - Começa com
- `ENDS_WITH` - Termina com
- `CONTAINS` - Contém
- `DOES_NOT_CONTAIN` - Não contém

### NumericOperatorType

- `IS_EQUAL` - Igual
- `IS_NOT_EQUAL` - Diferente
- `IS_GREATER_THAN` - Maior que
- `IS_GREATER_THAN_OR_EQUAL` - Maior ou igual
- `IS_LESS_THAN` - Menor que
- `IS_LESS_THAN_OR_EQUAL` - Menor ou igual

---

## Combinando Filtros com BinaryFilterExpressionCollection

Para combinar múltiplos filtros, use `BinaryFilterExpressionCollection`:

```csharp
var collection = new BinaryFilterExpressionCollection();

// Filtro 1: Tipo = ASSEMBLY
var typeFilter = new BinaryFilterExpression(
    new ObjectFilterExpressions.Type(),
    NumericOperatorType.IS_EQUAL,
    new NumericConstantFilterExpression((int)TeklaStructuresDatabaseTypeEnum.ASSEMBLY)
);
collection.Add(new BinaryFilterExpressionItem(typeFilter, BinaryFilterOperatorType.BOOLEAN_AND));

// Filtro 2: Nome = "PP1"
var nameFilter = new BinaryFilterExpression(
    new AssemblyFilterExpressions.Name(),
    StringOperatorType.IS_EQUAL,
    new StringConstantFilterExpression("PP1")
);
collection.Add(new BinaryFilterExpressionItem(nameFilter, BinaryFilterOperatorType.BOOLEAN_AND));

// Usar o filtro combinado
var selector = model.GetModelObjectSelector();
var enumerator = selector.GetObjectsByFilter(collection);
```

### BinaryFilterOperatorType

- `BOOLEAN_AND` - E lógico
- `BOOLEAN_OR` - OU lógico

---

## Exemplo Completo: Selecionar Assemblies por Nome

```csharp
using System;
using System.Collections;
using Tekla.Structures;
using Tekla.Structures.Model;
using Tekla.Structures.Filtering;
using Tekla.Structures.Filtering.Categories;
using ModelUI = Tekla.Structures.Model.UI;

public static class AssemblySelector
{
    public static void SelectAssembliesByName(string assemblyName)
    {
        var model = new Model();
        if (!model.GetConnectionStatus())
        {
            return;
        }

        // Criar filtro combinado
        var collection = new BinaryFilterExpressionCollection();

        // Filtro de tipo (ASSEMBLY)
        var typeFilter = new BinaryFilterExpression(
            new ObjectFilterExpressions.Type(),
            NumericOperatorType.IS_EQUAL,
            new NumericConstantFilterExpression((int)TeklaStructuresDatabaseTypeEnum.ASSEMBLY)
        );
        collection.Add(new BinaryFilterExpressionItem(typeFilter, BinaryFilterOperatorType.BOOLEAN_AND));

        // Filtro de nome
        var nameFilter = new BinaryFilterExpression(
            new AssemblyFilterExpressions.Name(),
            StringOperatorType.IS_EQUAL,
            new StringConstantFilterExpression(assemblyName)
        );
        collection.Add(new BinaryFilterExpressionItem(nameFilter, BinaryFilterOperatorType.BOOLEAN_AND));

        // Buscar objetos (usa Tekla.Structures.Model.ModelObjectSelector)
        var modelSelector = model.GetModelObjectSelector();
        var enumerator = modelSelector.GetObjectsByFilter(collection);

        // Coletar resultados
        var selection = new ArrayList();
        while (enumerator.MoveNext())
        {
            var assembly = enumerator.Current as Assembly;
            if (assembly != null)
            {
                selection.Add(assembly);
            }
        }

        // Selecionar visualmente na UI (usa Tekla.Structures.Model.UI.ModelObjectSelector)
        if (selection.Count > 0)
        {
            var uiSelector = new ModelUI.ModelObjectSelector();
            uiSelector.Select(selection);
        }
    }
}
```

---

## Erros Comuns e Soluções

### Erro CS0234: Namespace 'Expressions' não existe

**Causa:** Usar `Tekla.Structures.Filtering.Expressions`
**Solução:** Usar `Tekla.Structures.Filtering.Categories`

```csharp
// ERRADO
using Tekla.Structures.Filtering.Expressions;

// CORRETO
using Tekla.Structures.Filtering.Categories;
```

### Erro CS0103: 'TeklaStructuresDatabaseTypeEnum' não existe

**Causa:** Falta o using do namespace `Tekla.Structures`
**Solução:** Adicionar o using

```csharp
using Tekla.Structures;  // Contém TeklaStructuresDatabaseTypeEnum
```

### Erro CS1503: Não é possível converter enum para NumericConstantFilterExpression

**Causa:** `NumericConstantFilterExpression` espera `int`, não enum
**Solução:** Fazer cast explícito

```csharp
// ERRADO
new NumericConstantFilterExpression(TeklaStructuresDatabaseTypeEnum.ASSEMBLY)

// CORRETO
new NumericConstantFilterExpression((int)TeklaStructuresDatabaseTypeEnum.ASSEMBLY)
```

### Erro CS1503: Não é possível converter ModelObjectSelector

**Causa:** Confusão entre as duas classes `ModelObjectSelector`
**Solução:** Usar o tipo correto para cada operação

```csharp
// Para buscar com filtro - usa Tekla.Structures.Model.ModelObjectSelector
ModelObjectSelector modelSelector = model.GetModelObjectSelector();
modelSelector.GetObjectsByFilter(filter);

// Para selecionar na UI - usa Tekla.Structures.Model.UI.ModelObjectSelector
var uiSelector = new ModelUI.ModelObjectSelector();
uiSelector.Select(selection);
```

### Erro CS1502: Argumentos inválidos em BinaryFilterExpression

**Causa:** Tipos incompatíveis no construtor
**Solução:** Usar tipos correspondentes:

| FilterExpression | OperatorType | ConstantFilterExpression |
|-----------------|--------------|--------------------------|
| StringFilterExpression | StringOperatorType | StringConstantFilterExpression |
| NumericFilterExpression | NumericOperatorType | NumericConstantFilterExpression |
| BooleanFilterExpression | BooleanOperatorType | BooleanConstantFilterExpression |
| DateTimeFilterExpression | DateTimeOperatorType | DateTimeConstantFilterExpression |

---

## Referências

- [ModelObjectSelector.GetObjectsByFilter | Tekla Developer Center](https://developer.tekla.com/doc/tekla-structures/2025/get-objects-filter-method-53051)
- [BinaryFilterExpression Class | Tekla Developer Center](https://developer.tekla.com/doc/tekla-structures/2024/binary-filter-expression-class-25655)
- [TeklaStructuresDatabaseTypeEnum | Tekla Developer Center](https://developer.tekla.com/doc/tekla-structures/2025/tekla-structures-database-type-enum-enumeration-45544)
- [AssemblyFilterExpressions | Tekla Developer Center](https://developer.tekla.com/doc/tekla-structures/2024/assembly-filter-expressions-prefix-class-25817)
- [ObjectFilterExpressions.Type | Tekla Developer Center](https://developer.tekla.com/doc/tekla-structures/2023/object-filter-expressions-type-class-16423)
