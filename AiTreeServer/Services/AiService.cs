using System.Text.Json;
using AiTreeServer.Common;
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
                                        
                                        Используй только HEX-коды длиной 6, то есть не #FFF, а #FFFFFF.
                                        
                                        Скорость анимации: если движение, которое описано в сцене, быстрое и стремительное,
                                        то нужно использовать более высокие значения ближе к 10. Или если какой-то объект появляется 
                                        кратковременно, например, молния. Если же движение плавное и размеренное, нужно 
                                        использовать небольшие значения ближе к 1.
                                        
                                        Масштаб: если относительно размера сцены есть небольшие объекты, которые выделяются 
                                        цветом, либо есть частые переходы цветов от одних небольших объектов к другим, 
                                        то нужно использовать маленькие значения масштаба ближе к 1. Если же элементы 
                                        в сцене либо сами по себе относительно крупные, либо очень медленно переходят 
                                        по цветам друг в друга, нужно использовать большие знаения ближе к 10.
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
                        Description = "Масштабирование палитры на гирлянду от 1 (мелко) до 10 (крупно)"
                    }
                }
            },
            Required = ["colors"]
        },
        FewShotExamples = [
            new FewShotExample
            {
                Request = "океан во время шторма",
                Params = new Dictionary<string, object>
                {
                    {"colors", new[]{ "#243961", "#eaeef1", "#4e76c1", "#3661b0" }},
                    {"animation_speed", 7},
                    {"palette_scale", 5}
                }
            },
            new FewShotExample
            {
                Request = "цветущий сад сакуры летним днём",
                Params = new Dictionary<string, object>
                {
                    {"colors", new[]{ "#FFC0CB", "#FFE4E1", "#FFFFFF", "#90EE90", "#FFD700" }},
                    {"animation_speed", 3},
                    {"palette_scale", 7}        
                }
            },
            new FewShotExample
            {
                Request = "коробка с конфетти",
                Params = new Dictionary<string, object>
                {
                    {"colors", new[]{ "#FFD700","#FF69B4","#00FFFF","#FF00FF","#FF4500" }},
                    {"animation_speed", 10},
                    {"palette_scale", 1}        
                }
            },
            new FewShotExample
            {
                Request = "хвойный лес солнечным зимним днём",
                Params = new Dictionary<string, object>
                {
                    {"colors", new[]{ "#FFFFFF", "#006400", "#FFFFFF", "#FFD700", "#c8edf9" }},
                    {"animation_speed", 1},
                    {"palette_scale", 6}        
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

    public async Task<SetPaletteParameters?> AskChatForPalette(string userText)
    {
        var query = new MessageQuery(
            [
                //new MessageContent("system", SystemPrompt),
                //new MessageContent("user", userText)
                new MessageContent("user", SystemPrompt + "\n\nСцена: " +  userText), // так лучше работает
            ],
            [_setPaletteDefinition],
            model: "GigaChat-2-Pro",
            temperature: 1.5f,
            functionCall: new Dictionary<string, string> { { "name", _setPaletteDefinition.Name! } }
        );

        Response? response = await _gigaChat.CompletionsAsync(query);
        if (response?.Choices == null || response.Choices.Count < 1)
        {
            return null;
        }

        Choice choice = response.Choices[0];
        if (choice.Message?.FunctionCall?.Name != _setPaletteDefinition.Name)
        {
            return null;
        }

        FunctionCall functionCall = choice.Message!.FunctionCall!;
        if (!functionCall.Arguments.TryGetValue("colors", out object? colorsArgument) || colorsArgument is not JsonElement colorsEl)
        {
            return null;
        }

        string[] colors = colorsEl.EnumerateArray().Select(x => x.GetString()).OfType<string>().ToArray();

        if (colors is not { Length: > 0 })
        {
            return null;
        }
        
        var speed = 5;
        var scale = 5;

        if (functionCall.Arguments.TryGetValue("animation_speed", out object? speedArgument))
        {
            int.TryParse(speedArgument?.ToString(), out speed);
        }

        if (functionCall.Arguments.TryGetValue("palette_scale", out object? scaleArgument))
        {
            int.TryParse(scaleArgument?.ToString(), out scale);
        }

        return new SetPaletteParameters(colors.Take(16).ToArray(), speed, scale);
    }
}