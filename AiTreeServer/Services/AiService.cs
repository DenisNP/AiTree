using AiTreeServer.GigaChatSDK;
using AiTreeServer.GigaChatSDK.Interfaces;
using AiTreeServer.GigaChatSDK.Models;

namespace AiTreeServer.Services;

public class AiService
{
    private const string SystemPrompt = """
                                        Ты система управления анимированной новогодней гирляндой для ёлки. 
                                        Гирлянда умеет отображать заданную палитру, которая определяется перечнем 
                                        HEX-кодов цветов от 3 до 7 штук, а также скоростью анимации (1-10) и 
                                        масштабированием (тоже 1-10). Пользователь опишет сцену или назовёт предмет. 
                                        Тебе нужно подобрать такую палитру, скорость и масштаб, которые лучше всего 
                                        подходят под названное пользователем. Помни, что это цвет зажигания 
                                        светодиода,то есть, например, чёрный просто выключает светодиод, не 
                                        злоупотребляй таким. В идеале чтобы не больше трети гирлянды было выключено 
                                        в каждый момент времени. В гирлянде уже написан анимационный движок, 
                                        который будет перемещать цвета палитры по ней, тебе нужно только задать сами 
                                        цвета, скорость и масштаб. Разрешается иногда дублировать цвета в палитре при 
                                        необходимости, чтобы уменьшить долю присутствия других цветов.
                                        
                                        Подумай о том, как описанную сцену будет видеть человек. Важно не только то, 
                                        как много в сцене конкретного цвета, но и то, какие цвета будут больше всего 
                                        бросаться в глаза, и с какими цветами люди ассоциируют описанную сцену и 
                                        предметы в ней. Найди ряд ключевых цветов и строй палитру вокруг них. Но 
                                        иногда могут потребоваться вставки совершенно иного рода, если в сцене есть 
                                        яркие выделяющиеся элементы. Не нужно давать цвета предметам, которые в сцене 
                                        есть, но малозаметны или присутствуют в небольшом объёме, которые заслонены 
                                        другими предметами и так далее. Особенно если палитра небольшая по числу 
                                        цветов, тщательно продумай, какие цвета ты туда включишь. Иногда лучше 
                                        увеличить или продублировать цвета, чтобы не дать второстепенному цвету такой 
                                        же вес в конечной палитре, как ключевому.
                                        """;
    
    private readonly FunctionDescription _setPaletteDefinition = new()
    {
        Name = "set_palette",
        Description = "Устанавливает палитру для анимированной гирлянды",
        Parameters = new FunctionParameters
        {
            Type = "object",
            Properties = new Dictionary<string, ParameterProperty>
            {
                {
                    "colors", new ParameterProperty
                    {
                        Type = "array",
                        Items = new ParameterPropertyItems
                        {
                            Type = "string"
                        },
                        Description = "Список HEX-кодов цветов палитры, не менее 3 и не более 7 кодов"
                    }
                },
                {
                    "animation_speed", new ParameterProperty
                    {
                        Type = "integer",
                        Description = "Скорость анимации, от 1 (медленно) до 10 (быстро)"
                    }
                },
                {
                    "palette_scale", new ParameterProperty
                    {
                        Type = "integer",
                        Description = "Масштабирование палитры на гирлянду от 1 до 10. Если 1, то отдельные участки " +
                                      "палитры будут маленькими, а вся палитра заполнит гирлянду несколько раз. " +
                                      "Если 10, то, наоборот, во всю гирлянду будет только часть палитры " +
                                      "с анимированным переходом в другую."
                    }
                }
            },
            Required = ["colors"]
        },
        FewShotExamples = [new FewShotExample
            {
                Request = "Океан во время шторма",
                Params = new Dictionary<string, object>
                {
                    {"colors", new[]{ "#243961", "#eaeef1", "#4e76c1", "#3661b0" }},
                    {"animation_speed", 7},
                    {"palette_scale", 5}
                }
            }
        ],
        ReturnParameters = new ReturnParameters()
    };

    private readonly GigaChat _gigaChat;

    public AiService()
    {
        IHttpService httpService = new HttpService(ignoreTls: true);
        ITokenService tokenService = new TokenService(httpService, isCommercial: false);
        _gigaChat = new GigaChat(tokenService, httpService);
    }

    public async Task<Response?> AskChat(string userText)
    {
        var query = new MessageQuery(
            [
                new MessageContent("system", SystemPrompt),
                new MessageContent("user", userText)
            ],
            [_setPaletteDefinition],
            model: "GigaChat-2-Max",
            functionCall: new Dictionary<string, string> { { "name", _setPaletteDefinition.Name! } }
        );
        
        return await _gigaChat.CompletionsAsync(query);
    }
}