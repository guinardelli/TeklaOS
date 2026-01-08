# Instrucoes de programacao para macros no Tekla Structures 2023

Este arquivo resume os erros encontrados e as correcoes aplicadas ao longo do desenvolvimento da macro, com foco no runtime de macros do Tekla Structures 2023 e no uso correto da API.

## 1) Entry point obrigatorio (MacroEntryPointMissingException)

Erro:
- `Tekla.Macros.Runtime.MacroEntryPointMissingException`

Causa:
- O runtime de macros do Tekla 2023 exige um unico metodo publico, estatico, marcado com o atributo `MacroEntryPointAttribute`.

Correcao:
- Adicionar a referencia ao runtime e a assinatura correta:

```csharp
#pragma warning disable 1633 // Unrecognized #pragma directive
#pragma reference "Tekla.Macros.Runtime"
#pragma warning restore 1633 // Unrecognized #pragma directive

public class Script
{
    [Tekla.Macros.Runtime.MacroEntryPointAttribute()]
    public static void Run(Tekla.Macros.Runtime.IMacroRuntime runtime)
    {
        // codigo aqui
    }
}
```

Observacoes:
- Deve existir apenas um metodo publico com esse atributo.
- Se precisar usar Akit ou WPF runtime, adicionar `#pragma reference "Tekla.Macros.Akit"` e `#pragma reference "Tekla.Macros.Wpf.Runtime"`.

## 2) Erro CS1056: caractere '$' inesperado

Erro:
- `CS1056: Caractere '$' inesperado`

Causa:
- O compilador do runtime de macros nao suporta interpolacao de string (C# 6+).

Correcao:
- Usar `string.Format` ou concatenacao simples.

Exemplo:
```csharp
MessageBox.Show(string.Format("Modelo: {0}", info.ModelName));
```

## 3) Erro CS0103: MessageBox nao existe no contexto atual

Erro:
- `CS0103: O nome 'MessageBox' nao existe no contexto atual`

Causa:
- Faltou referencia ao namespace correto.

Correcao:
- Usar `System.Windows.Forms`:

```csharp
using System.Windows.Forms;

MessageBox.Show("Mensagem");
```

## 4) Erro CS1061: ModelInfo nao contem ModelNumber

Erro:
- `CS1061: 'Tekla.Structures.Model.ModelInfo' nao contem definicao para 'ModelNumber'`

Causa:
- `ModelNumber` nao existe em `ModelInfo` no Tekla 2023.

Correcao:
- O numero do projeto fica em `ProjectInfo.ProjectNumber`.

Exemplo:
```csharp
var info = model.GetInfo();
var projectInfo = model.GetProjectInfo();
MessageBox.Show(string.Format("Modelo: {0}\nNumero: {1}",
    info.ModelName, projectInfo.ProjectNumber));
```

## 5) Erro CS1502/CS1503: StartDate/EndDate sao string

Erro:
- `CS1502/CS1503: nao e possivel converter de 'string' em 'System.DateTime'`

Causa:
- `ProjectInfo.StartDate` e `ProjectInfo.EndDate` sao strings no Tekla 2023.

Correcao:
- Tratar como string ou tentar parsear:

```csharp
private static string FormatDate(string value)
{
    if (string.IsNullOrEmpty(value))
    {
        return "-";
    }

    DateTime parsed;
    if (DateTime.TryParse(value, out parsed))
    {
        return parsed.ToString("yyyy-MM-dd");
    }

    return value;
}
```

## 6) Avisos de obsoleto: ModelSharingLocalPath/ServerPath

Aviso:
- `ProjectInfo.ModelSharingLocalPath` e `ProjectInfo.ModelSharingServerPath` estao obsoletos.

Recomendacao:
- Remover esses campos para evitar warnings, a nao ser que voce precise manter compatibilidade com outro fluxo legado.

## 7) Campos validos do ModelInfo (Tekla 2023)

Principais propriedades disponiveis e testadas:
- `ModelName`
- `ModelPath`
- `CurrentPhase`
- `SharedModel`
- `SingleUserModel`
- `NorthDirection`

Uso:
```csharp
var info = model.GetInfo();
```

## 8) Campos validos do ProjectInfo (Tekla 2023)

Principais propriedades disponiveis e testadas:
- `Name`
- `ProjectNumber`
- `Description`
- `StartDate` (string)
- `EndDate` (string)
- `Designer`
- `Object`
- `Location`
- `Address`
- `PostalBox`
- `Town`
- `Region`
- `PostalCode`
- `Country`
- `Builder`
- `Info1`
- `Info2`
- `GUID`

Uso:
```csharp
var projectInfo = model.GetProjectInfo();
```

## 9) Template de macro seguro (resumo)

```csharp
#pragma warning disable 1633
#pragma reference "Tekla.Macros.Runtime"
#pragma warning restore 1633

using System.Windows.Forms;
using Tekla.Structures.Model;

public class Script
{
    [Tekla.Macros.Runtime.MacroEntryPointAttribute()]
    public static void Run(Tekla.Macros.Runtime.IMacroRuntime runtime)
    {
        var model = new Model();
        if (!model.GetConnectionStatus())
        {
            MessageBox.Show("Nao foi possivel conectar ao modelo.");
            return;
        }

        var info = model.GetInfo();
        MessageBox.Show(string.Format("Modelo: {0}", info.ModelName));
    }
}
```

## 10) Checklist rapido antes de rodar a macro

- [ ] Metodo `Run` possui `MacroEntryPointAttribute`.
- [ ] Assinatura correta com `IMacroRuntime runtime`.
- [ ] `#pragma reference "Tekla.Macros.Runtime"` presente.
- [ ] Sem interpolacao de string (`$"..."`).
- [ ] `using System.Windows.Forms;` adicionado para `MessageBox`.
- [ ] Evitar propriedades inexistentes/obsoletas da API.
- [ ] Modelo aberto e conectado (`GetConnectionStatus()`).

## 11) Distribuicao com TSEP e botao no Applications & Components

Objetivo:
- Empacotar a macro como extensao e adicionar um botao no A&C (Component Catalog).

Estrutura minima do pacote:
```
RelatorioModelo\
  Manifest.xml
  Icon.png
  Environments\
    common\
      macros\
        modeling\
          MarnaTeklaOS.cs
      extensions\
        RelatorioModelo\
          RelatorioModelo_ComponentCatalog.ac.xml
```

Arquivo de catalogo (botao no A&C):
```xml
<CatalogMacroModelingItem>
  <DisplayName>Relatorio do Modelo</DisplayName>
  <MacroFileName>MarnaTeklaOS.cs</MacroFileName>
  <Description>Relatorio do modelo e pecas selecionadas.</Description>
  <MacroLocation>GLOBAL</MacroLocation>
</CatalogMacroModelingItem>
```

Manifest (pontos criticos):
- O Product precisa de `Type="Extension"`.
- `TepOutputFolder` deve apontar para uma pasta gravavel (evitar OneDrive).

Exemplo de trecho:
```xml
<Product Id="RelatorioModelo" Version="1.0" Type="Extension" />
<SourcePathVariable Id="TepOutputFolder" Value="C:\TeklaTSEPOutput" />
```

Erros comuns ao gerar o TSEP:
- `MissingNotWritable`: a pasta de saida nao existe ou nao e gravavel.
  - Solucao: criar a pasta `output` ou apontar `TepOutputFolder` para um caminho local.
  - Em pastas do OneDrive, o Builder pode falhar por permissao/arquivo reparse.

Observacao:
- A macro dentro do TSEP e uma copia. Sempre atualize a copia antes de gerar o pacote.

## 12) Novos aprendizados recentes

- Evite `yield` dentro de `try/catch` ao compilar macros: o compilador Tekla 2023 nao aceita essa combinacao, e envolver um `yield return` com `try/catch` gera erros CS1626.
- Use as sobrecargas antigas de `GetAllReportProperties` e `GetReportProperty` (com `ref`) porque a versão 2023 nao expõe as variantes sem parâmetros; chame `GetAllReportProperties(names, types, rawValues, ref props)` e trate o retorno booleano.
- Para normalizar nomes e metadata, colecione `string`, `double` e `int` via `GetReportProperty` com `ref` e adicione as strings extraídas de `Hashtable` retornados por `GetAllReportProperties`.
- Sempre reconstrua `macros/MarnaTeklaOS.cs` e a copia dentro de `tsep/RelatorioModelo/Environments/common/macros/modeling/` através de `scripts/build_macro.ps1` depois de mudar a camada `src/`, garantindo que a copia instalada no Tekla receba `scripts/build_macro.ps1 -UpdateTekla`.
