public class Script
{
    [Tekla.Macros.Runtime.MacroEntryPointAttribute()]
    public static void Run(Tekla.Macros.Runtime.IMacroRuntime runtime)
    {
        try
        {
            MenuUi.Show(runtime);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ocorreu um erro inesperado: " + ex.Message, "Erro Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
