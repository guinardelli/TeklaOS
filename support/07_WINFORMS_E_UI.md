7. Integração WinForms e UI
Para integrar nativamente com o Tekla, seu Form deve herdar de PluginFormBase (namespace Tekla.Structures.Dialog).

Binding Automático
O Tekla faz o binding automático entre controles WinForms e a classe StructuresData através da propriedade Tag ou nomes de atributos, dependendo da implementação base.

Pickers (Interação com Usuário)
Botões no WinForms podem acionar "Pickers" no modelo.

private void btnPickPoint_Click(object sender, EventArgs e)
{
    Tekla.Structures.Model.UI.Picker picker = new Tekla.Structures.Model.UI.Picker();
    try 
    {
        // Solicita ao usuário clicar em um ponto no 3D
        Tekla.Structures.Geometry3d.Point point = picker.PickPoint("Por favor, clique em um ponto de origem.");
        
        // Atualiza UI com coordenadas
        txtX.Text = point.X.ToString();
    }
    catch (Exception) { /* Usuário cancelou */ }
}