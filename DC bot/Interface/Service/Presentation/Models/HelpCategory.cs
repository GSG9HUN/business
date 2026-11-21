namespace DC_bot.Interface.Service.Presentation.Models;

/// <summary>Identifies the user-facing command categories of interactive help.</summary>
public enum HelpCategory
{

    /// <summary>Playback and track navigation.</summary>
    Playback,

    /// <summary>Waiting-track management.</summary>
    Queue,

    /// <summary>Saved playlist management.</summary>
    Playlists,

    /// <summary>Audio controls and effects.</summary>
    Audio,

    /// <summary>Recommendations, lyrics and favorites.</summary>
    Discovery,

    /// <summary>Server preferences and voice setup.</summary>
    Settings,

    /// <summary>Information and utility commands.</summary>
    Utilities
}
