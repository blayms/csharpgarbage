// |¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯|
// |2025   ---        -<CODE BY BLAYMS>-        ---   2025|
// |______________________________________________________|

/// <summary>
/// A class that contains images bytes of an AI generated image of a human. Fully supported by thispersondoesnotexist.com! Code by Blayms!
/// </summary>
public static class ThisPersonDoesNotExist
{
    public static async Task<byte[]?> RequestAPI()
    {
        string url = "https://thispersondoesnotexist.com/";

        using HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            byte[] data = await response.Content.ReadAsByteArrayAsync();
            return data;
        }
        return default;
    }
}
