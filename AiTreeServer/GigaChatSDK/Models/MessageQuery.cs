using System.Text.Json.Serialization;

namespace AiTreeServer.GigaChatSDK.Models;

/// <summary>
/// В модели MessageQuery хранятся все данные запроса, в случае, если вам необходимо только передать, 
/// только сообщения, не указывайте значения остальных аргументов при иницилизации класса
/// </summary>
public class MessageQuery
{
    /// <summary>
    /// Название модели
    /// По умолчанию: GigaChat:latest
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; }
    /// <summary>
    /// Массив передаваемых сообщений
    /// По умолчанию: -
    /// </summary>

    [JsonPropertyName("messages")]
    public List<MessageContent> Messages { get; set; }
    [JsonPropertyName("functions")]
    public List<FunctionDescription>? Functions { get; set; } // Поле для списка функций

    [JsonPropertyName("function_call")]
    public object? FunctionCall { get; set; } // Авто-вызов функций
    /// <summary>
    /// Температура выборки в диапазоне от ноля до двух. Чем выше значение, тем более случайным будет ответ модели.
    /// По умолчанию: 0.87
    /// </summary>

    [JsonPropertyName("temperature")]
    public float? Temperature { get; set; }
    /// <summary>
    /// Параметр используется как альтернатива temperature. 
    /// Задает вероятностную массу токенов, которые должна учитывать модель. 
    /// Так, если передать значение 0.1, модель будет учитывать только токены, чья вероятностная масса входит в верхние 10%.
    /// По умолчанию: 0.47
    /// </summary>
    [JsonPropertyName("top_p")]
    public float? TopP { get; set; }
    /// <summary>
    /// Количество вариантов ответов, которые нужно сгенерировать для каждого входного сообщения
    /// По умолчанию: 1
    /// </summary>
    [JsonPropertyName("n")]
    public long N { get; set; }
    /// <summary>
    /// Указывает, что сообщения надо передавать по частям в потоке.
    /// По умолчанию: false
    /// </summary>
    [JsonPropertyName("stream")]
    public bool Stream { get; set; }
    /// <summary>
    /// Максимальное количество токенов, которые будут использованы для создания ответов
    /// По умолчанию: 512
    /// </summary>
    [JsonPropertyName("max_tokens")]
    public long MaxTokens { get; set; }

    public MessageQuery(
        List<MessageContent>? messages = null,
        List<FunctionDescription>? functions = null,
        object? functionCall = null,
        string model = "GigaChat:latest",  
        float? temperature = null, 
        float? topP = null, 
        long n = 1, 
        bool stream = false, 
        long maxTokens = 512)
    {
        Model = model;
        Messages = messages ?? new List<MessageContent>();
        Functions = functions ?? new List<FunctionDescription>();
        FunctionCall = functionCall ?? "auto";
        Temperature = temperature;
        TopP = topP;
        N = n;
        Stream = stream;
        MaxTokens = maxTokens;
    }
}