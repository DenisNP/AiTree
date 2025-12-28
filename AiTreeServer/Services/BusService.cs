using System.Collections.Concurrent;
using AiTreeServer.Common;

namespace AiTreeServer.Services;

public class BusService
{
    public ConcurrentQueue<string> Requests { get; } = new();
    public volatile string CurrentResponse = "0,7,255,0,0,255,127,0,255,255,0,0,255,0,0,255,255,0,0,255,127,0,255";
    public SetPaletteParameters? LastParameters { get; set; }
}