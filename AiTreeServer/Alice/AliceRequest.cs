namespace AiTreeServer.Alice;

public record AliceEmpty;
    
public record Interfaces
{
    public AliceEmpty? Screen { get; set; }
    public AliceEmpty? Payments { get; set; }
    public AliceEmpty? AccountLinking { get; set; }
}

public record Meta
{
    public string? Locale { get; set; }
    public string? Timezone { get; set; }
    public string? ClientId { get; set; }
    public Interfaces? Interfaces { get; set; }
}

public record Markup
{
    public bool DangerousContext { get; set; }
}

public record Intent
{
    public Dictionary<string, IntentSlot> Slots { get; set; } = [];
}

public record IntentSlot
{
    public string? Type { get; set; }
    public object? Value { get; set; }
}

public record TokensRange
{
    public int Start { get; set; }
    public int End { get; set; }
}

public record Entity
{
    public TokensRange? Tokens { get; set; }
    public string? Type { get; set; }
    public object? Value { get; set; }
}

public record Nlu
{
    public List<string> Tokens { get; set; } = [];
    public List<Entity> Entities { get; set; } = [];
    public Dictionary<string, Intent> Intents { get; set; } = [];
}

public record Request
{
    public string? Command { get; set; }
    public string? OriginalUtterance { get; set; }
    public string? Type { get; set; }
    public Markup Markup { get; set; } = new();
    public Dictionary<string, string> Payload { get; set; } = [];
    public Nlu Nlu { get; set; } = new();
}

public record User
{
    public string? UserId { get; set; }
}

public record Device
{
    public string? DeviceId { get; set; }
}

public record Session
{
    public bool New { get; set; }
    public int MessageId { get; set; }
    public string? SessionId { get; set; }
    public string? SkillId { get; set; }
    public string? UserId { get; set; }
    public User? User { get; set; }
    public Device Device { get; set; } = new();
}

public class State
{
    public UserState User { get; set; } = new();
    public SessionState Session { get; set; } = new();
}

public class AliceRequest
{
    public Meta Meta { get; set; } = new();
    public AliceEmpty? AccountLinkingCompleteEvent { get; set; }
    public Request Request { get; set; } = new();
    public Session Session { get; set; } = new();
    public string? Version { get; set; }
    public State State { get; set; } = new();

    public int ExtractFirstNumericToken(int min, int max, int fallback)
    {
        foreach (string token in Request.Nlu.Tokens)
        {
            if (int.TryParse(token, out int n))
            {
                if (min <= n && n <= max)
                {
                    return n;
                }
            }
        }

        return fallback;
    }

    public bool HasScreen()
    {
        return Meta?.Interfaces?.Screen != null;
    }

    public bool IsAccountLinking()
    {
        return AccountLinkingCompleteEvent != null;
    }

    public bool IsPing()
    {
        return Request.Command?.ToLower() == "ping";
    }

    public bool IsAnonymous()
    {
        return Session.User == null;
    }

    public bool IsEnter()
    {
        return string.IsNullOrEmpty(Request.Command);
    }

    public Intent? GetIntent(string name)
    {
        return Request.Nlu.Intents.GetValueOrDefault(name);
    }

    public bool HasIntent(string name, bool withSlots = false)
    {
        return withSlots ? GetIntent(name)?.Slots != null : GetIntent(name) != null;
    }

    public bool HasOneOfIntents(params string[] names)
    {
        return names.Any(x=>GetIntent(x)!= null);
    }

    public bool HasSlot(string intentName, string slotName)
    {
        return !string.IsNullOrEmpty(GetSlot(intentName, slotName));
    }

    public bool HasOneOfSlots(string intentName, params string[] slotNames)
    {
        return slotNames.Any(s => HasSlot(intentName, s));
    }

    public bool HasAllSlots(string intentName, params string[] slotNames)
    {
        return slotNames.All(s => HasSlot(intentName, s));
    }

    public string? GetSlot(string intentName, string slotName)
    {
        Intent? intent = GetIntent(intentName);
        if (intent?.Slots == null || !intent.Slots.TryGetValue(slotName, out IntentSlot? slot))
        {
            return null;
        }

        return slot.Value?.ToString();
    }
}