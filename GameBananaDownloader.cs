// |¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯|
// |2025   ---        -<CODE BY BLAYMS>-        ---   2025|
// |______________________________________________________|

using System.Buffers;
using System.Text.Json;

/// <summary>
/// A class that holds GameBanana file metadata. Fully supported by gamebanana.com! Code by Blayms!
/// </summary>
public readonly struct GameBananaFile
{
    /// <summary>
    /// The unique identifier of the file on GameBanana
    /// </summary>
    public readonly int IdRow;

    /// <summary>
    /// The name of the file including its extension
    /// </summary>
    public readonly string FileName;

    /// <summary>
    /// The size of the file in bytes
    /// </summary>
    public readonly int FileSize;

    /// <summary>
    /// The description of the file provided by the uploader
    /// </summary>
    public readonly string Description;

    /// <summary>
    /// The Unix timestamp representing when the file was added to GameBanana
    /// </summary>
    public readonly long DateAddedLong;

    /// <summary>
    /// The number of times this file has been downloaded
    /// </summary>
    public readonly int DownloadCount;

    /// <summary>
    /// The URL used to download this file
    /// </summary>
    public readonly string DownloadUrl;
    /// <summary>
    /// Gets the date and time when the file was added, converted from Unix timestamp
    /// </summary>
    public DateTime DateAdded => DateTimeOffset.FromUnixTimeSeconds(DateAddedLong).DateTime;

    /// <summary>
    /// Initializes a new instance of the GameBananaFile struct with the specified parameters
    /// </summary>
    /// <param name="idRow">The unique identifier of the file</param>
    /// <param name="fileName">The name of the file</param>
    /// <param name="fileSize">The size of the file in bytes</param>
    /// <param name="description">The description of the file</param>
    /// <param name="dateAdded">Unix timestamp of when the file was added</param>
    /// <param name="downloadCount">Number of times the file was downloaded</param>
    /// <param name="downloadUrl">URL to download the file</param>
    public GameBananaFile(int idRow, string fileName, int fileSize, string description,
                         long dateAdded, int downloadCount, string downloadUrl)
    {
        IdRow = idRow;
        FileName = fileName;
        FileSize = fileSize;
        Description = description;
        DateAddedLong = dateAdded;
        DownloadCount = downloadCount;
        DownloadUrl = downloadUrl;
    }

    public override string ToString()
    {
        return $"{nameof(GameBananaFile)}(IdRow: {IdRow}, FileName: \"{FileName}\", FileSize: {FileSize}, Description: \"{Description}\", DateAdded: {DateAddedLong}, DownloadCount: {DownloadCount}, DownloadUrl: \"{DownloadUrl}\")";
    }
}
/// <summary>
/// A class that helps to download files from GameBanana. Fully supported by gamebanana.com! Code by Blayms!
/// </summary>
public static class GameBananaDownloader
{
    private static readonly HttpClient _httpClient;
    private static readonly ArrayPool<GameBananaFile> _arrayPool = ArrayPool<GameBananaFile>.Shared;

    static GameBananaDownloader()
    {
        _httpClient = new HttpClient(new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(1),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
            MaxConnectionsPerServer = 10
        })
        {
            DefaultRequestHeaders =
            {
                { "User-Agent", "GameBanana Downloader" }
            }
        };
    }
    /// <summary>
    /// Asynchronously gets all downloadable files from a specified mod ID, can be later use in <see cref="DownloadFileAsync(GameBananaFile, string?)"/>
    /// </summary>
    /// <returns></returns>
    public static async Task<GameBananaFile[]> GetModFilesAsync(int modId)
    {
        string apiUrl = $"https://api.gamebanana.com/Core/Item/Data?itemtype=Mod&itemid={modId}&fields=Files().aFiles()";

        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(apiUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            await using Stream stream = await response.Content.ReadAsStreamAsync();
            return await ParseResponseOptimizedAsync(stream);
        }
        catch
        {
            return Array.Empty<GameBananaFile>();
        }
    }

    private static async Task<GameBananaFile[]> ParseResponseOptimizedAsync(Stream stream)
    {
        using JsonDocument doc = await JsonDocument.ParseAsync(stream);
        JsonElement root = doc.RootElement;

        if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
        {
            return Array.Empty<GameBananaFile>();
        }

        JsonElement filesObject = root[0];
        if (filesObject.ValueKind != JsonValueKind.Object)
        {
            return Array.Empty<GameBananaFile>();
        }

        int estimatedSize = 0;
        foreach (var _ in filesObject.EnumerateObject())
        {
            estimatedSize++;
        }

        GameBananaFile[] files = _arrayPool.Rent(estimatedSize);
        int index = 0;

        try
        {
            foreach (var fileProperty in filesObject.EnumerateObject())
            {
                var fileElement = fileProperty.Value;

                files[index++] = new GameBananaFile(
                    fileElement.GetProperty("_idRow").GetInt32(),
                    fileElement.GetProperty("_sFile").GetString() ?? string.Empty,
                    fileElement.GetProperty("_nFilesize").GetInt32(),
                    fileElement.GetProperty("_sDescription").GetString() ?? string.Empty,
                    fileElement.GetProperty("_tsDateAdded").GetInt64(),
                    fileElement.GetProperty("_nDownloadCount").GetInt32(),
                    fileElement.GetProperty("_sDownloadUrl").GetString() ?? string.Empty
                );
            }

            if (index == files.Length)
            {
                return files;
            }

            GameBananaFile[] result = new GameBananaFile[index];
            Array.Copy(files, result, index);
            return result;
        }
        finally
        {
            _arrayPool.Return(files);
        }
    }
    /// <summary>
    /// Downloads a file requested from GameBanana
    /// </summary>
    /// <param name="file">Requested file</param>
    /// <param name="downloadPath">Directory where downloaded file will be saved at</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static async Task DownloadFileAsync(GameBananaFile file, string? downloadPath = null)
    {
        if (string.IsNullOrEmpty(file.DownloadUrl))
        {
            throw new ArgumentException("Invalid download URL");
        }

        string fileName = downloadPath == null ? file.FileName : Path.Combine(downloadPath, file.FileName);

        using HttpResponseMessage response = await _httpClient.GetAsync(file.DownloadUrl, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        await using Stream contentStream = await response.Content.ReadAsStreamAsync();
        await using FileStream fileStream = File.Create(fileName);

        await contentStream.CopyToAsync(fileStream);
    }
}
