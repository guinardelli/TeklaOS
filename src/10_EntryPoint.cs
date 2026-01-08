public class Script
{
    [Tekla.Macros.Runtime.MacroEntryPointAttribute()]
    public static void Run(Tekla.Macros.Runtime.IMacroRuntime runtime)
    {
        MenuUi.Show(runtime);
    }
}
