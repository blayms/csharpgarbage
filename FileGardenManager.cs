// |¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯|
// |2025   ---        -<CODE BY BLAYMS>-        ---   2025|
// |______________________________________________________|

using System.Collections;
using System.Text;
/// <summary>
/// A class that helps to grab data from File Garden. Fully supported by filegarden.com! Code by Blayms!
/// </summary>
public static class FileGardenManager
{
    /// <summary>
    /// Identificator of the garden used by this manager
    /// </summary>
    public static string GardenID { get; private set; } = string.Empty;
    /// <summary>
    /// Determines if the manager is initialized
    /// </summary>
    public static bool Initialized { get; private set; } = false;
    /// <summary>
    /// Initializes the manager only when it's not initialized
    /// </summary>
    /// <param name="gardenID">Identificator of the garden</param>
    /// <exception cref="InvalidOperationException">Thrown if the manager was already initialized</exception>
    private static void CheckForInitialization()
    {
        if (!Initialized)
        {
            throw new InvalidOperationException($"{nameof(FileGardenManager)} was never initialized! Call {nameof(FileGardenManager)}{nameof(Initialize)}() to intialize the manager!");
        }
    }
    public static void Initialize(string gardenID)
    {
        if (!Initialized)
        {
            GardenID = gardenID;
            Initialized = true;
        }
        else
        {
            throw new InvalidOperationException($"{nameof(FileGardenManager)} is already initialized with Garden ID = {GardenID}");
        }
    }
    /// <summary>
    /// Uninitializes the manager only when it's initialized
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the manager was never initialized</exception>
    public static void Uninitialize()
    {
        CheckForInitialization();
        GardenID = string.Empty;
        Initialized = false;
    }
    /// <summary>
    /// Tries to run <see cref="Uninitialize"/> and then initializes the manager with specified garden identificator
    /// </summary>
    /// <param name="gardenId">Identificator of the garden</param>
    public static void Reintialize(string gardenId)
    {
        Uninitialize();
        Initialize(gardenId);
    }

    private static async Task<HttpContent> GrabContentAsync(string fileID)
    {
        CheckForInitialization();
        string url = $"https://file.garden/{GardenID}/{fileID}";
        using (HttpClient http = new HttpClient())
        {
            HttpResponseMessage response = await http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to fetch data from {url}. Status: {response.StatusCode}");
            }
            return response.Content;
        }
    }
    /// <summary>
    /// Asynchronously grabs the string from the file identificator
    /// </summary>
    /// <param name="fileID">Identificator of the file from the garden (must contain the file name and it's extension with a "." separator)</param>
    /// <param name="encoding">Determines how to decode the string. Default <see cref="Encoding.Default"></param>
    /// <returns></returns>
    public static async Task<string> GrabStringAsync(string fileID, Encoding? encoding = null)
    {
        CheckForInitialization();
        var content = await GrabContentAsync(fileID);
        var bytes = await content.ReadAsByteArrayAsync();
        return (encoding ?? Encoding.Default).GetString(bytes);
    }
    /// <summary>
    /// Asynchronously grabs the byte array from the file identificator
    /// </summary>
    /// <param name="fileID">Identificator of the file from the garden (must contain the file name and it's extension with a "." separator)</param>
    /// <returns></returns>
    public static async Task<byte[]> GrabBytesAsync(string fileID)
    {
        CheckForInitialization();
        return await GrabContentAsync(fileID).Result.ReadAsByteArrayAsync();
    }
    /// <summary>
    /// Asynchronously grabs the stream from the file identificator
    /// </summary>
    /// <param name="fileID">Identificator of the file from the garden (must contain the file name and it's extension with a "." separator)</param>
    /// <returns></returns>
    public static async Task<Stream> GrabStreamAsync(string fileID)
    {
        CheckForInitialization();
        return await GrabContentAsync(fileID).Result.ReadAsStreamAsync();
    }
    /// <summary>
    /// Coroutine-based version of <see cref="GrabStringAsync"/>.
    /// Yields until the file is downloaded and decoded into a string.
    /// </summary>
    /// <param name="fileID">Identificator of the file from the garden (must include the file name and extension).</param>
    /// <param name="onSuccess">Callback invoked with the downloaded string on success.</param>
    /// <param name="onError">Optional callback invoked with the exception if an error occurs.</param>
    /// <param name="encoding">Optional encoding used to decode the file. Defaults to <see cref="Encoding.Default"/>.</param>
    /// <returns><see cref="IEnumerator"/> that yields while the async task is running.</returns>
    public static IEnumerator GrabStringCoroutine(string fileID, Encoding? encoding = null, Action<string>? onSuccess = null, Action<Exception>? onError = null)
    {
        CheckForInitialization();
        Task<string> task = GrabStringAsync(fileID, encoding);
        while (!task.IsCompleted)
        {
            yield return null;
        }
        if (task.Exception != null)
        {
            onError?.Invoke(task.Exception.InnerException ?? task.Exception);
        }
        else
        {
            onSuccess?.Invoke(task.Result);
        }
    }

    /// <summary>
    /// Coroutine-based version of <see cref="GrabBytesAsync"/>.
    /// Yields until the file is downloaded as a byte array.
    /// </summary>
    /// <param name="fileID">Identificator of the file from the garden (must include the file name and extension).</param>
    /// <param name="onSuccess">Callback invoked with the downloaded byte array on success.</param>
    /// <param name="onError">Optional callback invoked with the exception if an error occurs.</param>
    /// <returns><see cref="IEnumerator"/> that yields while the async task is running.</returns>
    public static IEnumerator GrabBytesCoroutine(string fileID, Action<byte[]>? onSuccess = null, Action<Exception>? onError = null)
    {
        CheckForInitialization();
        Task<byte[]> task = GrabBytesAsync(fileID);
        while (!task.IsCompleted)
        {
            yield return null;
        }
        if (task.Exception != null)
        {
            onError?.Invoke(task.Exception.InnerException ?? task.Exception);
        }
        else
        {
            onSuccess?.Invoke(task.Result);
        }
    }

    /// <summary>
    /// Coroutine-based version of <see cref="GrabStreamAsync"/>.
    /// Yields until the file is downloaded as a <see cref="Stream"/>.
    /// </summary>
    /// <param name="fileID">Identificator of the file from the garden (must include the file name and extension).</param>
    /// <param name="onSuccess">Callback invoked with the downloaded stream on success.</param>
    /// <param name="onError">Optional callback invoked with the exception if an error occurs.</param>
    /// <returns><see cref="IEnumerator"/> that yields while the async task is running.</returns>
    public static IEnumerator GrabStreamCoroutine(string fileID, Action<Stream>? onSuccess = null, Action<Exception>? onError = null)
    {
        CheckForInitialization();
        Task<Stream> task = GrabStreamAsync(fileID);
        while (!task.IsCompleted)
        {
            yield return null;
        }
        if (task.Exception != null)
        {
            onError?.Invoke(task.Exception.InnerException ?? task.Exception);
        }
        else
        {
            onSuccess?.Invoke(task.Result);
        }
    }
}
