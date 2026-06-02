#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public enum StoryGraphSimulationState
{
    None,
    Reachable,
    Blocked,
    Current,
    Completed
}

public class StoryGraphNode : Node
{
    public StoryNodeData Data { get; private set; }
    public Port InputPort { get; private set; }
    public Port DialogueInputPort { get; private set; }
    public List<Port> OutputPorts { get; private set; } = new List<Port>();
    public List<Port> NextDialoguePorts { get; private set; } = new List<Port>();

    public event Action<StoryGraphNode> DataChanged;
    public event Action<StoryGraphNode> StructureChanged;

    private readonly Label warningBadge = new Label();
    private StoryGraphSimulationState simulationState = StoryGraphSimulationState.None;
    private bool hasValidationErrors;
    private bool isSearchMatch;

    public StoryGraphNode(StoryNodeData data)
    {
        Data = data;
        title = data.nodeName;

        style.width = 300;
        style.marginLeft = 20;
        style.marginRight = 20;
        style.marginTop = 10;
        style.marginBottom = 10;

        warningBadge.text = string.Empty;
        warningBadge.style.unityFontStyleAndWeight = FontStyle.Bold;
        warningBadge.style.color = Color.yellow;
        titleContainer.Add(warningBadge);

        GeneratePorts();
        GenerateEditableFields();
        RefreshNodeColor();
    }

    private void GeneratePorts()
    {
        InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(string));
        InputPort.portName = string.IsNullOrEmpty(Data.requiredFlag) ? "Req: START / None" : $"Req: {Data.requiredFlag}";
        InputPort.tooltip = "Story flag dependency. Flag edges visualize shared story flags and are not unique direct links.";
        inputContainer.Add(InputPort);

        if (Data.nodeType == NodeType.Dialogue)
        {
            DialogueInputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(DialogueEntry));
            DialogueInputPort.portName = string.IsNullOrEmpty(Data.dialogueKey) ? "Dialogue ID: <empty>" : $"Dialogue ID: {Data.dialogueKey}";
            DialogueInputPort.tooltip = "Direct dialogue jump target. Connect choice Next Dialogue ports here.";
            inputContainer.Add(DialogueInputPort);
        }

        Port resultPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(string));
        resultPort.portName = string.IsNullOrEmpty(Data.resultFlag) ? "Result: END" : $"Result: {Data.resultFlag}";
        resultPort.tooltip = "Story flag produced by this node. Flag edges visualize shared story flags and are not unique direct links.";
        outputContainer.Add(resultPort);
        OutputPorts.Add(resultPort);

        if (Data.nodeType == NodeType.Dialogue && Data.dialogueChoices != null)
        {
            foreach (DialogueOptionData choice in Data.dialogueChoices)
            {
                Port choiceFlagPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(string));
                choiceFlagPort.portName = string.IsNullOrEmpty(choice.resultFlag)
                    ? $"Choice Flag: END ({choice.optionText})"
                    : $"Choice Flag: {choice.resultFlag} ({choice.optionText})";
                choiceFlagPort.tooltip = "Story flag produced when this choice is selected.";
                outputContainer.Add(choiceFlagPort);
                OutputPorts.Add(choiceFlagPort);

                Port nextDialoguePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(DialogueEntry));
                nextDialoguePort.portName = string.IsNullOrEmpty(choice.nextDialogueKey)
                    ? $"Next Dialogue: END ({choice.optionText})"
                    : $"Next Dialogue: {choice.nextDialogueKey} ({choice.optionText})";
                nextDialoguePort.tooltip = "Direct dialogue key jump for this choice.";
                outputContainer.Add(nextDialoguePort);
                NextDialoguePorts.Add(nextDialoguePort);
            }
        }
    }

    private void GenerateEditableFields()
    {
        extensionContainer.Clear();

        VisualElement details = new VisualElement();
        details.style.paddingLeft = 6;
        details.style.paddingRight = 6;
        details.style.paddingTop = 4;
        details.style.paddingBottom = 4;

        if (Data.nodeType == NodeType.Dialogue || Data.nodeType == NodeType.Cutscene)
        {
            string keyLabel = Data.nodeType == NodeType.Cutscene ? "Cutscene Dialogue ID" : "Dialogue ID";
            TextField dialogueKeyField = CreateTextField(keyLabel, Data.dialogueKey, value =>
            {
                Data.dialogueKey = value;
                if (Data.nodeType == NodeType.Dialogue)
                {
                    Data.nodeName = $"Dialog: {value}";
                    title = Data.nodeName;
                }
                UpdatePortNames();
                NotifyDataChanged();
            });
            details.Add(dialogueKeyField);
        }

        TextField requiredFlagField = CreateTextField("Required Flag", Data.requiredFlag, value =>
        {
            Data.requiredFlag = value;
            EnsurePrimaryRequiredFlagListValue(value);
            if (Data.nodeType == NodeType.StoryFlag)
            {
                Data.resultFlag = value;
                Data.nodeName = $"Flag: {value}";
                title = Data.nodeName;
            }
            UpdatePortNames();
            RefreshNodeColor();
            NotifyDataChanged();
        });
        details.Add(requiredFlagField);

        TextField resultFlagField = CreateTextField("Result Flag", Data.resultFlag, value =>
        {
            Data.resultFlag = value;
            if (Data.nodeType == NodeType.StoryFlag)
            {
                Data.requiredFlag = value;
                EnsurePrimaryRequiredFlagListValue(value);
                Data.nodeName = $"Flag: {value}";
                title = Data.nodeName;
            }
            UpdatePortNames();
            RefreshNodeColor();
            NotifyDataChanged();
        });
        details.Add(resultFlagField);

        if (Data.nodeType != NodeType.StoryFlag)
        {
            details.Add(CreateFlagListFoldout("Required Flags", Data.requiredFlags, value => Data.requiredFlags = value));
            details.Add(CreateFlagListFoldout("Blocked Flags", Data.blockedFlags, value => Data.blockedFlags = value));
        }

        if (Data.nodeType == NodeType.Dialogue)
        {
            if (Data.dialogueChoices == null)
            {
                Data.dialogueChoices = new List<DialogueOptionData>();
            }

            Label choiceHelp = new Label("Choices can produce story flags and/or jump to another dialogue key.");
            choiceHelp.style.whiteSpace = WhiteSpace.Normal;
            details.Add(choiceHelp);

            for (int i = 0; i < Data.dialogueChoices.Count; i++)
            {
                details.Add(CreateChoiceFoldout(i));
            }

            Button addChoiceButton = new Button(() =>
            {
                Data.dialogueChoices.Add(new DialogueOptionData { optionText = "New Choice" });
                NotifyStructureChanged();
            })
            {
                text = "+ Add Choice"
            };
            details.Add(addChoiceButton);
        }

        extensionContainer.Add(details);
        RefreshExpandedState();
    }

    private Foldout CreateChoiceFoldout(int choiceIndex)
    {
        DialogueOptionData choice = Data.dialogueChoices[choiceIndex];
        Foldout foldout = new Foldout { text = $"Choice {choiceIndex + 1}: {choice.optionText}", value = false };

        foldout.Add(CreateTextField("Text", choice.optionText, value =>
        {
            Data.dialogueChoices[choiceIndex].optionText = value;
            foldout.text = $"Choice {choiceIndex + 1}: {value}";
            UpdatePortNames();
            NotifyDataChanged();
        }));

        foldout.Add(CreateTextField("Result Flag", choice.resultFlag, value =>
        {
            Data.dialogueChoices[choiceIndex].resultFlag = value;
            UpdatePortNames();
            NotifyDataChanged();
        }));

        foldout.Add(CreateTextField("Next Dialogue ID", choice.nextDialogueKey, value =>
        {
            Data.dialogueChoices[choiceIndex].nextDialogueKey = value;
            UpdatePortNames();
            NotifyDataChanged();
        }));

        VisualElement row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
        row.Add(new Button(() => MoveChoice(choiceIndex, -1)) { text = "↑" });
        row.Add(new Button(() => MoveChoice(choiceIndex, 1)) { text = "↓" });
        row.Add(new Button(() =>
        {
            Data.dialogueChoices.RemoveAt(choiceIndex);
            NotifyStructureChanged();
        }) { text = "Remove" });
        foldout.Add(row);

        return foldout;
    }

    private Foldout CreateFlagListFoldout(string label, List<string> flags, Action<List<string>> assignList)
    {
        if (flags == null)
        {
            flags = new List<string>();
            assignList(flags);
        }

        List<string> editableFlags = flags;
        Foldout foldout = new Foldout { text = label, value = false };
        for (int i = 0; i < editableFlags.Count; i++)
        {
            int flagIndex = i;
            VisualElement row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            TextField field = CreateTextField(string.Empty, editableFlags[flagIndex], value =>
            {
                editableFlags[flagIndex] = value;
                NotifyDataChanged();
            });
            field.style.flexGrow = 1;
            row.Add(field);
            row.Add(new Button(() =>
            {
                editableFlags.RemoveAt(flagIndex);
                NotifyStructureChanged();
            }) { text = "-" });
            foldout.Add(row);
        }

        foldout.Add(new Button(() =>
        {
            editableFlags.Add(string.Empty);
            NotifyStructureChanged();
        }) { text = $"+ {label}" });

        return foldout;
    }

    private static TextField CreateTextField(string label, string value, Action<string> onChanged)
    {
        TextField field = new TextField(label) { value = value ?? string.Empty, isDelayed = true };
        field.RegisterValueChangedCallback(evt => onChanged(evt.newValue));
        return field;
    }

    private void MoveChoice(int choiceIndex, int direction)
    {
        int newIndex = choiceIndex + direction;
        if (newIndex < 0 || newIndex >= Data.dialogueChoices.Count)
        {
            return;
        }

        DialogueOptionData choice = Data.dialogueChoices[choiceIndex];
        Data.dialogueChoices.RemoveAt(choiceIndex);
        Data.dialogueChoices.Insert(newIndex, choice);
        NotifyStructureChanged();
    }

    private void EnsurePrimaryRequiredFlagListValue(string flag)
    {
        if (Data.requiredFlags == null)
        {
            Data.requiredFlags = new List<string>();
        }

        if (Data.requiredFlags.Count == 0)
        {
            Data.requiredFlags.Add(flag ?? string.Empty);
        }
        else
        {
            Data.requiredFlags[0] = flag ?? string.Empty;
        }
    }

    public void RebuildVisuals()
    {
        inputContainer.Clear();
        outputContainer.Clear();
        OutputPorts.Clear();
        NextDialoguePorts.Clear();
        DialogueInputPort = null;

        GeneratePorts();
        GenerateEditableFields();
        UpdatePortNames();
        RefreshPorts();
        RefreshExpandedState();
        RefreshNodeColor();
    }

    public void UpdatePortNames()
    {
        InputPort.portName = string.IsNullOrEmpty(Data.requiredFlag) ? "Req: START / None" : $"Req: {Data.requiredFlag}";

        if (DialogueInputPort != null)
        {
            DialogueInputPort.portName = string.IsNullOrEmpty(Data.dialogueKey) ? "Dialogue ID: <empty>" : $"Dialogue ID: {Data.dialogueKey}";
        }

        if (OutputPorts.Count > 0)
        {
            OutputPorts[0].portName = string.IsNullOrEmpty(Data.resultFlag) ? "Result: END" : $"Result: {Data.resultFlag}";
        }

        if (Data.nodeType == NodeType.Dialogue && Data.dialogueChoices != null)
        {
            for (int i = 0; i < Data.dialogueChoices.Count; i++)
            {
                DialogueOptionData choice = Data.dialogueChoices[i];
                int choiceFlagPortIndex = i + 1;

                if (choiceFlagPortIndex < OutputPorts.Count)
                {
                    OutputPorts[choiceFlagPortIndex].portName = string.IsNullOrEmpty(choice.resultFlag)
                        ? $"Choice Flag: END ({choice.optionText})"
                        : $"Choice Flag: {choice.resultFlag} ({choice.optionText})";
                }

                if (i < NextDialoguePorts.Count)
                {
                    NextDialoguePorts[i].portName = string.IsNullOrEmpty(choice.nextDialogueKey)
                        ? $"Next Dialogue: END ({choice.optionText})"
                        : $"Next Dialogue: {choice.nextDialogueKey} ({choice.optionText})";
                }
            }
        }
    }

    public void SetValidationState(bool hasErrors, string tooltipText)
    {
        hasValidationErrors = hasErrors;
        warningBadge.text = hasErrors ? " ⚠" : string.Empty;
        warningBadge.tooltip = tooltipText ?? string.Empty;
        RefreshNodeColor();
    }

    public void SetSearchMatch(bool matches)
    {
        isSearchMatch = matches;
        RefreshNodeColor();
    }

    public void SetSimulationState(StoryGraphSimulationState state)
    {
        simulationState = state;
        RefreshNodeColor();
    }

    public void RefreshNodeColor()
    {
        if (simulationState != StoryGraphSimulationState.None)
        {
            switch (simulationState)
            {
                case StoryGraphSimulationState.Reachable:
                    titleContainer.style.backgroundColor = new Color(0.2f, 0.65f, 0.95f, 0.95f);
                    return;
                case StoryGraphSimulationState.Blocked:
                    titleContainer.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 0.95f);
                    return;
                case StoryGraphSimulationState.Current:
                    titleContainer.style.backgroundColor = new Color(0.8f, 0.55f, 1f, 0.95f);
                    return;
                case StoryGraphSimulationState.Completed:
                    titleContainer.style.backgroundColor = new Color(0.95f, 0.8f, 0.2f, 0.95f);
                    return;
            }
        }

        if (hasValidationErrors)
        {
            titleContainer.style.backgroundColor = new Color(0.9f, 0.45f, 0.1f, 0.95f);
            return;
        }

        if (isSearchMatch)
        {
            titleContainer.style.backgroundColor = new Color(0.55f, 0.35f, 0.95f, 0.95f);
            return;
        }

        bool hasInputConnection = InputPort.connected || (DialogueInputPort != null && DialogueInputPort.connected);
        bool hasOutputConnection = OutputPorts.Exists(p => p.connected) || NextDialoguePorts.Exists(p => p.connected);
        bool isStartNode = string.IsNullOrEmpty(Data.requiredFlag) && (Data.nodeType != NodeType.Dialogue || !string.IsNullOrEmpty(Data.dialogueKey));

        if (!hasInputConnection && !hasOutputConnection && !isStartNode)
        {
            titleContainer.style.backgroundColor = new Color(0.85f, 0.65f, 0.1f, 0.9f);
            return;
        }

        switch (Data.nodeType)
        {
            case NodeType.StoryFlag:
                titleContainer.style.backgroundColor = new Color(0.15f, 0.4f, 0.7f, 0.9f);
                break;
            case NodeType.Dialogue:
                titleContainer.style.backgroundColor = new Color(0.15f, 0.6f, 0.3f, 0.9f);
                break;
            case NodeType.Cutscene:
                titleContainer.style.backgroundColor = new Color(0.75f, 0.15f, 0.15f, 0.9f);
                break;
        }
    }

    private void NotifyDataChanged()
    {
        DataChanged?.Invoke(this);
    }

    private void NotifyStructureChanged()
    {
        StructureChanged?.Invoke(this);
    }
}
#endif
