#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class StoryGraphNode : Node
{
    public StoryNodeData Data { get; private set; }
    public Port InputPort { get; private set; }
    public List<Port> OutputPorts { get; private set; } = new List<Port>();

    public StoryGraphNode(StoryNodeData data)
    {
        Data = data;
        title = data.nodeName;

        style.width = 260; // Etwas breiter für String-Namen
        style.marginLeft = 20; style.marginRight = 20;
        style.marginTop = 10; style.marginBottom = 10;

        GeneratePorts();
        RefreshNodeColor();
    }

    private void GeneratePorts()
    {
        // Linker Connector: Welches Flag schaltet diese Node frei?
        InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
        InputPort.portName = string.IsNullOrEmpty(Data.requiredFlag) ? "START / None" : $"Req: {Data.requiredFlag}";
        inputContainer.Add(InputPort);

        // Rechter Connector: Welche Flags werden abgefeuert?
        if (Data.nodeType == NodeType.Dialogue && Data.dialogueChoices != null && Data.dialogueChoices.Count > 0)
        {
            // Verzweigter Dialog (Choices)
            foreach (var choice in Data.dialogueChoices)
            {
                var outPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
                outPort.portName = $"➔ {choice.resultFlag} ({choice.optionText})";
                outputContainer.Add(outPort);
                OutputPorts.Add(outPort);
            }
        }
        else
        {
            // Normale Story-Flags oder Cutscenes mit linearem Ausgang
            var outPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            outPort.portName = string.IsNullOrEmpty(Data.resultFlag) ? "END" : $"➔ {Data.resultFlag}";
            outputContainer.Add(outPort);
            OutputPorts.Add(outPort);
        }
    }

    public void RefreshNodeColor()
    {
        // Verbindung prüfen (Knoten ohne Ein- UND Ausgang werden gelb)
        bool hasInputConnection = InputPort.connected;
        bool hasOutputConnection = OutputPorts.Exists(p => p.connected);

        // Ausnahme: Start-Nodes (kein requiredFlag) brauchen logischerweise keinen Eingang
        bool isStartNode = string.IsNullOrEmpty(Data.requiredFlag);

        if (!hasInputConnection && !hasOutputConnection && !isStartNode)
        {
            titleContainer.style.backgroundColor = new Color(0.85f, 0.65f, 0.1f, 0.9f); // Gelb (Unconnected)
            return;
        }

        switch (Data.nodeType)
        {
            case NodeType.StoryFlag:
                titleContainer.style.backgroundColor = new Color(0.15f, 0.4f, 0.7f, 0.9f); // Blau
                break;
            case NodeType.Dialogue:
                titleContainer.style.backgroundColor = new Color(0.15f, 0.6f, 0.3f, 0.9f); // Grün
                break;
            case NodeType.Cutscene:
                titleContainer.style.backgroundColor = new Color(0.75f, 0.15f, 0.15f, 0.9f); // Rot
                break;
        }
    }
}
#endif