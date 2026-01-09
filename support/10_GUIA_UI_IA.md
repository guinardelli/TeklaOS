# GUIA DE UI PARA ASSISTENTES DE IA (TeklaOS)

Este documento define as diretrizes estritas para a geração de código de Interface de Usuário (UI) para o projeto TeklaOS.
**Contexto:** O projeto é uma Macro C# para Tekla Structures (Single File Script).
**Restrição Crítica:** Não existe Designer Visual (`.resx`, `.Designer.cs`). Toda a UI é construída via código (Programmatically) dentro de `src/20_MenuUi.cs`.

---

## 1. O "Design System" (Variáveis Obrigatórias)

Ao gerar novos controles, **NUNCA** use cores hardcoded (ex: `Color.Blue`, `Color.White`). Utilize **sempre** as constantes definidas no topo da classe `MenuUi`.

### Paleta de Cores
* `C_FundoForm` (Cinza Escuro): Fundo de formulários e painéis principais.
* `C_Cabecalho` (Cinza Médio): Fundo de painéis de título/topo.
* `C_Texto` (Branco/Cinza Claro): Cor da fonte principal.
* `C_DestaqueAzul` (Azul Tekla): Botões primários, bordas de foco.
* `C_Borda` (Cinza): Linhas separadoras.
* `C_BotaoHover` (Azul Claro/Cinza): Estado visual ao passar o mouse.

### Tipografia
* `F_Titulo`: Segoe UI, 12pt, Bold.
* `F_Texto`: Segoe UI, 10pt, Regular.
* `F_Icone`: (Se aplicável) Fontes de ícones ou emojis.

---

## 2. Regras de Construção de Layout

Como a IA não "vê" a tela, siga estas regras para evitar sobreposição de controles:

1.  **Evite Coordenadas Absolutas:** Não use `new Point(120, 50)` a menos que estritamente necessário.
2.  **Use Containers de Layout:**
    * `FlowLayoutPanel`: Para listas de botões ou campos (vertical ou horizontal).
    * `TableLayoutPanel`: Para formulários alinhados (Label na col 0, TextBox na col 1).
    * `Panel` com `Dock`: Use `Dock = DockStyle.Top` ou `Fill` para organizar as seções principais.
3.  **Padding e Margin:** Sempre defina `Padding` nos containers e `Margin` nos controles para evitar que fiquem "colados".

---

## 3. Templates de Código (Snippet "Golden Path")

Use este padrão ao criar botões para manter a consistência visual do TeklaOS:

### Criar um Botão Padrão
```csharp
Button btnAcao = new Button();
btnAcao.Text = "Executar Ação";
btnAcao.FlatStyle = FlatStyle.Flat;
btnAcao.FlatAppearance.BorderSize = 0;
btnAcao.BackColor = C_DestaqueAzul; // Ou C_FundoForm para botões secundários
btnAcao.ForeColor = C_Texto;
btnAcao.Font = F_Texto;
btnAcao.Height = 30;
btnAcao.Cursor = Cursors.Hand;
btnAcao.Click += (sender, e) => {
    // Lógica aqui (preferencialmente chamando método externo)
    TeklaCommands.ExecutarMinhaAcao();
};
// Adicionar ao container pai
painelPrincipal.Controls.Add(btnAcao);

Criar uma Seção com Título

// 1. Container da Seção
Panel pnlSecao = new Panel();
pnlSecao.Dock = DockStyle.Top;
pnlSecao.Height = 100; // Ajustar conforme conteúdo
pnlSecao.Padding = new Padding(5);

// 2. Título da Seção
Label lblTitulo = new Label();
lblTitulo.Text = "Configurações Avançadas";
lblTitulo.Font = F_Titulo;
lblTitulo.ForeColor = C_Texto;
lblTitulo.Dock = DockStyle.Top;
lblTitulo.Height = 25;

// 3. Adicionar ao painel
pnlSecao.Controls.Add(lblTitulo);
// Adicionar outros controles...
form.Controls.Add(pnlSecao);

4. Tratamento de Imagens e Ícones
NÃO tente carregar arquivos do disco (Image.FromFile). O script deve ser portátil.

Se precisar de um ícone novo: Peça ao usuário a string Base64 do ícone.

Se for usar existente: Use Assets.LogoPngBase64 (exemplo).

Código de Carregamento Seguro:

try {
    byte[] imageBytes = Convert.FromBase64String(STRING_BASE64);
    using (var ms = new MemoryStream(imageBytes)) {
        pictureBox.Image = Image.FromStream(ms);
    }
} catch { 
    // Falha silenciosa ou placeholder
}

5. Instruções para a IA (Checklist)
Antes de gerar o código final, verifique:

[ ] Estou usando as variáveis C_... e F_... em vez de cores fixas?

[ ] O código está usando using para descartar recursos gráficos (Pens, Brushes) se criar GDI+ customizado?

[ ] A lógica de negócio (Tekla API) está separada da lógica de UI (Evento Click)?

[ ] O layout é fluido (Dock / Flow) ou vai quebrar se o usuário redimensionar a janela?

6. Licoes aprendidas (refactor MenuUi -> Dashboard)
- Preferir TableLayoutPanel como container principal para evitar ajustes manuais de largura.
- Agrupar funcoes em grupos visuais (ex.: Relatorios, Selecao, Acoes) reduz carga cognitiva.
- Header e footer devem ser fixos; conteudo central pode ser scrollable.
- Evitar cores hardcoded; criar constantes C_... e F_... para manter consistencia.
- Buttons devem seguir um unico helper para padronizar hover, padding e borda.
- Input e labels devem respeitar Padding/Margin para nao colar no container.
- Evitar ajustar largura via SizeChanged quando o layout pode resolver sozinho.
- Editar UI em src/*.cs e gerar o macro; nao edite macros/*.cs diretamente.
