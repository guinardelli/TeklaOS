# Instrucoes para agente de IA

Este arquivo orienta como gerar e manter o codigo deste projeto TeklaOS.

## Objetivo do projeto
- Macro Tekla Structures (WinForms) que gera relatorios do modelo e oferece comandos de manutencao.
- Distribuicao por TSEP (Applications & Components).

## Fluxo de desenvolvimento (obrigatorio)
- Edite apenas os arquivos em `src/`.
- Gere a macro com `scripts/build_macro.ps1`.
- Para atualizar o Tekla instalado, use `scripts/build_macro.ps1 -UpdateTekla`.
- Para distribuir, gere o TSEP apos atualizar a copia em `tsep/`.
- Nao edite diretamente `macros/MarnaTeklaOS.cs` nem `tsep/.../MarnaTeklaOS.cs` (sao gerados).

## Arquitetura e arquivos gerados
- Fonte real: `src/*.cs`.
- Cabecalho e referencias ficam em `src/00_Header.cs`.
- Entry point unico em `src/10_EntryPoint.cs` com `Script.Run`.
- Gerados:
  - `macros/MarnaTeklaOS.cs`
  - `tsep/RelatorioModelo/Environments/common/macros/modeling/MarnaTeklaOS.cs`
  - `C:\TeklaStructures\2023.0\Environments\common\macros\modeling\MarnaTeklaOS.cs` (quando usar `-UpdateTekla`)

## Regras do runtime de macros Tekla
- Deve existir apenas um `MacroEntryPointAttribute` em `Script.Run`.
- Nao use interpolacao de string (`$"..."`); use `string.Format`.
- Mantenha `#pragma reference "Tekla.Macros.Runtime"` e `#pragma reference "Tekla.Macros.Akit"`.
- Use `System.Windows.Forms` para UI e mensagens.

## Design e qualidade
Aplicar sempre:
- DRY: evitar duplicacao de codigo e de regras de formatacao.
- KISS: manter estruturas simples e claras.
- YAGNI: implementar apenas o necessario.
- SOLID:
  - SRP: cada classe tem responsabilidade unica.
  - OCP: estender sem modificar quando possivel.
  - LSP: substituicoes nao devem quebrar comportamento.
  - ISP: interfaces pequenas e especificas.
  - DIP: depender de abstracoes quando fizer sentido.

## Conteudo e idioma
- Manter mensagens e textos em portugues (sem acentos, ASCII).
- Atualizar `README.md` quando mudar fluxo, nomes ou estrutura.

## Catalogo do Tekla (A&C)
- Se o nome da macro mudar:
  - Atualize `MacroFileName` e `ItemIdString` em `tsep/RelatorioModelo/Environments/common/extensions/RelatorioModelo/RelatorioModelo_ComponentCatalog.ac.xml`.
  - Atualize o catalogo instalado em `C:\TeklaStructures\2023.0\Environments\common\extensions\RelatorioModelo\RelatorioModelo_ComponentCatalog.ac.xml`.
