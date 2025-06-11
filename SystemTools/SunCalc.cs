namespace SystemTools;

public static class SunCalc
{
    public static DateTime GetSunrise(DateTime date, double lat, double lng) => date.Date.AddHours(6.0);
    public static DateTime GetSunset(DateTime date, double lat, double lng) => date.Date.AddHours(20.0);
}