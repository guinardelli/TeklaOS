# CODEX

Este documento fornece ao Codex um unico ponto de referencia para desenvolver, gerar e distribuir macros
Tekla Structures 2023 neste repositorio.

## 1. Objetivo do projeto
- Macro WinForms para Tekla Structures que gera relatorios do modelo e com comandos de manutencao.
- Distribuicao via TSEP com botao no Applications & Components (A&C).

## 2. Fluxo de desenvolvimento obrigatorio
1. Edite apenas arquivos em `src/`.
2. Rode `scripts/build_macro.ps1` para compilar.
3. Para atualizar o Tekla instalado, execute `scripts/build_macro.ps1 -UpdateTekla`.
4. Para empacotar a extensao, gere o TSEP depois de copiar o codigo atualizado em `tsep/`.
5. Nunca altere diretamente `macros/MarnaTeklaOS.cs` ou `tsep/.../MarnaTeklaOS.cs`, pois sao gerados.

## 3. Arquitetura e pontos de entrada
- As classes do projeto estao em `src/*.cs`.
- O cabecalho e referencias ficam em `src/00_Header.cs`.
- O entry point unico em `src/10_EntryPoint.cs` tem `Script.Run` anotado com `MacroEntryPointAttribute`.
- Build gera:
  - `macros/MarnaTeklaOS.cs`
  - `tsep/RelatorioModelo/Environments/common/macros/modeling/MarnaTeklaOS.cs`
  - `C:\\TeklaStructures\\2023.0\\Environments\\common\\macros\\modeling\\MarnaTeklaOS.cs` (quando `-UpdateTekla`)

## 4. Requisitos do runtime Tekla
- Apenas um metodo com `[Tekla.Macros.Runtime.MacroEntryPointAttribute]`.
- Assinatura `public static void Run(Tekla.Macros.Runtime.IMacroRuntime runtime)`.
- Mantenha:
  - `#pragma reference "Tekla.Macros.Runtime"`
  - `#pragma reference "Tekla.Macros.Akit"` quando necessario
  - `using System.Windows.Forms` para dialogs.
- Nao use interpolacao com `$"..."`; prefira `string.Format` ou concatenacao.

### Exemplo seguro resumido
```
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

## 5. Erros comuns e correcoes direta
- **MacroEntryPointMissingException**: falta metodo publico static com `[MacroEntryPointAttribute]`.
- **CS1056 ($ inesperado)**: runtime nao suporta string interpolation. Use `string.Format`.
- **CS0103 MessageBox nao existe**: adicione `using System.Windows.Forms;`.
- **CS1061 ModelInfo sem ModelNumber**: use `ProjectInfo.ProjectNumber`.
- **CS1502/CS1503 StartDate/EndDate string**: trate como string ou parseie com `DateTime.TryParse`.
- **Warn obsoleto ModelSharingLocalPath/ServerPath**: remova se nao precisar e evite warnings.
- Sempre use propriedades testadas de `ModelInfo` (`ModelName`, `ModelPath`, `CurrentPhase`, `SharedModel`, `SingleUserModel`, `NorthDirection`) e de `ProjectInfo` (Nome, ProjectNumber, Description, StartDate/EndDate em string, Designer, Location, Address, PostalCode, Country, Builder, Info1, Info2, GUID).
- Para relatorios use as sobrecargas antigas de `GetAllReportProperties`/`GetReportProperty` com `ref`.
- Evite `yield` dentro de `try/catch`, pois gera CS1626 no compilador Tekla 2023.

## 6. Checklist de seguranca antes de rodar a macro
- Metodo `Run` anotado com `MacroEntryPointAttribute`.
- Parametro `IMacroRuntime runtime` presente.
- `#pragma reference "Tekla.Macros.Runtime"` incluso.
- Sem interpolacao `$`.
- `System.Windows.Forms` presente para `MessageBox`.
- Nao usar propriedades inexistentes ou obsoletas.
- Modelo aberto (`model.GetConnectionStatus()` retorna true).

## 7. Distribuicao e catalogo A&C
- Estrutura minima de TSEP:
  ```
  RelatorioModelo/
    Manifest.xml
    Icon.png
    Environments/
      common/
        macros/modeling/MarnaTeklaOS.cs
        extensions/RelatorioModelo/RelatorioModelo_ComponentCatalog.ac.xml
  ```
- No catalogo, a entrada de botao fica em `CatalogMacroModelingItem`.
- `Product` no `Manifest` deve ser `Type="Extension"`.
- `TepOutputFolder` precisa apontar para pasta gravavel.
- Atualize `MacroFileName` e `ItemIdString` em `tsep/.../RelatorioModelo_ComponentCatalog.ac.xml` se mudar o nome da macro.
- Atualize tambem o catalogo em `C:\\TeklaStructures\\2023.0\\Environments\\common\\extensions\\RelatorioModelo\\RelatorioModelo_ComponentCatalog.ac.xml`.
- Evite saida em pastas OneDrive para nao pegar `MissingNotWritable`.
- Rode `scripts/build_macro.ps1` para sincronizar as copias e use `-UpdateTekla` ao atualizar o Tekla.

## 8. Qualidade e estilo
- Aplicar DRY, KISS, YAGNI e principios SOLID (SRP, OCP, LSP, ISP, DIP).
- Mensagens/arquivos em portugues sem acentos (ASCII somente).
- Atualizar `README.md` sempre que o fluxo, nomes ou estrutura mudarem.

## 9. Notes finais
- Macro dentro do TSEP e uma copia; sempre reconstrua `macros/MarnaTeklaOS.cs` e a copia em `tsep/...` por meio de `scripts/build_macro.ps1` (use `-UpdateTekla` se for necessario).
- Para normalizar metadados, utilize `GetReportProperty` com `ref` e combine os valores de `Hashtable` retornados por `GetAllReportProperties`.
