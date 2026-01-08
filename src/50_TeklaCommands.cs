internal static class TeklaCommands
{
    public static void RunModelRepair(Tekla.Macros.Runtime.IMacroRuntime runtime)
    {
        if (runtime == null)
        {
            MessageBox.Show("Nao foi possivel acessar o runtime da macro.");
            return;
        }

        Tekla.Macros.Akit.IAkitScriptHost akit = runtime.Get<Tekla.Macros.Akit.IAkitScriptHost>();
        if (akit == null)
        {
            MessageBox.Show("Nao foi possivel acessar o Akit.");
            return;
        }

        akit.Callback("acmd_check_database", "1", "main_frame");
        // Repara o banco de dados de bibliotecas (xslib) apos o reparo do modelo.
        akit.Callback("acmd_check_database", "XS_LIB_CORRECT", "main_frame");
    }
}
