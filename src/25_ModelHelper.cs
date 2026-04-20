internal static class ModelHelper
{
    public static Model GetConnectedModel()
    {
        var model = new Model();
        if (!model.GetConnectionStatus())
        {
            MessageBox.Show("Nao foi possivel conectar ao modelo Tekla. Abra um modelo e tente novamente.");
            return null;
        }
        return model;
    }

    public static string GetReportProperty(ModelObject obj, string propertyName)
    {
        if (obj == null) return "-";

        string stringValue = null;
        if (obj.GetReportProperty(propertyName, ref stringValue))
        {
            return Formatters.FormatValue(stringValue);
        }

        double doubleValue = 0.0;
        if (obj.GetReportProperty(propertyName, ref doubleValue))
        {
            return Formatters.FormatValue(string.Format("{0:F1}", doubleValue));
        }

        int intValue = 0;
        if (obj.GetReportProperty(propertyName, ref intValue))
        {
            return intValue.ToString();
        }

        return "-";
    }
}
