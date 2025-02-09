// |¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯|
// |2025   ---        -<CODE BY BLAYMS>-        ---   2025|
// |______________________________________________________|

using System.Globalization;
/// <summary>
/// A class that contains in-real-time data about the International Space Station (ISS). Fully supported by open-notify.org! Code by Blayms!
/// </summary>
public static class InternationalSpaceStation
{
    /// <summary>
    /// The status of ISS data fetching
    /// </summary>
    public static Task<string> Message => GetMessage();
    /// <summary>
    /// The Y axis (height) from ISS location
    /// </summary>
    public static Task<float> Latitude => GetLatitude();
    /// <summary>
    /// The X axis (horizontal) from ISS location
    /// </summary>
    public static Task<float> Longitude => GetLongitude();
    /// <summary>
    /// Timestamp in Unix format with UTC+0 timezone
    /// </summary>
    public static Task<int> TimestampUnix => GetTimestamp();
    /// <summary>
    /// Creates and returns <see cref="System.DateTime"></see> from <see cref="TimestampUnix"></see>
    /// </summary>
    /// <param name="useLocalTime">Determines if conversion to your local time zone is necessary</param>
    /// <returns></returns>
    public static async Task<DateTime> GetTimestampAsObject(bool useLocalTime)
    {
        int unix = await GetTimestamp();

        return UnixTimeStampToDateTime(unix, useLocalTime);
    }

    private static async Task<string> GetMessage()
    {
        var tuple = await RequestOpenNotifyAPI();
        return tuple.Item1;
    }

    private static async Task<float> GetLatitude()
    {
        var tuple = await RequestOpenNotifyAPI();
        return float.Parse(tuple.Item2, CultureInfo.InvariantCulture);
    }

    private static async Task<float> GetLongitude()
    {
        var tuple = await RequestOpenNotifyAPI();
        return float.Parse(tuple.Item3, CultureInfo.InvariantCulture);
    }

    private static async Task<int> GetTimestamp()
    {
        var tuple = await RequestOpenNotifyAPI();
        return int.Parse(tuple.Item4);
    }
    private static DateTime UnixTimeStampToDateTime(int unixTimeStamp, bool useLocalTime)
    {
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = useLocalTime ? dateTime.AddSeconds(unixTimeStamp).ToLocalTime() : dateTime.AddSeconds(unixTimeStamp);
        return dateTime;
    }

    private static async Task<Tuple<string, string, string, string>> RequestOpenNotifyAPI()
    {
        string url = "http://api.open-notify.org/iss-now.json";

        using HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            string data = await response.Content.ReadAsStringAsync();
            // Extracting fields from JSON manually
            string message = ExtractJsonValue(data, "message");
            string latitude = ExtractJsonValue(data, "latitude");
            string longitude = ExtractJsonValue(data, "longitude");
            string timestamp = ExtractJsonValue(data, "timestamp");
            return new Tuple<string, string, string, string>(message, latitude, longitude, timestamp);
        }
        return new Tuple<string, string, string, string>("N/A", "N/A", "N/A", "N/A");
    }
    private static string ExtractJsonValue(string json, string key)
    {
        string searchKey = $"\"{key}\"";
        int keyIndex = json.IndexOf(searchKey);

        if (keyIndex == -1)
            return "N/A";

        int valueStart = json.IndexOf(":", keyIndex) + 1;

        if (json[valueStart] == '{')
        {
            return "OBJECT";
        }

        int valueEnd = json.IndexOf(",", valueStart);
        if (valueEnd == -1) valueEnd = json.IndexOf("}", valueStart);

        string value = json.Substring(valueStart, valueEnd - valueStart).Trim();
        return value.Trim('"');
    }
}
