#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class StoryGraphEditorWindow : EditorWindow
{
    private StoryGraphView graphView;
    private readonly List<StoryNodeData> masterStoryNodes = new List<StoryNodeData>();
    private readonly List<StoryGraphNode> visualNodes = new List<StoryGraphNode>();
    private bool isReloadingGraph;

    [MenuItem("Window/Story Graph Editor")]
    public static void OpenWindow()
    {
        var window = GetWindow<StoryGraphEditorWindow>();
        window.titleContent = new GUIContent("Story Graph");
    }

    private void OnEnable()
    {
        CreateGraphView();
        ReloadGraphFromAssets();
    }

    private void OnDisable()
    {
        if (graphView != null)
        {
            graphView.EdgeCreated -= HandleEdgeCreated;
            graphView.EdgeRemoved -= HandleEdgeRemoved;
            rootVisualElement.Remove(graphView);
            graphView = null;
        }
    }

    private void OnFocus()
    {
        // Pull the newest ScriptableObject values whenever the window becomes active again.
        ReloadGraphFromAssets();
    }

    private void OnProjectChange()
    {
        ReloadGraphFromAssets();
    }

    private void CreateGraphView()
    {
        graphView = new StoryGraphView { style = { flexGrow = 1 } };
        graphView.EdgeCreated += HandleEdgeCreated;
        graphView.EdgeRemoved += HandleEdgeRemoved;
        rootVisualElement.Add(graphView);
    }

    private void ReloadGraphFromAssets()
    {
        if (graphView == null)
        {
            return;
        }

        isReloadingGraph = true;
        try
        {
            graphView.DeleteElements(graphView.graphElements.ToList());
        }
        finally
        {
            isReloadingGraph = false;
        }

        visualNodes.Clear();
        LoadRealDatabases();
        PopulateGraph();
    }

    private void LoadRealDatabases()
    {
        masterStoryNodes.Clear();

        string[] dialogueDBGuids = AssetDatabase.FindAssets("t:DialogueDatabaseSO");
        foreach (string guid in dialogueDBGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var db = AssetDatabase.LoadAssetAtPath<DialogueDatabaseSO>(path);
            if (db == null) continue;

            for (int i = 0; i < db.dialogueEntries.Count; i++)
            {
                DialogueEntry entry = db.dialogueEntries[i];
                if (entry == null) continue;

                var node = new StoryNodeData
                {
                    nodeName = $"Dialog: {entry.key}",
                    nodeType = NodeType.Dialogue,
                    dialogueKey = entry.key,
                    requiredFlag = string.Empty,
                    resultFlag = entry.resultFlag,
                    sourceIndex = i,
                    descriptionText = entry.conversationLines != null && entry.conversationLines.Length > 0 ? entry.conversationLines[0] : string.Empty,
                    dialogueChoices = new List<DialogueOptionData>(),
                    originalDialogue = entry,
                    originalDialogueAssetReference = db
                };

                if (entry.choices != null)
                {
                    foreach (DialogueChoice choice in entry.choices)
                    {
                        node.dialogueChoices.Add(new DialogueOptionData
                        {
                            optionText = choice.displayText,
                            resultFlag = choice.resultFlag,
                            nextDialogueKey = choice.nextDialogueKey
                        });
                    }
                }

                masterStoryNodes.Add(node);
            }
        }

        string[] cutsceneDBGuids = AssetDatabase.FindAssets("t:StoryCutsceneDatabaseSO");
        foreach (string guid in cutsceneDBGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var db = AssetDatabase.LoadAssetAtPath<StoryCutsceneDatabaseSO>(path);
            if (db == null) continue;

            SerializedObject serializedDb = new SerializedObject(db);
            SerializedProperty cutsceneDialogues = serializedDb.FindProperty("cutsceneDialogues");
            if (cutsceneDialogues == null || !cutsceneDialogues.isArray) continue;

            for (int i = 0; i < cutsceneDialogues.arraySize; i++)
            {
                SerializedProperty entryProperty = cutsceneDialogues.GetArrayElementAtIndex(i);
                string triggerFlag = entryProperty.FindPropertyRelative("triggerStoryFlag")?.stringValue ?? string.Empty;
                string speakerName = entryProperty.FindPropertyRelative("speakerName")?.stringValue ?? string.Empty;
                string dialogueKey = entryProperty.FindPropertyRelative("dialogueKey")?.stringValue ?? string.Empty;

                masterStoryNodes.Add(new StoryNodeData
                {
                    nodeName = $"Cutscene: {speakerName}",
                    nodeType = NodeType.Cutscene,
                    dialogueKey = dialogueKey,
                    requiredFlag = triggerFlag,
                    resultFlag = string.Empty,
                    sourceIndex = i,
                    descriptionText = $"Speaker: {speakerName} with key {dialogueKey}",
                    originalCutsceneAssetReference = db
                });
            }
        }

        string[] flagDBGuids = AssetDatabase.FindAssets("t:StoryFlagDatabaseSO");
        foreach (string guid in flagDBGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var db = AssetDatabase.LoadAssetAtPath<StoryFlagDatabaseSO>(path);
            if (db == null) continue;

            for (int i = 0; i < db.allFlags.Count; i++)
            {
                string flag = db.allFlags[i];
                if (string.IsNullOrEmpty(flag)) continue;
                if (masterStoryNodes.Any(n => n.resultFlag == flag || n.requiredFlag == flag)) continue;

                masterStoryNodes.Add(new StoryNodeData
                {
                    nodeName = $"Flag: {flag}",
                    nodeType = NodeType.StoryFlag,
                    requiredFlag = flag,
                    resultFlag = flag,
                    sourceIndex = i,
                    descriptionText = "Global Story Milestone",
                    originalStoryFlagAssetReference = db
                });
            }
        }
    }

    private void PopulateGraph()
    {
        foreach (StoryNodeData data in masterStoryNodes)
        {
            var node = new StoryGraphNode(data);
            node.DataChanged += HandleNodeDataChanged;
            graphView.AddElement(node);
            visualNodes.Add(node);
        }

        AutoLayoutStoryGraph(visualNodes);
        ConnectExistingAssetLinks();

        foreach (StoryGraphNode node in visualNodes)
        {
            node.RefreshNodeColor();
        }
    }

    private void ConnectExistingAssetLinks()
    {
        foreach (StoryGraphNode outputNode in visualNodes)
        {
            ConnectFlagToRequiredNode(outputNode.OutputPorts.FirstOrDefault(), outputNode.Data.resultFlag);

            if (outputNode.Data.nodeType != NodeType.Dialogue || outputNode.Data.dialogueChoices == null)
            {
                continue;
            }

            for (int i = 0; i < outputNode.Data.dialogueChoices.Count; i++)
            {
                DialogueOptionData choice = outputNode.Data.dialogueChoices[i];
                int choiceFlagPortIndex = i + 1;

                if (choiceFlagPortIndex < outputNode.OutputPorts.Count)
                {
                    ConnectFlagToRequiredNode(outputNode.OutputPorts[choiceFlagPortIndex], choice.resultFlag);
                }

                if (i < outputNode.NextDialoguePorts.Count)
                {
                    ConnectDialogueKeyToDialogueNode(outputNode.NextDialoguePorts[i], choice.nextDialogueKey);
                }
            }
        }
    }

    private void ConnectFlagToRequiredNode(Port outputPort, string flag)
    {
        if (outputPort == null || string.IsNullOrEmpty(flag)) return;

        foreach (StoryGraphNode inputNode in visualNodes.Where(node => node.Data.requiredFlag == flag))
        {
            ConnectPorts(outputPort, inputNode.InputPort);
        }
    }

    private void ConnectDialogueKeyToDialogueNode(Port outputPort, string dialogueKey)
    {
        if (outputPort == null || string.IsNullOrEmpty(dialogueKey)) return;

        foreach (StoryGraphNode inputNode in visualNodes.Where(node => node.Data.nodeType == NodeType.Dialogue && node.Data.dialogueKey == dialogueKey))
        {
            ConnectPorts(outputPort, inputNode.DialogueInputPort);
        }
    }

    private void ConnectPorts(Port output, Port input)
    {
        if (output == null || input == null || output.connections.Any(edge => edge.input == input)) return;
        Edge edge = output.ConnectTo(input);
        graphView.AddElement(edge);
    }

    private void HandleEdgeCreated(Edge edge)
    {
        if (isReloadingGraph) return;

        StoryGraphNode outputNode = edge.output?.node as StoryGraphNode;
        StoryGraphNode inputNode = edge.input?.node as StoryGraphNode;
        if (outputNode == null || inputNode == null) return;

        RecordUndo(outputNode, "Change Story Graph Connection");

        if (edge.output.portType == typeof(DialogueEntry) && outputNode.Data.nodeType == NodeType.Dialogue)
        {
            int choiceIndex = outputNode.NextDialoguePorts.IndexOf(edge.output);
            if (choiceIndex >= 0 && choiceIndex < outputNode.Data.dialogueChoices.Count)
            {
                outputNode.Data.dialogueChoices[choiceIndex].nextDialogueKey = inputNode.Data.dialogueKey;
            }
        }
        else
        {
            string incomingRequiredFlag = inputNode.Data.requiredFlag;
            int portIndex = outputNode.OutputPorts.IndexOf(edge.output);

            if (outputNode.Data.nodeType == NodeType.Dialogue && portIndex > 0)
            {
                int choiceIndex = portIndex - 1;
                if (choiceIndex >= 0 && choiceIndex < outputNode.Data.dialogueChoices.Count)
                {
                    outputNode.Data.dialogueChoices[choiceIndex].resultFlag = incomingRequiredFlag;
                }
            }
            else
            {
                outputNode.Data.resultFlag = incomingRequiredFlag;
            }
        }

        outputNode.UpdatePortNames();
        outputNode.RefreshNodeColor();
        inputNode.RefreshNodeColor();
        SaveNodeToSourceAsset(outputNode);
    }

    private void HandleEdgeRemoved(Edge edge)
    {
        if (isReloadingGraph) return;

        StoryGraphNode outputNode = edge.output?.node as StoryGraphNode;
        StoryGraphNode inputNode = edge.input?.node as StoryGraphNode;
        if (outputNode == null) return;

        RecordUndo(outputNode, "Remove Story Graph Connection");

        if (edge.output.portType == typeof(DialogueEntry) && outputNode.Data.nodeType == NodeType.Dialogue)
        {
            int choiceIndex = outputNode.NextDialoguePorts.IndexOf(edge.output);
            if (choiceIndex >= 0 && choiceIndex < outputNode.Data.dialogueChoices.Count)
            {
                outputNode.Data.dialogueChoices[choiceIndex].nextDialogueKey = string.Empty;
            }
        }
        else
        {
            int portIndex = outputNode.OutputPorts.IndexOf(edge.output);
            if (outputNode.Data.nodeType == NodeType.Dialogue && portIndex > 0)
            {
                int choiceIndex = portIndex - 1;
                if (choiceIndex >= 0 && choiceIndex < outputNode.Data.dialogueChoices.Count)
                {
                    outputNode.Data.dialogueChoices[choiceIndex].resultFlag = string.Empty;
                }
            }
            else
            {
                outputNode.Data.resultFlag = string.Empty;
            }
        }

        outputNode.UpdatePortNames();
        outputNode.RefreshNodeColor();
        inputNode?.RefreshNodeColor();
        SaveNodeToSourceAsset(outputNode);
    }

    private void HandleNodeDataChanged(StoryGraphNode node)
    {
        RecordUndo(node, "Edit Story Graph Node");
        SaveNodeToSourceAsset(node);
    }

    private void RecordUndo(StoryGraphNode node, string undoName)
    {
        ScriptableObject undoAsset = GetUndoAsset(node);
        if (undoAsset != null)
        {
            Undo.RecordObject(undoAsset, undoName);
        }
    }

    private ScriptableObject GetUndoAsset(StoryGraphNode node)
    {
        return node.Data.originalDialogueAssetReference != null
            ? node.Data.originalDialogueAssetReference
            : node.Data.originalCutsceneAssetReference != null
                ? node.Data.originalCutsceneAssetReference
                : node.Data.originalStoryFlagAssetReference;
    }

    private void SaveNodeToSourceAsset(StoryGraphNode node)
    {
        StoryNodeData data = node.Data;

        if (data.nodeType == NodeType.Dialogue && data.originalDialogueAssetReference is DialogueDatabaseSO dialogueDatabase)
        {
            DialogueEntry entry = data.originalDialogue;
            entry.key = data.dialogueKey;
            entry.resultFlag = data.resultFlag;

            if (entry.choices != null && data.dialogueChoices != null)
            {
                int count = Mathf.Min(entry.choices.Length, data.dialogueChoices.Count);
                for (int i = 0; i < count; i++)
                {
                    entry.choices[i].displayText = data.dialogueChoices[i].optionText;
                    entry.choices[i].resultFlag = data.dialogueChoices[i].resultFlag;
                    entry.choices[i].nextDialogueKey = data.dialogueChoices[i].nextDialogueKey;
                }
            }

            EditorUtility.SetDirty(dialogueDatabase);
            AssetDatabase.SaveAssets();
            return;
        }

        if (data.nodeType == NodeType.StoryFlag && data.originalStoryFlagAssetReference is StoryFlagDatabaseSO storyFlagDatabase)
        {
            if (data.sourceIndex >= 0 && data.sourceIndex < storyFlagDatabase.allFlags.Count)
            {
                string flag = !string.IsNullOrEmpty(data.resultFlag) ? data.resultFlag : data.requiredFlag;
                storyFlagDatabase.allFlags[data.sourceIndex] = flag;
                data.requiredFlag = flag;
                data.resultFlag = flag;
                node.UpdatePortNames();
                EditorUtility.SetDirty(storyFlagDatabase);
                AssetDatabase.SaveAssets();
            }

            return;
        }

        if (data.nodeType == NodeType.Cutscene && data.originalCutsceneAssetReference is StoryCutsceneDatabaseSO cutsceneDatabase)
        {
            SerializedObject serializedDb = new SerializedObject(cutsceneDatabase);
            SerializedProperty cutsceneDialogues = serializedDb.FindProperty("cutsceneDialogues");
            if (cutsceneDialogues != null && data.sourceIndex >= 0 && data.sourceIndex < cutsceneDialogues.arraySize)
            {
                SerializedProperty entryProperty = cutsceneDialogues.GetArrayElementAtIndex(data.sourceIndex);
                entryProperty.FindPropertyRelative("triggerStoryFlag").stringValue = data.requiredFlag;
                entryProperty.FindPropertyRelative("dialogueKey").stringValue = data.dialogueKey;
                serializedDb.ApplyModifiedProperties();
                EditorUtility.SetDirty(cutsceneDatabase);
                AssetDatabase.SaveAssets();
            }
        }
    }

    private void AutoLayoutStoryGraph(List<StoryGraphNode> nodes)
    {
        float startX = 50f;
        float startY = 50f;
        float horizontalSpacing = 400f;
        float verticalSpacing = 260f;

        Dictionary<StoryGraphNode, int> nodeDepths = new Dictionary<StoryGraphNode, int>();
        Queue<StoryGraphNode> queue = new Queue<StoryGraphNode>();

        foreach (StoryGraphNode startNode in nodes.Where(node => string.IsNullOrEmpty(node.Data.requiredFlag)))
        {
            nodeDepths[startNode] = 0;
            queue.Enqueue(startNode);
        }

        while (queue.Count > 0)
        {
            StoryGraphNode currentNode = queue.Dequeue();
            int currentDepth = nodeDepths[currentNode];

            foreach (StoryGraphNode nextNode in GetLinkedNodes(currentNode, nodes))
            {
                int nextDepth = currentDepth + 1;
                if (!nodeDepths.ContainsKey(nextNode))
                {
                    nodeDepths[nextNode] = nextDepth;
                    queue.Enqueue(nextNode);
                }
            }
        }

        int fallbackDepth = 0;
        foreach (StoryGraphNode node in nodes)
        {
            if (!nodeDepths.ContainsKey(node))
            {
                nodeDepths[node] = fallbackDepth;
            }
        }

        foreach (IGrouping<int, StoryGraphNode> depthGroup in nodes.GroupBy(node => nodeDepths[node]))
        {
            int i = 0;
            foreach (StoryGraphNode node in depthGroup)
            {
                node.SetPosition(new Rect(startX + depthGroup.Key * horizontalSpacing, startY + i * verticalSpacing, 300, 220));
                i++;
            }
        }
    }

    private IEnumerable<StoryGraphNode> GetLinkedNodes(StoryGraphNode currentNode, List<StoryGraphNode> nodes)
    {
        foreach (string flag in GetOutgoingFlags(currentNode))
        {
            foreach (StoryGraphNode nextNode in nodes.Where(node => node.Data.requiredFlag == flag))
            {
                yield return nextNode;
            }
        }

        if (currentNode.Data.nodeType == NodeType.Dialogue && currentNode.Data.dialogueChoices != null)
        {
            foreach (DialogueOptionData choice in currentNode.Data.dialogueChoices)
            {
                foreach (StoryGraphNode nextNode in nodes.Where(node => node.Data.nodeType == NodeType.Dialogue && node.Data.dialogueKey == choice.nextDialogueKey))
                {
                    yield return nextNode;
                }
            }
        }
    }

    private List<string> GetOutgoingFlags(StoryGraphNode node)
    {
        List<string> outgoingFlags = new List<string>();

        if (!string.IsNullOrEmpty(node.Data.resultFlag))
        {
            outgoingFlags.Add(node.Data.resultFlag);
        }

        if (node.Data.nodeType == NodeType.Dialogue && node.Data.dialogueChoices != null)
        {
            foreach (DialogueOptionData choice in node.Data.dialogueChoices)
            {
                if (!string.IsNullOrEmpty(choice.resultFlag))
                {
                    outgoingFlags.Add(choice.resultFlag);
                }
            }
        }

        return outgoingFlags;
    }
}
#endif
