namespace DC_bot.Interface.Service.Presentation.Models;

/// <summary>Defines the versioned help interaction actions carried by component IDs.</summary>
public enum HelpNavigationAction
{

    /// <summary>Select a category from the home view.</summary>
    Category = 0,

    /// <summary>Select a command from the displayed category page.</summary>
    Command = 1,

    /// <summary>Show the previous category page.</summary>
    Previous = 2,

    /// <summary>Show the next category page.</summary>
    Next = 3,

    /// <summary>Return to the category overview.</summary>
    Home = 4,

    /// <summary>Restore the category page preceding command details.</summary>
    Back = 5
}
