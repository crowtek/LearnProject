using System;

public interface IDialogueView
{
    /// Raised when the player asks to advance the dialogue text.
    event Action ContinueRequested;

    /// Raised when the player selects one of the active dialogue choices.
    event Action<DialogueChoice> ChoiceSelected;

    /// True while the current line is still being animated or revealed.
    bool IsDisplayingLine { get; }

    /// Shows the dialogue container and speaker metadata.
    void Show(DialogueData data);

    /// Displays or animates a single dialogue line.
    void DisplayLine(string line);

    /// Immediately completes the current line display, if it is animated.
    void CompleteLine(string line);

    /// Shows the supplied choices and reports selection through ChoiceSelected.
    void ShowChoices(DialogueChoice[] choices);

    /// Hides the dialogue presentation and clears transient UI state.
    void Hide();
}