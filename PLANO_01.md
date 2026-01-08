# Plano 01 - Ajustes de UI TeklaOS

## Objetivo
- Atualizar textos do menu principal.
- Reorganizar os botoes em coluna unica.
- Criar dois quadros (GroupBox) para separar o botao de reparo.

## Arquivos alvo
- `src/20_MenuUi.cs`
- `README.md` (atualizar nomes/textos se a UI mudar)

## Passos
1) Atualizar textos do formulario
   - `form.Text`: "TeklaOS".
   - `titleLabel.Text`: "Sistema Marna Pre-fabricados x Tekla2023".
   - `infoLabel.Text`: "Sistema de utilitarios MarnaTeklaOS versao 1.0."

2) Reorganizar layout dos botoes para coluna unica
   - Substituir o `FlowLayoutPanel` atual por um container vertical (ex.: `FlowLayoutPanel` TopDown ou `TableLayoutPanel` 1 coluna).
   - Criar `GroupBox` "Limpeza e correcao do modelo" e mover `repairButton` para dentro.
   - Criar `GroupBox` "teste" e colocar os demais controles (run, selection, transparencia, fechar).
   - Garantir espacamentos consistentes (margins/padding) e `AutoSize` quando fizer sentido.

3) Ajustar tamanho do formulario e layout
   - Revisar `form.Size`/`form.MinimumSize` para acomodar as duas caixas empilhadas.
   - Verificar ancoras/dock para manter o layout estavel.

4) Validacao local
   - Gerar macro via `scripts/build_macro.ps1`.
   - Abrir no Tekla e confirmar textos, ordem dos controles e alinhamento vertical.

## Perguntas/confirmacoes
- Os textos devem manter acentos? O padrao do projeto pede ASCII sem acentos.
- O nome do quadro "teste" eh definitivo ou provisiorio?
- O checkbox "Modo transparente" deve ficar dentro do quadro "teste" junto dos botoes?
