5. Desenvolvimento de Plugins e Custom Components
Para ferramentas de utilitários que aparecem no catálogo lateral:

Estrutura de Dados (Data Transfer)
A classe de dados mapeia os campos do WinForms para o Plugin.

public class StructuresData
{
    [StructuresField("CheckQuantity")] // Nome deve bater com o Attribute no Form
    public int CheckQuantity;
    
    [StructuresField("Comment")]
    public string Comment;
}

Direct Manipulation (Novidade Recente)
Em 2023, o uso de DirectManipulationFeature permite criar gráficos interativos na tela (Grips) para controlar seus plugins, além da Dialog padrão.