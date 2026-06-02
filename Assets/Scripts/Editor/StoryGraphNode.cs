#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class StoryGraphNode : Node
{
    public StoryNodeData Data { get; private set; }
    public Port InputPort { get; private set; }
    public Port DialogueInputPort { get; private set; }
    public List<Port> OutputPorts { get; private set; } = new List<Port>();
    public List<Port> NextDialoguePorts { get; private set; } = new List<Port>();

    public event Action<StoryGraphNode> DataChanged;

    public StoryGraphNode(StoryNodeData data)
    {
        Data = data;
        title = data.nodeName;

        style.width = 300;
        style.marginLeft = 20; style.marginRight = 20;
        style.marginTop = 10; style.marginBottom = 10;

        GeneratePorts();
        GenerateEditableFields();
        RefreshNodeColor();
    }

    private void GeneratePorts()
    {
        // Story flag input: which flag unlocks/triggers this node?
        InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(string));
        InputPort.portName = string.IsNullOrEmpty(Data.requiredFlag) ? "Req: START / None" : $"Req: {Data.requiredFlag}";
        inputContainer.Add(InputPort);

        // Dialogue id input: which dialogue choices jump to this dialogue key?
        if (Data.nodeType == NodeType.Dialogue)
        {
            DialogueInputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(DialogueEntry));
            DialogueInputPort.portName = string.IsNullOrEmpty(Data.dialogueKey) ? "Dialogue ID: <empty>" : $"Dialogue ID: {Data.dialogueKey}";
            inputContainer.Add(DialogueInputPort);
        }

        // Main result/story flag output.
        Port resultPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(string));
        resultPort.portName = string.IsNullOrEmpty(Data.resultFlag) ? "Result: END" : $"Result: {Data.resultFlag}";
        outputContainer.Add(resultPort);
        OutputPorts.Add(resultPort);

        if (Data.nodeType == NodeType.Dialogue && Data.dialogueChoices != null)
        {
            // Choice result flags and next-dialogue ids are separate so both serialized fields can be edited visually.
            foreach (var choice in Data.dialogueChoices)
            {
                Port choiceFlagPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(string));
                choiceFlagPort.portName = string.IsNullOrEmpty(choice.resultFlag)
                    ? $"Choice Flag: END ({choice.optionText})"
                    : $"Choice Flag: {choice.resultFlag} ({choice.optionText})";
                outputContainer.Add(choiceFlagPort);
                OutputPorts.Add(choiceFlagPort);

                Port nextDialoguePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(DialogueEntry));
                nextDialoguePort.portName = string.IsNullOrEmpty(choice.nextDialogueKey)
                    ? $"Next Dialogue: END ({choice.optionText})"
                    : $"Next Dialogue: {choice.nextDialogueKey} ({choice.optionText})";
                outputContainer.Add(nextDialoguePort);
                NextDialoguePorts.Add(nextDialoguePort);
            }
        }
    }

    private void GenerateEditableFields()
    {
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
                Data.nodeName = $"Flag: {value}";
                title = Data.nodeName;
            }
            UpdatePortNames();
            RefreshNodeColor();
            NotifyDataChanged();
        });
        details.Add(resultFlagField);

        if (Data.nodeType == NodeType.Dialogue && Data.dialogueChoices != null && Data.dialogueChoices.Count > 0)
        {
            for (int i = 0; i < Data.dialogueChoices.Count; i++)
            {
                int choiceIndex = i;
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
                details.Add(foldout);
            }
        }

        extensionContainer.Add(details);
        RefreshExpandedState();
    }

    private static TextField CreateTextField(string label, string value, Action<string> onChanged)
    {
        TextField field = new TextField(label) { value = value ?? string.Empty };
        field.RegisterValueChangedCallback(evt => onChanged(evt.newValue));
        return field;
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

    private void NotifyDataChanged()
    {
        DataChanged?.Invoke(this);
    }

    public void RefreshNodeColor()
    {
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
}
#endif
