using AiTreeServer.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using AiTreeServer.Common;

namespace AiTreeServer.Controllers;

[ApiController]
[Route("[controller]")]
public class PaletteController(BusService bus)
{
    [HttpGet]
    public string GetCurrentPalette()
    {
        return bus.GetCurrentResponse();
    }

    [HttpGet("test")]
    public void Test([FromQuery] string q)
    {
        bus.Requests.Enqueue(q);
    }

    [HttpGet("delete")]
    public void Delete()
    {
        bus.DeleteCurrentPalette();
    }

    [HttpGet("view")]
    public async Task<ContentResult> ViewPalette()
    {
        string html = await GeneratePaletteHtml();
        return new ContentResult
        {
            ContentType = "text/html",
            Content = html
        };
    }

    private async Task<string> GeneratePaletteHtml()
    {
        // Читаем шаблон
        string template = await File.ReadAllTextAsync("Templates/PaletteView.html");
        
        // Генерируем контент с палитрой
        var content = new StringBuilder();
        
        SetPaletteParameters? currentParameters = bus.PeekParameters();
        if (currentParameters is { Colors.Length: > 0 })
        {
            content.AppendLine("        <div class=\"palette\">");
            
            foreach (string color in currentParameters.Colors)
            {
                content.AppendLine("            <div class=\"color-box\">");
                content.AppendLine($"                <div class=\"color-rect\" style=\"background-color: {color};\"></div>");
                content.AppendLine($"                <div class=\"color-code\">{color}</div>");
                content.AppendLine("            </div>");
            }
            
            content.AppendLine("        </div>");
            content.AppendLine($"        <div class=\"info\">Всего цветов: {currentParameters.Colors.Length} | Скорость: {currentParameters.Speed} | Масштаб: {currentParameters.Scale}</div>");
        }
        else
        {
            content.AppendLine("        <div class=\"empty-state\">Палитра пуста или данные не распознаны</div>");
        }
        
        // Подставляем контент в шаблон
        return template.Replace("{{CONTENT}}", content.ToString());
    }

}