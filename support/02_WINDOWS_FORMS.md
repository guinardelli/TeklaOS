## 2. Configuração de Projeto C# WinForms

### Configuração do .csproj
* **Target Framework**: .NET Framework 4.8 (Padrão para Tekla 2023).
* **Plataforma**: `AnyCPU` ou `x64`.

### Referências NuGet/Locais
Recomenda-se usar as DLLs localizadas em `C:\TeklaStructures\2023.0\bin\plugins` ou via pacote NuGet oficial `Tekla.Structures.Model` versão `2023.0.x`.

### Estrutura Básica de Plugin com Dialog
```csharp
using Tekla.Structures.Plugins;

[Plugin("MeusUtilitarios")] // Nome interno
[PluginUserInterface("MeusUtilitarios.MainForm")] // Namespace.Classe do Form
public class MeuPlugin : PluginBase
{
    public override List<InputDefinition> DefineInput()
    {
        // Define entradas se necessário (ex: pick points)
        return new List<InputDefinition>();
    }

    public override bool Run(List<InputDefinition> Input)
    {
        // Lógica principal executada ao clicar "Criar" ou aplicar
        return true;
    }
}