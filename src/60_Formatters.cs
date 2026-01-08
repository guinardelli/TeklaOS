internal static class Formatters
{
    public static string FormatValue(string value)
    {
        return string.IsNullOrEmpty(value) ? "-" : value;
    }

    public static string FormatValue(object value)
    {
        return value == null ? "-" : value.ToString();
    }

    public static string FormatBool(bool value)
    {
        return value ? "Sim" : "Nao";
    }

    public static string FormatPhaseNumber(ModelObject modelObject)
    {
        if (modelObject == null)
        {
            return "-";
        }

        Phase phase;
        if (modelObject.GetPhase(out phase))
        {
            return phase.PhaseNumber.ToString();
        }

        return "-";
    }

    public static string FormatDate(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "-";
        }

        DateTime parsed;
        if (DateTime.TryParse(value, out parsed))
        {
            return parsed.ToString("yyyy-MM-dd");
        }

        return value;
    }
}
