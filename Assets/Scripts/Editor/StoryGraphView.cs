#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class StoryGraphView : GraphView
{
    public StoryGraphView()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();

        // HIER REGISTRIEREN WIR DASÄNDERUNGS-EVENT
        graphViewChanged = OnGraphViewChanged;
    }

    // Diese Methode regelt das Erstellen von Verbindungen im GraphView-Fenster
    public override System.Collections.Generic.List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new System.Collections.Generic.List<Port>();

        ports.ForEach(port =>
        {
            // Verhindert, dass man Ausgänge mit Ausgängen verbindet oder sich selbst verknüpft
            if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
            {
                compatiblePorts.Add(port);
            }
        });

        return compatiblePorts;
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        // 1. Wenn neue Linien (Edges) im Editor gezogen wurden
        if (graphViewChange.edgesToCreate != null)
        {
            foreach (Edge edge in graphViewChange.edgesToCreate)
            {
                // Hol dir die beiden beteiligten Custom Nodes
                var outputNode = edge.output.node as StoryGraphNode;
                var inputNode = edge.input.node as StoryGraphNode;

                if (outputNode != null && inputNode != null)
                {
                    // Das Flag, das die Ziel-Node zum Starten benötigt (Eingang links)
                    string incomingRequiredFlag = inputNode.Data.requiredFlag;

                    if (outputNode.Data.nodeType == NodeType.Dialogue && outputNode.Data.originalDialogue.key != null)
                    {
                        var asset = outputNode.Data.originalDialogueAssetReference;

                        Undo.RecordObject(asset, "Change Story Graph Connection");

                        int portIndex = outputNode.OutputPorts.IndexOf(edge.output);

                        var dialogEntry = outputNode.Data.originalDialogue;

                        if (dialogEntry.choices != null && portIndex >= 0 && portIndex < dialogEntry.choices.Length)
                        {
                            dialogEntry.choices[portIndex].resultFlag = incomingRequiredFlag;
                            Debug.Log($"[StoryEditor] Connected Dialog Choice '{dialogEntry.choices[portIndex].displayText}' to Flag: {incomingRequiredFlag}");
                        }
                        else
                        {
                            dialogEntry.resultFlag = incomingRequiredFlag;
                            Debug.Log($"[StoryEditor] Connected Dialog '{dialogEntry.key}' to Flag: {incomingRequiredFlag}");
                        }

                        EditorUtility.SetDirty(asset);
                        AssetDatabase.SaveAssets();
                    }

                    // Fall B: Die Ausgangs-Node ist eine Cutscene
                    else if (outputNode.Data.nodeType == NodeType.Cutscene && outputNode.Data.originalCutscene.dialogueKey != null)
                    {
                        // Hier kannst du das Datenfeld deines Cutscene-Eintrags anpassen.
                        // EditorUtility.SetDirty(outputNode.Data.originalCutsceneAssetReference);
                    }
                }
            }
        }

        // 2. Wenn Verbindungen im Editor gelöscht werden (Entfernen-Taste)
        if (graphViewChange.elementsToRemove != null)
        {
            foreach (var element in graphViewChange.elementsToRemove)
            {
                if (element is Edge edge)
                {
                    var outputNode = edge.output.node as StoryGraphNode;
                    if (outputNode != null && outputNode.Data.nodeType == NodeType.Dialogue)
                    {
                        var asset = outputNode.Data.originalDialogueAssetReference;

                        Undo.RecordObject(asset, "Remove Story Graph Connection");

                        var dialogEntry = outputNode.Data.originalDialogue;
                        int portIndex = outputNode.OutputPorts.IndexOf(edge.output);

                        if (dialogEntry.choices != null && portIndex >= 0 && portIndex < dialogEntry.choices.Length)
                        {
                            dialogEntry.choices[portIndex].resultFlag = "";
                        }
                        else
                        {
                            dialogEntry.resultFlag = "";
                        }

                        EditorUtility.SetDirty(asset);
                        AssetDatabase.SaveAssets();

                        Debug.Log("[StoryEditor] Connection severed. Flag cleared in ScriptableObject.");
                    }
                }
            }
        }

        return graphViewChange;
    }
}
#endif