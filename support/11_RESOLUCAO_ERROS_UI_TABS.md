# Resolucao de Erros - UI com Guias (Tabs) no Tekla Macro

Este documento registra a resolucao dos erros que ocorreram ao introduzir layout com abas (`TabControl`) na macro.

## Contexto

Durante a evolucao de UI/UX (separacao visual por guias e cores), o Tekla exibiu erros de compilacao no arquivo macro gerado:

- `CS0104`: `Point` ambiguo entre `System.Drawing.Point` e `Tekla.Structures.Geometry3d.Point`
- `CS1628`: nao e permitido usar parametro `ref/out` dentro de metodo anonimo/lambda
- `CS0162`: codigo inacessivel detectado (efeito secundario)
- warning `Akit`: referencia de assembly em runtime

## Causa Raiz e Correcao

### 1) CS0104 - Tipo ambiguo `Point`

**Causa:**  
No projeto, existem referencias de desenho (`System.Drawing`) e de geometria Tekla (`Tekla.Structures.Geometry3d`).  
Ao usar `new Point(...)` no `TabControl.Padding`, o compilador da macro ficou ambiguo.

**Correcao aplicada:**  
Usar tipo totalmente qualificado:

```csharp
tabs.Padding = new System.Drawing.Point(14, 4);
```

### 2) CS1628 - Uso de `out` dentro de lambda

**Causa:**  
No metodo:

```csharp
private static TabPage CreateTabPageWithLayout(..., out TableLayoutPanel layout)
```

o parametro `layout` era usado dentro de:

```csharp
scrollPanel.SizeChanged += delegate { ... layout ... };
```

O compilador do ambiente de macro do Tekla nao aceita esse padrao.

**Correcao aplicada:**  
Criar variavel local e usar a local na lambda; atribuir ao `out` apenas no final:

```csharp
var contentLayout = DesignSystem.CriarLayoutVertical();
scrollPanel.SizeChanged += delegate { contentLayout.Width = ...; };
layout = contentLayout;
```

### 3) CS0162 - Codigo inacessivel

**Observacao:**  
Este warning apareceu como efeito cascata da falha de compilacao anterior.  
Apos corrigir `CS0104` e `CS1628`, o warning deixou de ser bloqueador.

### 4) Warning de referencia `Akit`

**Observacao:**  
Warning comum de referencia em tempo de execucao no ambiente de macro Tekla.
Nao bloqueia a geracao/execucao da macro quando o restante compila corretamente.

## Checklist Rapido para evitar regressao

1. Sempre qualificar tipos potencialmente ambiguos (`System.Drawing.Point`, etc.).
2. Evitar usar parametros `ref/out` diretamente dentro de lambdas/delegates.
3. Se necessario, usar variavel local intermediaria para eventos (`SizeChanged`, `Click`, etc.).
4. Regerar macro com `scripts/build_macro.ps1` apos qualquer mudanca de UI.
5. Validar tambem os arquivos gerados:
   - `macros/MarnaTeklaOS.cs`
   - `tsep/.../MarnaTeklaOS.cs`

