# TeklaOS - Sistema de utilitarios MarnaTeklaOS (macro + TSEP)

Este projeto contem uma macro para Tekla Structures 2023 e a estrutura de um pacote TSEP para distribuicao. O foco e oferecer um sistema de utilitarios com relatorios do modelo e ferramentas de manutencao, integrados ao Tekla por meio do Applications & Components.

## Visao geral das funcionalidades

- Menu principal (WinForms) com botoes organizados em dois grupos:
  - "Relatorios e Acoes":
  - "Gerar relatorio": mostra informacoes do modelo e do projeto.
  - "Pecas selecionadas": mostra informacoes das pecas atualmente selecionadas.
  - "Selecionar pecas": digite os nomes dos conjuntos separados por virgula (ex.: PP1,PP2,VR1) e o sistema seleciona esses conjuntos diretamente no modelo.
  - "Fechar": fecha o menu.
  - "Limpeza e correcao do modelo":
    - "Reparar modelo e banco de dados": aciona o comando nativo "Diagnosticar e corrigir -> Reparar Modelo".
- Janela de relatorio:
  - Exibe o texto completo com scroll e fonte monoespacada.
  - Botao "Copiar" para enviar o texto para a area de transferencia.
- Modo transparente:
  - O menu principal possui um checkbox na parte inferior para alternar opacidade (0.85/1.0).
  - O menu fica sempre por cima (TopMost), para nao se perder atras do Tekla.

## Como a integracao com o Tekla funciona

1) Macro como ponto de entrada
- Arquivo principal (gerado por `scripts/build_macro.ps1`): `MarnaTeklaOS.cs`.
- Usa o runtime de macros do Tekla:
  - `#pragma reference "Tekla.Macros.Runtime"`
  - `#pragma reference "Tekla.Macros.Akit"`
  - Metodo de entrada:
    ```csharp
    [Tekla.Macros.Runtime.MacroEntryPointAttribute()]
    public static void Run(Tekla.Macros.Runtime.IMacroRuntime runtime)
    ```

2) Acesso ao modelo e UI do Tekla
- Dados do modelo:
  - `Model.GetInfo()` (ModelInfo)
  - `Model.GetProjectInfo()` (ProjectInfo)
- Objetos selecionados:
  - `Tekla.Structures.Model.UI.ModelObjectSelector.GetSelectedObjects()`
- Fase da peca:
  - `modelObject.GetPhase(out Phase phase)`

3) Comando de reparo via Akit
- O botao "REPARAR MODELO" usa Akit:
  - `akit.Callback("acmd_check_database", "1", "main_frame");`
- Isso abre o fluxo de "Reparar Modelo" no Tekla.

## Arquitetura em src/

- `src/00_Header.cs`: pragmas e `using` do macro gerado.
- `src/10_EntryPoint.cs`: entry point `Script.Run`.
- `src/20_MenuUi.cs`: cria o menu com botoes e checkbox.
- `src/30_ReportBuilder.cs`: monta relatorio do modelo e pecas selecionadas.
- `src/40_ReportWindow.cs`: janela de relatorio (TextBox + copiar).
- `src/50_TeklaCommands.cs`: dispara o comando de reparo no Tekla.
- `src/60_Formatters.cs`: formatadores e helpers.
- `scripts/build_macro.ps1`: gera `macros/MarnaTeklaOS.cs` e a copia do TSEP.

## Pastas importantes no projeto

Projeto (repo):
- `src/` (codigo fonte principal, dividido por arquivos)
- `macros/MarnaTeklaOS.cs` (macro gerada no repo)
- `tsep/RelatorioModelo/` (estrutura do TSEP)
- `tsep/RelatorioModelo/Environments/common/macros/modeling/MarnaTeklaOS.cs` (copia para o TSEP)
- `tsep/RelatorioModelo/Environments/common/extensions/RelatorioModelo/RelatorioModelo_ComponentCatalog.ac.xml` (botao no A&C)
- `tsep/RelatorioModelo/Manifest.xml` (definicao do pacote)

Instalacao Tekla (local de execucao):
- `C:\TeklaStructures\2023.0\Environments\common\macros\modeling\MarnaTeklaOS.cs`
- `C:\TeklaStructures\2023.0\Environments\common\extensions\RelatorioModelo\RelatorioModelo_ComponentCatalog.ac.xml`

Observacao importante:
- Existem tres copias do `MarnaTeklaOS.cs`. Para distribuir, a copia dentro do TSEP precisa estar atualizada.
- A fonte verdadeira fica em `src/`; gere as copias com `scripts/build_macro.ps1`.

## Como o botao aparece no Tekla

O botao e criado por um catalogo de componentes:
- `RelatorioModelo_ComponentCatalog.ac.xml`
- Tipo: `CatalogMacroModelingItem`
- O `ItemIdString` precisa usar o nome do arquivo da macro sem extensao:
  - `CatalogMacroModelingItem?MarnaTeklaOS?GLOBAL`

No Tekla, o botao aparece no painel **Applications & Components** no grupo `TeklaOS`.

## TSEP (empacotamento)

- O pacote e descrito em `tsep/RelatorioModelo/Manifest.xml`.
- O `Product` precisa de `Type="Extension"`.
- Saida configurada para pasta local gravavel:
  - `TepOutputFolder = C:\TeklaTSEPOutput`

Gerar o TSEP:
- Usar o `TeklaExtensionPackage.Builder.exe` ou `TeklaExtensionPackage.Manager.exe`.
- O arquivo final sai em `C:\TeklaTSEPOutput\RelatorioModelo_1.0.tsep`.

## Fluxo recomendado de desenvolvimento

- Para testar rapido:
  - Editar os arquivos em `src/`.
  - Rodar `scripts/build_macro.ps1 -UpdateTekla` para atualizar a macro no Tekla.
  - Executar pelo botao do A&C.
- Para distribuir:
  - Rodar `scripts/build_macro.ps1` para atualizar a copia do TSEP.
  - Gerar um novo `.tsep`.

## Notas tecnicas importantes

- O compilador de macros do Tekla nao suporta interpolacao de string (`$"..."`).
- Use `string.Format` ou concatenacao simples.
- `ProjectInfo.StartDate` e `EndDate` sao strings no Tekla 2023.

## Instrucoes para agente de IA

- Consulte `CLAUDE.md` para o fluxo oficial de geracao de codigo e principios de design.
