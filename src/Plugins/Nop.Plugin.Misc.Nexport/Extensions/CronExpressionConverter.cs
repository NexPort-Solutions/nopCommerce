namespace Nop.Plugin.Misc.Nexport.Extensions;

public static class CronExpressionConverter
{
    public static string ConvertToCronExpression(this TimeSpan periodRecurrence)
    {
        if (periodRecurrence.Hours >= 1)
        {
            if (periodRecurrence.Minutes > 1)
            {
                return periodRecurrence.Seconds > 1
                    ? $"*/{periodRecurrence.Seconds} */{periodRecurrence.Minutes} */{periodRecurrence.Hours} * * *"
                    : $"* */{periodRecurrence.Minutes} */{periodRecurrence.Hours} * * *";
            }

            return periodRecurrence.Hours == 1
                ? "0 * * * *"
                : $"* */{periodRecurrence.Hours} * * *";
        }

        if (periodRecurrence.Minutes >= 1)
        {
            if (periodRecurrence.Seconds > 1)
                return $"*/{periodRecurrence.Seconds} */{periodRecurrence.Minutes} * * * *";

            return periodRecurrence.Minutes == 1
                ? "* * * * *"
                : $"* */{periodRecurrence.Minutes} * * * *";
        }

        return periodRecurrence.Seconds > 1 ? $"*/{periodRecurrence.Seconds} * * * * *" : "*/1 * * * * *";
    }
}