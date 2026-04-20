# MELHORIAS_01 - Refatoracao e Polimento do Codigo Existente

Este documento descreve o passo a passo para aplicar melhorias ao codigo atual sem adicionar funcionalidades novas.

---

## 1. Extrair Design System de `MenuUi`

**Arquivo novo:** `src/15_DesignSystem.cs`
**Arquivo modificado:** `src/20_MenuUi.cs`

### Passo a passo

1. Criar `src/15_DesignSystem.cs` com classe `internal static class DesignSystem`.
2. Mover todas as constantes de cor (`C_FundoForm`, `C_CardFundo`, `C_Cabecalho`, etc.) de `MenuUi` para `DesignSystem`.
3. Mover todas as constantes de fonte (`F_Titulo`, `F_Texto`, `F_Secao`) para `DesignSystem`.
4. Mover os metodos helpers de criacao de controles para `DesignSystem`:
   - `CriarGrupoDashboard(string titulo)`
   - `CriarLayoutVertical()`
   - `AdicionarLinha(TableLayoutPanel, Control)`
   - `CriarLabelInfo(string texto)`
   - `CriarBotaoDashboard(string texto, bool ehPerigo)`
5. Em `MenuUi.Show()`, trocar todas as referencias para usar `DesignSystem.C_FundoForm`, `DesignSystem.F_Texto`, `DesignSystem.CriarBotaoDashboard(...)`, etc.
6. Rodar `scripts/build_macro.ps1` e verificar que a macro compila sem erros.

---

## 2. Aplicar Design System a `ReportWindow`

**Arquivo modificado:** `src/40_ReportWindow.cs`

### Passo a passo

1. Substituir `SystemColors.Window` por `DesignSystem.C_CardFundo`.
2. Substituir `SystemColors.WindowText` por `DesignSystem.C_TextoPrimario`.
3. Substituir `new Font("Consolas", 9f)` por uma constante em `DesignSystem` (ex: `F_Mono = new Font("Consolas", 9f)`).
4. Aplicar estilo dos botoes usando `DesignSystem.CriarBotaoDashboard` ou, se nao couber, pelo menos usar as mesmas cores (`C_CardFundo`, `C_Borda`, `C_TextoPrimario`).
5. Definir `form.BackColor = DesignSystem.C_FundoForm` e `form.Font = DesignSystem.F_Texto`.
6. Verificar visualmente que a janela de relatorio ficou consistente com o menu principal.

---

## 3. Eliminar duplicacao de conexao ao modelo

**Arquivo novo:** `src/25_ModelHelper.cs`
**Arquivos modificados:** `src/30_ReportBuilder.cs`, `src/70_AssemblySelectionHelper.cs`, `src/80_AssemblyComparator.cs`

### Passo a passo

1. Criar `src/25_ModelHelper.cs` com classe `internal static class ModelHelper`.
2. Adicionar metodo:
   ```csharp
   public static Model GetConnectedModel()
   {
       var model = new Model();
       if (!model.GetConnectionStatus())
       {
           MessageBox.Show("Nao foi possivel conectar ao modelo Tekla. Abra um modelo e tente novamente.");
           return null;
       }
       return model;
   }
   ```
3. Em `ReportBuilder.BuildReport()`, substituir as linhas 5-10 por:
   ```csharp
   var model = ModelHelper.GetConnectedModel();
   if (model == null) return null;
   ```
4. Fazer o mesmo em `ReportBuilder.BuildSelectedPartsReport()`.
5. Fazer o mesmo em `AssemblySelectionHelper.SelectAssemblies()`.
6. Fazer o mesmo em `AssemblyComparator.CompareSelectedAssemblies()`.
7. Rodar `scripts/build_macro.ps1` e verificar.

---

## 4. Consolidar `GetReportProperty` duplicado

**Arquivo modificado:** `src/25_ModelHelper.cs`
**Arquivos modificados:** `src/70_AssemblySelectionHelper.cs`, `src/80_AssemblyComparator.cs`

### Passo a passo

1. Em `ModelHelper`, adicionar metodo compartilhado:
   ```csharp
   public static string GetReportProperty(ModelObject obj, string propertyName)
   {
       if (obj == null) return "-";

       string stringValue = null;
       if (obj.GetReportProperty(propertyName, ref stringValue))
           return Formatters.FormatValue(stringValue);

       double doubleValue = 0.0;
       if (obj.GetReportProperty(propertyName, ref doubleValue))
           return Formatters.FormatValue(string.Format("{0:F1}", doubleValue));

       int intValue = 0;
       if (obj.GetReportProperty(propertyName, ref intValue))
           return intValue.ToString();

       return "-";
   }
   ```
2. Em `AssemblyComparator`, remover o metodo privado `GetReportProperty` (linhas 551-571) e trocar todas as chamadas para `ModelHelper.GetReportProperty(...)`.
3. Em `AssemblySelectionHelper`, remover o metodo `TryGetReportProperty` e usar `ModelHelper.GetReportProperty(...)`, tratando `"-"` como nao encontrado.
4. Rodar `scripts/build_macro.ps1` e verificar.

---

## 5. Adicionar feedback visual (cursor de espera) nos botoes

**Arquivo modificado:** `src/20_MenuUi.cs`

### Passo a passo

1. No click handler do botao "Gerar relatorio" (linhas 103-106), envolver com cursor de espera:
   ```csharp
   btnGeral.Click += delegate {
       var prev = Cursor.Current;
       Cursor.Current = Cursors.WaitCursor;
       try
       {
           string r = ReportBuilder.BuildReport();
           if(!string.IsNullOrEmpty(r)) ReportWindow.ShowReport(r);
       }
       finally { Cursor.Current = prev; }
   };
   ```
2. Repetir o mesmo padrao para o botao "Ver pecas selecionadas" (linhas 109-112).
3. Repetir para o botao "Comparar conjuntos" (linha 222).
4. Nota: o botao "Selecionar pecas" ja tem cursor de espera dentro do `AssemblySelectionHelper`, entao nao precisa.

---

## 6. Adicionar botao [?] ao "Comparar conjuntos"

**Arquivo modificado:** `src/20_MenuUi.cs`

### Passo a passo

1. Criar um `FlowLayoutPanel` para o botao "Comparar conjuntos", igual ao padrao dos outros botoes (ver `repairActions` e `selectionActions`).
2. Criar botao `btnCompareHelp` com texto `"[?]"` e o mesmo estilo dos outros botoes de help.
3. No click do `btnCompareHelp`, exibir:
   ```csharp
   MessageBox.Show(
       "Selecione exatamente dois conjuntos (ex: PP1 e PP2) e clique para comparar as pecas lado a lado.",
       "Como usar",
       MessageBoxButtons.OK,
       MessageBoxIcon.Information
   );
   ```
4. Substituir a linha `AdicionarLinha(actionsLayout, btnCompare)` por adicionar o `FlowLayoutPanel` contendo `btnCompare` + `btnCompareHelp`.

---

## 7. Proteger `GetSolid()` contra nulo

**Arquivo modificado:** `src/80_AssemblyComparator.cs`

### Passo a passo

1. No metodo `GetLocalBoundingBox(Part part)` (linha 296), adicionar verificacao:
   ```csharp
   private static Box GetLocalBoundingBox(Part part)
   {
       Solid solid = part.GetSolid();
       if (solid == null || solid.MinimumPoint == null || solid.MaximumPoint == null)
       {
           var zero = new Tekla.Structures.Geometry3d.Point(0, 0, 0);
           return new Box(zero, zero);
       }
       return new Box(solid.MinimumPoint, solid.MaximumPoint);
   }
   ```

---

## 8. Proteger `Clipboard.SetText` contra falha

**Arquivo modificado:** `src/40_ReportWindow.cs`

### Passo a passo

1. No click handler do botao "Copiar" (linha 30), envolver com try/catch:
   ```csharp
   copyButton.Click += delegate {
       try
       {
           Clipboard.SetText(textBox.Text);
       }
       catch
       {
           MessageBox.Show("Nao foi possivel copiar. Tente novamente.");
       }
   };
   ```

---

## 9. Corrigir indentacao em `MenuUi`

**Arquivo modificado:** `src/20_MenuUi.cs`

### Passo a passo

1. Alinhar as linhas 129-130:
   ```diff
   -var btnSelectParts = CriarBotaoDashboard("Selecionar pecas", false);
   -btnSelectParts.MinimumSize = new Size(200, 36);
   +            var btnSelectParts = CriarBotaoDashboard("Selecionar pecas", false);
   +            btnSelectParts.MinimumSize = new Size(200, 36);
   ```

---

## 10. Corrigir paths no `build_macro.ps1`

**Arquivo modificado:** `scripts/build_macro.ps1`

### Passo a passo

1. Nas linhas 2-5, substituir `\\\\` por `\`:
   ```diff
   -    [string]$SourceDir = (Join-Path $PSScriptRoot "..\\\\src"),
   -    [string]$OutputMacro = (Join-Path $PSScriptRoot "..\\\\macros\\\\MarnaTeklaOS.cs"),
   +    [string]$SourceDir = (Join-Path $PSScriptRoot "..\src"),
   +    [string]$OutputMacro = (Join-Path $PSScriptRoot "..\macros\MarnaTeklaOS.cs"),
   ```
2. Repetir para `$OutputTsep` e `$OutputTekla`.
3. Corrigir tambem os caminhos nas linhas 30-31 do comentario `<auto-generated>`.

---

## 11. Atualizar README.md e CODEX.md

**Arquivos modificados:** `README.md`, `CODEX.md`

### Passo a passo

1. Em `README.md`, secao "Arquitetura em src/", adicionar:
   ```
   - `src/15_DesignSystem.cs`: constantes de cor, fonte e helpers de criacao de controles.
   - `src/25_ModelHelper.cs`: conexao ao modelo e leitura de report properties.
   - `src/70_AssemblySelectionHelper.cs`: selecao inteligente de conjuntos por nome.
   - `src/80_AssemblyComparator.cs`: comparacao lado a lado de dois conjuntos.
   ```
2. Em `README.md`, secao "Visao geral", adicionar as funcionalidades de selecao e comparacao.
3. Em `CODEX.md`, secao 3, adicionar os novos arquivos.

---

## Ordem recomendada de execucao

| Etapa | Melhoria | Risco | Dependencia |
|-------|----------|-------|-------------|
| 1 | 9. Corrigir indentacao | Nenhum | - |
| 2 | 10. Corrigir paths do build | Nenhum | - |
| 3 | 8. Proteger Clipboard | Baixo | - |
| 4 | 7. Proteger GetSolid | Baixo | - |
| 5 | 3. Criar ModelHelper (conexao) | Medio | - |
| 6 | 4. Consolidar GetReportProperty | Medio | Etapa 5 |
| 7 | 1. Extrair DesignSystem | Medio | - |
| 8 | 2. Aplicar DesignSystem a ReportWindow | Medio | Etapa 7 |
| 9 | 5. Cursor de espera nos botoes | Baixo | - |
| 10 | 6. Botao [?] no Comparar | Baixo | - |
| 11 | 11. Atualizar docs | Nenhum | Todas |

> Apos cada etapa, rodar `scripts/build_macro.ps1` para validar a compilacao.
