using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using AiTreeServer.Common;
using DeepSeek.Core;
using DeepSeek.Core.Models;

namespace AiTreeServer.Services;

public class AiService(DeepSeekClient deepSeek)
{
    private const string SystemPrompt = """
                                        НЕ ПИШИ НИКАКОЙ ТЕКСТ В ОТВЕТЕ, ПРОСТО ВЫЗОВИ tool !

                                        Ты система управления анимированной новогодней гирляндой для ёлки. 
                                        Гирлянда умеет отображать заданную палитру, которая определяется перечнем 
                                        HEX-кодов цветов от 3 до 7 штук, а также скоростью анимации (1-10) и 
                                        масштабированием (тоже 1-10). Пользователь опишет сцену или назовёт предмет. 
                                        Тебе нужно подобрать такую палитру, скорость и масштаб, которые лучше всего 
                                        подходят под названное пользователем. Помни, что это цвет зажигания 
                                        светодиода,то есть, например, чёрный просто выключает светодиод, не 
                                        злоупотребляй таким. В гирлянде уже написан анимационный движок, 
                                        который будет перемещать цвета палитры по ней, тебе нужно только задать сами 
                                        цвета, скорость и масштаб. Разрешается иногда дублировать цвета в палитре при 
                                        необходимости, чтобы уменьшить долю присутствия других цветов.

                                        Подумай о том, как описанную сцену будет видеть человек. Важно не только то, 
                                        как много в сцене конкретного цвета, но и то, какие цвета будут больше всего 
                                        бросаться в глаза, и с какими цветами люди ассоциируют описанную сцену и 
                                        предметы в ней. Найди ряд ключевых цветов и строй палитру вокруг них. Но 
                                        иногда могут потребоваться вставки совершенно иного рода, если в сцене есть 
                                        яркие выделяющиеся элементы. Хочу обратить внимание на то, что цвет представленный
                                        единожды в палитре из семи цветов, почти не будет попадать в конечную анимацию
                                        гирлянды из-за особенностей того, как эта анимация строится. В целом если можно
                                        обойтись пятью цветами без дублирования то это лушче, чем семью с дублированием.

                                        Используй только HEX-коды длиной 6, то есть не #FFF, а #FFFFFF.

                                        Скорость анимации: если движение, которое описано в сцене, быстрое и стремительное,
                                        то нужно использовать более высокие значения ближе к 10. Если же движение плавное 
                                        и размеренное, нужно использовать небольшие значения ближе к 1.

                                        Масштаб: если относительно размера сцены есть небольшие объекты, которые выделяются 
                                        цветом, либо есть частые переходы цветов от одних небольших объектов к другим, 
                                        то нужно использовать маленькие значения масштаба ближе к 1. Важен не собственный
                                        размер цветных элементов, а то, как они будут смотреться в масштабах всей сцены, 
                                        с учётом размера фона.
                                        """;

    private static readonly JsonSerializerOptions Options = JsonSerializerOptions.Default;
    private static readonly JsonSchemaExporterOptions ExporterOptions = new()
    {
        TreatNullObliviousAsNonNullable = true,
        TransformSchemaNode = (context, schema) =>
        {
            // Determine if a type or property and extract the relevant attribute provider.
            ICustomAttributeProvider? attributeProvider = context.PropertyInfo is not null
                ? context.PropertyInfo.AttributeProvider
                : context.TypeInfo.Type;

            // Look up any description attributes.
            DescriptionAttribute? descriptionAttr = attributeProvider?
                .GetCustomAttributes(inherit: true)
                .Select(attr => attr as DescriptionAttribute)
                .FirstOrDefault(attr => attr is not null);

            // Apply description attribute to the generated schema.
            if (descriptionAttr != null)
            {
                if (schema is not JsonObject jObj)
                {
                    // Handle the case where the schema is a Boolean.
                    JsonValueKind valueKind = schema.GetValueKind();
                    Debug.Assert(valueKind is JsonValueKind.True or JsonValueKind.False);
                    schema = jObj = new JsonObject();
                    if (valueKind is JsonValueKind.False)
                    {
                        jObj.Add("not", true);
                    }
                }

                jObj.Insert(0, "description", descriptionAttr.Description);
            }

            return schema;
        }
    };

    private readonly Tool _setPaletteDefinition = new()
    {
        Function = new RequestFunction
        {
            Name = "set_palette",
            Description = "Устанавливает палитру для анимированной гирлянды",
            Parameters = Options.GetJsonSchemaAsNode(typeof(SetPaletteParameters), ExporterOptions)
        }
    };

    public async Task<SetPaletteParameters?> AskChatForPalette(string userText)
    {
        var query = new ChatRequest
        {
            Messages = [
                Message.NewSystemMessage(SystemPrompt),
                Message.NewUserMessage(userText)
            ],
            Model = DeepSeekModels.ChatModel,
            Tools = [_setPaletteDefinition]
        };

        ChatResponse? response = await deepSeek.ChatAsync(query, CancellationToken.None);
        if (response?.Choices == null || response.Choices.Count < 1)
        {
            return null;
        }

        Choice choice = response.Choices[0];
        if (choice.Message?.ToolCalls?.FirstOrDefault()?.Function.Name != _setPaletteDefinition.Function.Name)
        {
            return null;
        }

        ToolCalls.ToolCallsFunction functionCall = choice.Message.ToolCalls.First().Function;
        var arguments = JsonSerializer.Deserialize<SetPaletteParameters>(functionCall.Arguments, Options);

        if (arguments?.Colors is not { Length: > 0 })
        {
            return null;
        }

        arguments = arguments with { 
            Colors = Utils.SortColorsByProximity(arguments.Colors.Select(c => c.StartsWith('#') ? c : $"#{c}").Take(16))
        };

        if (arguments.Speed == 0)
        {
            arguments = arguments with { Speed = 5 }; // default
        }
        else
        {
            arguments = arguments with { Speed = Math.Clamp(arguments.Speed + 2, 1, 10) }; // сетка занижает скорость, увеличим искусственно
        }

        if (arguments.Scale == 0)
        {
            arguments = arguments with { Scale = 5 }; // default
        }
        else
        {
            arguments = arguments with { Scale = Math.Clamp(arguments.Scale - 2, 1, 10) }; // сетка завышает масштаб, уменьшим искусственно
        }

        return arguments;
    }
}