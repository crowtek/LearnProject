#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class StoryGraphEditorWindow : EditorWindow
{
    private const string LayoutAssetPath = "Assets/Scripts/Editor/StoryGraphLayout.asset";

    private StoryGraphView graphView;
    private IMGUIContainer toolbarContainer;
    private IMGUIContainer sidePanelContainer;
    private StoryGraphLayoutSO layoutAsset;

    private readonly List<StoryNodeData> masterStoryNodes = new List<StoryNodeData>();
    private readonly List<StoryGraphNode> visualNodes = new List<StoryGraphNode>();
    private readonly List<string> validationMessages = new List<string>();
    private readonly Dictionary<StoryGraphNode, List<string>> validationMessagesByNode = new Dictionary<StoryGraphNode, List<string>>();
    private readonly HashSet<StoryGraphNode> dirtyNodes = new HashSet<StoryGraphNode>();
    private readonly HashSet<ScriptableObject> dirtyAssets = new HashSet<ScriptableObject>();
    private readonly HashSet<string> simulationFlags = new HashSet<string>();

    private DialogueDatabaseSO selectedDialogueDatabase;
    private StoryCutsceneDatabaseSO selectedCutsceneDatabase;
    private StoryFlagDatabaseSO selectedStoryFlagDatabase;
    private bool showAllDatabases = true;
    private bool simulationEnabled;
    private string searchText = string.Empty;
    private Vector2 panelScroll;
    private bool isReloadingGraph;
    private bool applyingSavedLayout;

    [MenuItem("Window/Story Graph Editor")]
    public static void OpenWindow()
    {
        var window = GetWindow<StoryGraphEditorWindow>();
        window.titleContent = new GUIContent("Story Graph");
    }

    private void OnEnable()
    {
        EnsureLayoutAsset();
        rootVisualElement.Clear();
        CreateGraphView();
        ReloadGraphFromAssets();
    }

    private void OnDisable()
    {
        if (graphView != null)
        {
            graphView.EdgeCreated -= HandleEdgeCreated;
            graphView.EdgeRemoved -= HandleEdgeRemoved;
            graphView.ElementMoved -= HandleElementMoved;
            graphView = null;
        }
    }

    private void OnFocus()
    {
        if (dirtyNodes.Count == 0)
        {
            ReloadGraphFromAssets();
        }
    }

    private void OnProjectChange()
    {
        if (dirtyNodes.Count == 0)
        {
            ReloadGraphFromAssets();
        }
    }

    private void CreateGraphView()
    {
        toolbarContainer = new IMGUIContainer(DrawToolbar) { style = { flexShrink = 0 } };
        rootVisualElement.Add(toolbarContainer);

        VisualElement body = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Row } };
        graphView = new StoryGraphView { style = { flexGrow = 1 } };
        graphView.EdgeCreated += HandleEdgeCreated;
        graphView.EdgeRemoved += HandleEdgeRemoved;
        graphView.ElementMoved += HandleElementMoved;
        body.Add(graphView);

        sidePanelContainer = new IMGUIContainer(DrawSidePanel)
        {
            style =
            {
                width = 360,
                flexShrink = 0,
                borderLeftWidth = 1,
                borderLeftColor = new Color(0.2f, 0.2f, 0.2f)
            }
        };
        body.Add(sidePanelContainer);
        rootVisualElement.Add(body);
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginVertical(EditorStyles.toolbar);
        EditorGUILayout.BeginHorizontal();

        bool newShowAll = GUILayout.Toggle(showAllDatabases, "Show All Databases", EditorStyles.toolbarButton, GUILayout.Width(135));
        if (newShowAll != showAllDatabases)
        {
            showAllDatabases = newShowAll;
            ReloadGraphFromAssets();
        }

        if (GUILayout.Button("Reload", EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            if (EditorUtility.DisplayDialog("Reload Story Graph", "Reloading discards unsaved graph edits. Continue?", "Reload", "Cancel"))
            {
                dirtyNodes.Clear();
                dirtyAssets.Clear();
                ReloadGraphFromAssets();
            }
        }

        GUI.enabled = dirtyNodes.Count > 0;
        if (GUILayout.Button($"Save Graph ({dirtyNodes.Count})", EditorStyles.toolbarButton, GUILayout.Width(110)))
        {
            SaveDirtyNodes();
        }
        GUI.enabled = true;

        if (GUILayout.Button("Auto Layout", EditorStyles.toolbarButton, GUILayout.Width(85)))
        {
            AutoLayoutStoryGraph(visualNodes);
            SaveAllNodePositions();
        }

        GUILayout.Space(10);
        DrawSearchControls();

        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField(validationMessages.Count == 0 ? "No validation errors" : $"{validationMessages.Count} validation issue(s)", GUILayout.Width(170));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        DrawDatabaseField("Dialogue", selectedDialogueDatabase, value => selectedDialogueDatabase = value, 230);
        DrawDatabaseField("Cutscenes", selectedCutsceneDatabase, value => selectedCutsceneDatabase = value, 230);
        DrawDatabaseField("Flags", selectedStoryFlagDatabase, value => selectedStoryFlagDatabase = value, 230);
        if (GUILayout.Button("Create Dialogue", EditorStyles.toolbarButton, GUILayout.Width(115))) CreateDialogueNode();
        if (GUILayout.Button("Create Cutscene", EditorStyles.toolbarButton, GUILayout.Width(115))) CreateCutsceneNode();
        if (GUILayout.Button("Create Flag", EditorStyles.toolbarButton, GUILayout.Width(95))) CreateFlagNode();
        if (GUILayout.Button("Delete Selected", EditorStyles.toolbarButton, GUILayout.Width(115))) DeleteSelectedNodes();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private void DrawSearchControls()
    {
        EditorGUILayout.LabelField("Search", GUILayout.Width(45));
        string newSearch = EditorGUILayout.TextField(searchText, EditorStyles.toolbarSearchField, GUILayout.Width(190));
        if (newSearch != searchText)
        {
            searchText = newSearch;
            ApplySearchHighlight();
        }

        if (GUILayout.Button("Focus", EditorStyles.toolbarButton, GUILayout.Width(55)))
        {
            FocusFirstSearchMatch();
        }
    }

    private void DrawDatabaseField<T>(string label, T currentValue, System.Action<T> assign, float width) where T : ScriptableObject
    {
        EditorGUI.BeginChangeCheck();
        T newValue = (T)EditorGUILayout.ObjectField(label, currentValue, typeof(T), false, GUILayout.Width(width));
        if (EditorGUI.EndChangeCheck())
        {
            assign(newValue);
            showAllDatabases = false;
            ReloadGraphFromAssets();
        }
    }

    private void DrawSidePanel()
    {
        panelScroll = EditorGUILayout.BeginScrollView(panelScroll);
        EditorGUILayout.LabelField("Simulation", EditorStyles.boldLabel);
        bool newSimulationEnabled = EditorGUILayout.Toggle("Enable Simulation", simulationEnabled);
        if (newSimulationEnabled != simulationEnabled)
        {
            simulationEnabled = newSimulationEnabled;
            ApplySimulation();
        }

        foreach (string flag in GetKnownFlags().OrderBy(flag => flag))
        {
            bool active = simulationFlags.Contains(flag);
            bool newActive = EditorGUILayout.ToggleLeft(flag, active);
            if (newActive != active)
            {
                if (newActive) simulationFlags.Add(flag); else simulationFlags.Remove(flag);
                ApplySimulation();
            }
        }

        if (GUILayout.Button("Clear Simulation Flags"))
        {
            simulationFlags.Clear();
            ApplySimulation();
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
        if (validationMessages.Count == 0)
        {
            EditorGUILayout.HelpBox("No issues found.", MessageType.Info);
        }
        else
        {
            foreach (string message in validationMessages)
            {
                EditorGUILayout.HelpBox(message, MessageType.Warning);
            }
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Authoring Notes", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Flag edges show shared story-flag dependencies. Dialogue Next edges are direct dialogue jumps. Use Save Graph to write pending text/edge edits.", MessageType.None);
        EditorGUILayout.EndScrollView();
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
        validationMessages.Clear();
        validationMessagesByNode.Clear();
        LoadRealDatabases();
        PopulateGraph();
        ApplySearchHighlight();
        ApplySimulation();
        ValidateStoryGraph();
    }

    private void LoadRealDatabases()
    {
        masterStoryNodes.Clear();
        List<DialogueDatabaseSO> dialogueDatabases = GetDatabases(selectedDialogueDatabase, "t:DialogueDatabaseSO");
        List<StoryCutsceneDatabaseSO> cutsceneDatabases = GetDatabases(selectedCutsceneDatabase, "t:StoryCutsceneDatabaseSO");
        List<StoryFlagDatabaseSO> flagDatabases = GetDatabases(selectedStoryFlagDatabase, "t:StoryFlagDatabaseSO");

        foreach (DialogueDatabaseSO db in dialogueDatabases)
        {
            string assetGuid = GetAssetGuid(db);
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
                    requiredFlags = new List<string>(),
                    blockedFlags = new List<string>(),
                    originalDialogue = entry,
                    originalDialogueAssetReference = db,
                    SourceAssetGuid = assetGuid
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

        foreach (StoryCutsceneDatabaseSO db in cutsceneDatabases)
        {
            string assetGuid = GetAssetGuid(db);
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
                    requiredFlags = string.IsNullOrEmpty(triggerFlag) ? new List<string>() : new List<string> { triggerFlag },
                    blockedFlags = new List<string>(),
                    sourceIndex = i,
                    descriptionText = $"Speaker: {speakerName} with key {dialogueKey}",
                    originalCutsceneAssetReference = db,
                    SourceAssetGuid = assetGuid
                });
            }
        }

        foreach (StoryFlagDatabaseSO db in flagDatabases)
        {
            string assetGuid = GetAssetGuid(db);
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
                    requiredFlags = new List<string> { flag },
                    blockedFlags = new List<string>(),
                    sourceIndex = i,
                    descriptionText = "Global Story Milestone",
                    originalStoryFlagAssetReference = db,
                    SourceAssetGuid = assetGuid
                });
            }
        }
    }

    private List<T> GetDatabases<T>(T selectedAsset, string filter) where T : ScriptableObject
    {
        if (!showAllDatabases && selectedAsset != null)
        {
            return new List<T> { selectedAsset };
        }

        List<T> databases = new List<T>();
        foreach (string guid in AssetDatabase.FindAssets(filter))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T db = AssetDatabase.LoadAssetAtPath<T>(path);
            if (db != null)
            {
                databases.Add(db);
            }
        }

        if (selectedAsset == null && databases.Count > 0)
        {
            if (typeof(T) == typeof(DialogueDatabaseSO)) selectedDialogueDatabase = databases[0] as DialogueDatabaseSO;
            if (typeof(T) == typeof(StoryCutsceneDatabaseSO)) selectedCutsceneDatabase = databases[0] as StoryCutsceneDatabaseSO;
            if (typeof(T) == typeof(StoryFlagDatabaseSO)) selectedStoryFlagDatabase = databases[0] as StoryFlagDatabaseSO;
        }

        return databases;
    }

    private void PopulateGraph()
    {
        foreach (StoryNodeData data in masterStoryNodes)
        {
            var node = new StoryGraphNode(data);
            node.DataChanged += HandleNodeDataChanged;
            node.StructureChanged += HandleNodeStructureChanged;
            graphView.AddElement(node);
            visualNodes.Add(node);
        }

        if (!ApplySavedLayout())
        {
            AutoLayoutStoryGraph(visualNodes);
            SaveAllNodePositions();
        }

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

        foreach (StoryGraphNode inputNode in visualNodes.Where(node => node.Data.GetAllRequiredFlags().Contains(flag)))
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

        if (edge.output.portType == typeof(DialogueEntry) && outputNode.Data.nodeType == NodeType.Dialogue)
        {
            RecordUndo(outputNode, "Change Story Graph Dialogue Connection");
            int choiceIndex = outputNode.NextDialoguePorts.IndexOf(edge.output);
            if (choiceIndex >= 0 && choiceIndex < outputNode.Data.dialogueChoices.Count)
            {
                outputNode.Data.dialogueChoices[choiceIndex].nextDialogueKey = inputNode.Data.dialogueKey;
                MarkNodeDirty(outputNode);
            }
        }
        else
        {
            HandleFlagEdgeCreated(edge, outputNode, inputNode);
        }

        outputNode.UpdatePortNames();
        inputNode.UpdatePortNames();
        RebuildInferredEdges();
        ValidateStoryGraph();
        ApplySimulation();
    }

    private void HandleFlagEdgeCreated(Edge edge, StoryGraphNode outputNode, StoryGraphNode inputNode)
    {
        int portIndex = outputNode.OutputPorts.IndexOf(edge.output);
        string outgoingFlag = GetPortFlag(outputNode, portIndex);
        string incomingRequiredFlag = inputNode.Data.requiredFlag;

        if (string.IsNullOrEmpty(incomingRequiredFlag) && !string.IsNullOrEmpty(outgoingFlag))
        {
            RecordUndo(inputNode, "Set Story Graph Requirement");
            inputNode.Data.requiredFlag = outgoingFlag;
            if (inputNode.Data.requiredFlags == null) inputNode.Data.requiredFlags = new List<string>();
            if (!inputNode.Data.requiredFlags.Contains(outgoingFlag)) inputNode.Data.requiredFlags.Add(outgoingFlag);
            MarkNodeDirty(inputNode);
            return;
        }

        if (string.IsNullOrEmpty(incomingRequiredFlag))
        {
            Debug.LogWarning("Cannot create a flag edge when both the output flag and target required flag are empty.");
            return;
        }

        RecordUndo(outputNode, "Change Story Graph Flag Connection");
        if (outputNode.Data.nodeType == NodeType.Dialogue && portIndex > 0)
        {
            int choiceIndex = portIndex - 1;
            if (choiceIndex >= 0 && choiceIndex < outputNode.Data.dialogueChoices.Count)
            {
                outputNode.Data.dialogueChoices[choiceIndex].resultFlag = incomingRequiredFlag;
                MarkNodeDirty(outputNode);
            }
        }
        else
        {
            outputNode.Data.resultFlag = incomingRequiredFlag;
            if (outputNode.Data.nodeType == NodeType.StoryFlag)
            {
                outputNode.Data.requiredFlag = incomingRequiredFlag;
            }
            MarkNodeDirty(outputNode);
        }
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

        MarkNodeDirty(outputNode);
        outputNode.UpdatePortNames();
        outputNode.RefreshNodeColor();
        inputNode?.RefreshNodeColor();
        ValidateStoryGraph();
        ApplySimulation();
    }

    private void HandleNodeDataChanged(StoryGraphNode node)
    {
        RecordUndo(node, "Edit Story Graph Node");
        MarkNodeDirty(node);
        RebuildInferredEdges();
        ValidateStoryGraph();
        ApplySearchHighlight();
        ApplySimulation();
    }

    private void HandleNodeStructureChanged(StoryGraphNode node)
    {
        RecordUndo(node, "Edit Story Graph Choices");
        MarkNodeDirty(node);
        ReloadGraphFromMemory();
    }

    private void ReloadGraphFromMemory()
    {
        Dictionary<string, Rect> positions = visualNodes
            .GroupBy(GetLayoutKey)
            .ToDictionary(group => group.Key, group => group.Last().GetPosition());
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
        foreach (StoryNodeData data in masterStoryNodes)
        {
            var node = new StoryGraphNode(data);
            node.DataChanged += HandleNodeDataChanged;
            node.StructureChanged += HandleNodeStructureChanged;
            graphView.AddElement(node);
            if (positions.TryGetValue(GetLayoutKey(node), out Rect rect))
            {
                node.SetPosition(rect);
            }
            visualNodes.Add(node);
        }

        ConnectExistingAssetLinks();
        ApplySearchHighlight();
        ValidateStoryGraph();
        ApplySimulation();
    }

    private void RebuildInferredEdges()
    {
        isReloadingGraph = true;
        try
        {
            graphView.DeleteElements(graphView.edges.ToList());
            ConnectExistingAssetLinks();
        }
        finally
        {
            isReloadingGraph = false;
        }

        foreach (StoryGraphNode node in visualNodes)
        {
            node.RefreshNodeColor();
        }
    }

    private void MarkNodeDirty(StoryGraphNode node)
    {
        dirtyNodes.Add(node);
        ScriptableObject asset = GetUndoAsset(node);
        if (asset != null)
        {
            dirtyAssets.Add(asset);
            EditorUtility.SetDirty(asset);
        }
    }

    private void SaveDirtyNodes()
    {
        foreach (StoryGraphNode node in dirtyNodes.ToList())
        {
            SaveNodeToSourceAsset(node);
        }

        foreach (ScriptableObject asset in dirtyAssets)
        {
            if (asset != null)
            {
                EditorUtility.SetDirty(asset);
            }
        }

        AssetDatabase.SaveAssets();
        dirtyNodes.Clear();
        dirtyAssets.Clear();
        ReloadGraphFromAssets();
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
            if (entry == null && data.sourceIndex >= 0 && data.sourceIndex < dialogueDatabase.dialogueEntries.Count)
            {
                entry = dialogueDatabase.dialogueEntries[data.sourceIndex];
            }

            if (entry == null) return;
            entry.key = data.dialogueKey;
            entry.resultFlag = data.resultFlag;
            entry.choices = data.dialogueChoices == null
                ? new DialogueChoice[0]
                : data.dialogueChoices.Select(choice => new DialogueChoice
                {
                    displayText = choice.optionText,
                    resultFlag = choice.resultFlag,
                    nextDialogueKey = choice.nextDialogueKey
                }).ToArray();

            EditorUtility.SetDirty(dialogueDatabase);
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
            }
        }
    }

    private void CreateDialogueNode()
    {
        DialogueDatabaseSO db = selectedDialogueDatabase ?? AssetDatabase.FindAssets("t:DialogueDatabaseSO")
            .Select(guid => AssetDatabase.LoadAssetAtPath<DialogueDatabaseSO>(AssetDatabase.GUIDToAssetPath(guid)))
            .FirstOrDefault(database => database != null);
        if (db == null)
        {
            Debug.LogWarning("Create a DialogueDatabaseSO before adding dialogue nodes.");
            return;
        }

        Undo.RecordObject(db, "Create Dialogue Node");
        string key = GenerateUniqueDialogueKey(db, "NewDialogue");
        db.dialogueEntries.Add(new DialogueEntry { key = key, conversationLines = new[] { "New dialogue line." }, choices = new DialogueChoice[0] });
        selectedDialogueDatabase = db;
        showAllDatabases = false;
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        ReloadGraphFromAssets();
    }

    private void CreateCutsceneNode()
    {
        StoryCutsceneDatabaseSO db = selectedCutsceneDatabase ?? AssetDatabase.FindAssets("t:StoryCutsceneDatabaseSO")
            .Select(guid => AssetDatabase.LoadAssetAtPath<StoryCutsceneDatabaseSO>(AssetDatabase.GUIDToAssetPath(guid)))
            .FirstOrDefault(database => database != null);
        if (db == null)
        {
            Debug.LogWarning("Create a StoryCutsceneDatabaseSO before adding cutscene nodes.");
            return;
        }

        Undo.RecordObject(db, "Create Cutscene Node");
        SerializedObject serializedDb = new SerializedObject(db);
        SerializedProperty cutsceneDialogues = serializedDb.FindProperty("cutsceneDialogues");
        if (cutsceneDialogues == null) return;
        int index = cutsceneDialogues.arraySize;
        cutsceneDialogues.InsertArrayElementAtIndex(index);
        SerializedProperty entry = cutsceneDialogues.GetArrayElementAtIndex(index);
        entry.FindPropertyRelative("triggerStoryFlag").stringValue = GenerateUniqueFlag("NewCutsceneFlag");
        entry.FindPropertyRelative("speakerName").stringValue = "New Speaker";
        entry.FindPropertyRelative("dialogueKey").stringValue = selectedDialogueDatabase != null && selectedDialogueDatabase.dialogueEntries.Count > 0 ? selectedDialogueDatabase.dialogueEntries[0].key : string.Empty;
        serializedDb.ApplyModifiedProperties();
        selectedCutsceneDatabase = db;
        showAllDatabases = false;
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        ReloadGraphFromAssets();
    }

    private void CreateFlagNode()
    {
        StoryFlagDatabaseSO db = selectedStoryFlagDatabase ?? AssetDatabase.FindAssets("t:StoryFlagDatabaseSO")
            .Select(guid => AssetDatabase.LoadAssetAtPath<StoryFlagDatabaseSO>(AssetDatabase.GUIDToAssetPath(guid)))
            .FirstOrDefault(database => database != null);
        if (db == null)
        {
            Debug.LogWarning("Create a StoryFlagDatabaseSO before adding story flags.");
            return;
        }

        Undo.RecordObject(db, "Create Story Flag");
        db.allFlags.Add(GenerateUniqueFlag("NewStoryFlag"));
        selectedStoryFlagDatabase = db;
        showAllDatabases = false;
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        ReloadGraphFromAssets();
    }

    private void DeleteSelectedNodes()
    {
        List<StoryGraphNode> selectedNodes = graphView.selection.OfType<StoryGraphNode>().ToList();
        if (selectedNodes.Count == 0)
        {
            return;
        }

        if (!EditorUtility.DisplayDialog("Delete Story Nodes", $"Delete {selectedNodes.Count} selected story node(s) from their source assets?", "Delete", "Cancel"))
        {
            return;
        }

        foreach (StoryGraphNode node in selectedNodes)
        {
            DeleteNodeFromSourceAsset(node);
        }

        AssetDatabase.SaveAssets();
        dirtyNodes.Clear();
        dirtyAssets.Clear();
        ReloadGraphFromAssets();
    }

    private void DeleteNodeFromSourceAsset(StoryGraphNode node)
    {
        StoryNodeData data = node.Data;
        if (data.nodeType == NodeType.Dialogue && data.originalDialogueAssetReference is DialogueDatabaseSO dialogueDatabase)
        {
            Undo.RecordObject(dialogueDatabase, "Delete Dialogue Node");
            if (data.sourceIndex >= 0 && data.sourceIndex < dialogueDatabase.dialogueEntries.Count)
            {
                dialogueDatabase.dialogueEntries.RemoveAt(data.sourceIndex);
                EditorUtility.SetDirty(dialogueDatabase);
            }
        }
        else if (data.nodeType == NodeType.StoryFlag && data.originalStoryFlagAssetReference is StoryFlagDatabaseSO flagDatabase)
        {
            Undo.RecordObject(flagDatabase, "Delete Story Flag Node");
            if (data.sourceIndex >= 0 && data.sourceIndex < flagDatabase.allFlags.Count)
            {
                flagDatabase.allFlags.RemoveAt(data.sourceIndex);
                EditorUtility.SetDirty(flagDatabase);
            }
        }
        else if (data.nodeType == NodeType.Cutscene && data.originalCutsceneAssetReference is StoryCutsceneDatabaseSO cutsceneDatabase)
        {
            Undo.RecordObject(cutsceneDatabase, "Delete Cutscene Node");
            SerializedObject serializedDb = new SerializedObject(cutsceneDatabase);
            SerializedProperty cutsceneDialogues = serializedDb.FindProperty("cutsceneDialogues");
            if (cutsceneDialogues != null && data.sourceIndex >= 0 && data.sourceIndex < cutsceneDialogues.arraySize)
            {
                cutsceneDialogues.DeleteArrayElementAtIndex(data.sourceIndex);
                serializedDb.ApplyModifiedProperties();
                EditorUtility.SetDirty(cutsceneDatabase);
            }
        }
    }

    private void ValidateStoryGraph()
    {
        validationMessages.Clear();
        validationMessagesByNode.Clear();
        HashSet<string> knownFlags = GetKnownFlags();
        Dictionary<string, List<StoryGraphNode>> dialogueNodesByKey = visualNodes
            .Where(node => node.Data.nodeType == NodeType.Dialogue && !string.IsNullOrEmpty(node.Data.dialogueKey))
            .GroupBy(node => node.Data.dialogueKey)
            .ToDictionary(group => group.Key, group => group.ToList());

        foreach (KeyValuePair<string, List<StoryGraphNode>> pair in dialogueNodesByKey.Where(pair => pair.Value.Count > 1))
        {
            AddValidation(pair.Value, $"Duplicate dialogue key '{pair.Key}' is used by {pair.Value.Count} dialogue nodes.");
        }

        foreach (StoryGraphNode node in visualNodes)
        {
            foreach (string flag in node.Data.GetAllRequiredFlags())
            {
                if (!knownFlags.Contains(flag)) AddValidation(node, $"{node.Data.nodeName} requires missing story flag '{flag}'.");
            }

            foreach (string flag in node.Data.GetAllBlockedFlags())
            {
                if (!knownFlags.Contains(flag)) AddValidation(node, $"{node.Data.nodeName} blocks on missing story flag '{flag}'.");
            }

            if (!string.IsNullOrEmpty(node.Data.resultFlag) && !knownFlags.Contains(node.Data.resultFlag))
            {
                AddValidation(node, $"{node.Data.nodeName} produces missing story flag '{node.Data.resultFlag}'.");
            }

            if (node.Data.nodeType == NodeType.Cutscene && string.IsNullOrEmpty(node.Data.requiredFlag))
            {
                AddValidation(node, $"{node.Data.nodeName} has no trigger story flag.");
            }

            if (node.Data.nodeType == NodeType.Dialogue && node.Data.dialogueChoices != null)
            {
                foreach (DialogueOptionData choice in node.Data.dialogueChoices)
                {
                    if (!string.IsNullOrEmpty(choice.resultFlag) && !knownFlags.Contains(choice.resultFlag))
                    {
                        AddValidation(node, $"{node.Data.nodeName} choice '{choice.optionText}' produces missing story flag '{choice.resultFlag}'.");
                    }

                    if (!string.IsNullOrEmpty(choice.nextDialogueKey) && !dialogueNodesByKey.ContainsKey(choice.nextDialogueKey))
                    {
                        AddValidation(node, $"{node.Data.nodeName} choice '{choice.optionText}' jumps to missing dialogue key '{choice.nextDialogueKey}'.");
                    }
                }
            }

            bool isStartNode = string.IsNullOrEmpty(node.Data.requiredFlag) && !node.Data.GetAllRequiredFlags().Any();
            bool hasIncoming = HasIncomingStoryLink(node);
            bool hasOutgoing = GetLinkedNodes(node, visualNodes).Any();
            if (!isStartNode && !hasIncoming)
            {
                AddValidation(node, $"{node.Data.nodeName} has no incoming path.");
            }

            if (!hasOutgoing && node.Data.nodeType != NodeType.StoryFlag && string.IsNullOrEmpty(node.Data.resultFlag))
            {
                AddValidation(node, $"{node.Data.nodeName} has no outgoing path and no result flag.");
            }
        }

        foreach (StoryGraphNode node in visualNodes)
        {
            if (validationMessagesByNode.TryGetValue(node, out List<string> nodeMessages))
            {
                node.SetValidationState(true, string.Join("\n", nodeMessages));
            }
            else
            {
                node.SetValidationState(false, string.Empty);
            }
        }
    }

    private void AddValidation(StoryGraphNode node, string message)
    {
        AddValidation(new[] { node }, message);
    }

    private void AddValidation(IEnumerable<StoryGraphNode> nodes, string message)
    {
        validationMessages.Add(message);
        foreach (StoryGraphNode node in nodes)
        {
            if (!validationMessagesByNode.TryGetValue(node, out List<string> messages))
            {
                messages = new List<string>();
                validationMessagesByNode[node] = messages;
            }
            messages.Add(message);
        }
    }

    private bool HasIncomingStoryLink(StoryGraphNode targetNode)
    {
        foreach (StoryGraphNode node in visualNodes)
        {
            if (GetLinkedNodes(node, visualNodes).Contains(targetNode))
            {
                return true;
            }
        }

        return false;
    }

    private void ApplySimulation()
    {
        foreach (StoryGraphNode node in visualNodes)
        {
            if (!simulationEnabled)
            {
                node.SetSimulationState(StoryGraphSimulationState.None);
                continue;
            }

            bool blocked = node.Data.GetAllBlockedFlags().Any(flag => simulationFlags.Contains(flag));
            bool reachable = !blocked && node.Data.GetAllRequiredFlags().All(flag => simulationFlags.Contains(flag));
            bool completed = !string.IsNullOrEmpty(node.Data.resultFlag) && simulationFlags.Contains(node.Data.resultFlag);
            node.SetSimulationState(blocked ? StoryGraphSimulationState.Blocked : completed ? StoryGraphSimulationState.Completed : reachable ? StoryGraphSimulationState.Reachable : StoryGraphSimulationState.Blocked);
        }
    }

    private void ApplySearchHighlight()
    {
        foreach (StoryGraphNode node in visualNodes)
        {
            node.SetSearchMatch(!string.IsNullOrWhiteSpace(searchText) && NodeMatchesSearch(node, searchText));
        }
    }

    private void FocusFirstSearchMatch()
    {
        StoryGraphNode node = visualNodes.FirstOrDefault(candidate => NodeMatchesSearch(candidate, searchText));
        if (node == null) return;
        graphView.ClearSelection();
        graphView.AddToSelection(node);
        graphView.FrameSelection();
    }

    private bool NodeMatchesSearch(StoryGraphNode node, string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return false;
        string lowerQuery = query.ToLowerInvariant();
        IEnumerable<string> values = new[]
        {
            node.Data.nodeName,
            node.Data.dialogueKey,
            node.Data.requiredFlag,
            node.Data.resultFlag,
            node.Data.descriptionText
        }.Concat(node.Data.dialogueChoices == null
            ? Enumerable.Empty<string>()
            : node.Data.dialogueChoices.SelectMany(choice => new[] { choice.optionText, choice.resultFlag, choice.nextDialogueKey }));

        return values.Any(value => !string.IsNullOrEmpty(value) && value.ToLowerInvariant().Contains(lowerQuery));
    }

    private HashSet<string> GetKnownFlags()
    {
        HashSet<string> flags = new HashSet<string>();
        IEnumerable<StoryFlagDatabaseSO> databases = GetDatabases(selectedStoryFlagDatabase, "t:StoryFlagDatabaseSO");
        foreach (StoryFlagDatabaseSO db in databases)
        {
            foreach (string flag in db.allFlags)
            {
                if (!string.IsNullOrEmpty(flag)) flags.Add(flag);
            }
        }

        foreach (StoryGraphNode node in visualNodes)
        {
            if (node.Data.nodeType == NodeType.StoryFlag && !string.IsNullOrEmpty(node.Data.resultFlag)) flags.Add(node.Data.resultFlag);
        }

        return flags;
    }

    private void EnsureLayoutAsset()
    {
        layoutAsset = AssetDatabase.LoadAssetAtPath<StoryGraphLayoutSO>(LayoutAssetPath);
        if (layoutAsset != null) return;

        layoutAsset = CreateInstance<StoryGraphLayoutSO>();
        AssetDatabase.CreateAsset(layoutAsset, LayoutAssetPath);
        AssetDatabase.SaveAssets();
    }

    private bool ApplySavedLayout()
    {
        if (layoutAsset == null || layoutAsset.nodePositions.Count == 0) return false;
        Dictionary<string, Vector2> positions = layoutAsset.nodePositions
            .Where(entry => !string.IsNullOrEmpty(entry.nodeKey))
            .GroupBy(entry => entry.nodeKey)
            .ToDictionary(group => group.Key, group => group.Last().position);

        bool positionedAny = false;
        applyingSavedLayout = true;
        try
        {
            foreach (StoryGraphNode node in visualNodes)
            {
                if (positions.TryGetValue(GetLayoutKey(node), out Vector2 position))
                {
                    node.SetPosition(new Rect(position, node.GetPosition().size));
                    positionedAny = true;
                }
            }
        }
        finally
        {
            applyingSavedLayout = false;
        }

        return positionedAny;
    }

    private void HandleElementMoved(GraphElement element)
    {
        if (applyingSavedLayout || isReloadingGraph || !(element is StoryGraphNode node)) return;
        SaveNodePosition(node);
    }

    private void SaveAllNodePositions()
    {
        foreach (StoryGraphNode node in visualNodes)
        {
            SaveNodePosition(node);
        }
    }

    private void SaveNodePosition(StoryGraphNode node)
    {
        if (layoutAsset == null) return;
        string key = GetLayoutKey(node);
        StoryGraphNodePosition entry = layoutAsset.nodePositions.FirstOrDefault(item => item.nodeKey == key);
        if (entry == null)
        {
            entry = new StoryGraphNodePosition { nodeKey = key };
            layoutAsset.nodePositions.Add(entry);
        }

        entry.position = node.GetPosition().position;
        EditorUtility.SetDirty(layoutAsset);
    }

    private string GetLayoutKey(StoryGraphNode node)
    {
        return node == null ? string.Empty : GetLayoutKey(node.Data);
    }

    private string GetLayoutKey(StoryNodeData data)
    {
        string source = data.SourceAssetGuid ?? string.Empty;
        switch (data.nodeType)
        {
            case NodeType.Dialogue:
                return $"Dialogue:{source}:{data.dialogueKey}:{data.sourceIndex}";
            case NodeType.Cutscene:
                return $"Cutscene:{source}:{data.sourceIndex}:{data.requiredFlag}";
            case NodeType.StoryFlag:
                return $"Flag:{source}:{data.resultFlag}:{data.sourceIndex}";
            default:
                return $"Unknown:{source}:{data.sourceIndex}";
        }
    }

    private string GetAssetGuid(Object asset)
    {
        string path = AssetDatabase.GetAssetPath(asset);
        return string.IsNullOrEmpty(path) ? string.Empty : AssetDatabase.AssetPathToGUID(path);
    }

    private string GenerateUniqueDialogueKey(DialogueDatabaseSO db, string prefix)
    {
        HashSet<string> keys = new HashSet<string>(db.dialogueEntries.Where(entry => entry != null).Select(entry => entry.key));
        return GenerateUniqueString(keys, prefix);
    }

    private string GenerateUniqueFlag(string prefix)
    {
        return GenerateUniqueString(GetKnownFlags(), prefix);
    }

    private static string GenerateUniqueString(HashSet<string> existingValues, string prefix)
    {
        if (!existingValues.Contains(prefix)) return prefix;
        int index = 1;
        while (existingValues.Contains($"{prefix}_{index}")) index++;
        return $"{prefix}_{index}";
    }

    private string GetPortFlag(StoryGraphNode node, int portIndex)
    {
        if (node == null || portIndex < 0) return string.Empty;
        if (node.Data.nodeType == NodeType.Dialogue && portIndex > 0)
        {
            int choiceIndex = portIndex - 1;
            return choiceIndex >= 0 && choiceIndex < node.Data.dialogueChoices.Count ? node.Data.dialogueChoices[choiceIndex].resultFlag : string.Empty;
        }

        return node.Data.resultFlag;
    }

    private void AutoLayoutStoryGraph(List<StoryGraphNode> nodes)
    {
        if (nodes == null || nodes.Count == 0)
        {
            return;
        }

        const float startX = 50f;
        const float startY = 50f;
        const float horizontalSpacing = 400f;
        const float verticalSpacing = 260f;
        const float nodeWidth = 300f;
        const float nodeHeight = 220f;

        Dictionary<StoryGraphNode, List<StoryGraphNode>> linkedNodesByParent = BuildLinkedNodeMap(nodes);
        Dictionary<StoryGraphNode, int> incomingConnectionCounts = BuildIncomingConnectionCounts(nodes, linkedNodesByParent);
        List<StoryGraphNode> rootNodes = GetLayoutRootNodes(nodes, incomingConnectionCounts);
        Dictionary<StoryGraphNode, int> nodeDepths = CalculateNodeDepths(nodes, rootNodes, linkedNodesByParent);

        HashSet<StoryGraphNode> placedNodes = new HashSet<StoryGraphNode>();
        HashSet<StoryGraphNode> nodesBeingPlaced = new HashSet<StoryGraphNode>();
        Dictionary<StoryGraphNode, float> nodeCenters = new Dictionary<StoryGraphNode, float>();
        float nextAvailableY = startY;

        applyingSavedLayout = true;
        try
        {
            foreach (StoryGraphNode rootNode in rootNodes)
            {
                PlaceNodeBranch(rootNode);
            }

            foreach (StoryGraphNode unplacedNode in nodes.Where(node => !placedNodes.Contains(node)))
            {
                PlaceNodeBranch(unplacedNode);
            }
        }
        finally
        {
            applyingSavedLayout = false;
        }

        float PlaceNodeBranch(StoryGraphNode node)
        {
            if (nodeCenters.TryGetValue(node, out float existingCenter))
            {
                return existingCenter;
            }

            if (!nodesBeingPlaced.Add(node))
            {
                return nextAvailableY;
            }

            List<float> childCenters = new List<float>();
            foreach (StoryGraphNode childNode in linkedNodesByParent[node])
            {
                if (childNode == node || nodesBeingPlaced.Contains(childNode))
                {
                    continue;
                }

                childCenters.Add(PlaceNodeBranch(childNode));
            }

            float centerY;
            if (childCenters.Count > 0)
            {
                centerY = (childCenters[0] + childCenters[childCenters.Count - 1]) * 0.5f;
            }
            else
            {
                centerY = nextAvailableY;
                nextAvailableY += verticalSpacing;
            }

            nodesBeingPlaced.Remove(node);
            placedNodes.Add(node);
            nodeCenters[node] = centerY;

            int depth = nodeDepths.TryGetValue(node, out int calculatedDepth) ? calculatedDepth : 0;
            node.SetPosition(new Rect(startX + depth * horizontalSpacing, centerY, nodeWidth, nodeHeight));
            return centerY;
        }
    }

    private Dictionary<StoryGraphNode, List<StoryGraphNode>> BuildLinkedNodeMap(List<StoryGraphNode> nodes)
    {
        Dictionary<StoryGraphNode, List<StoryGraphNode>> linkedNodesByParent = new Dictionary<StoryGraphNode, List<StoryGraphNode>>();

        foreach (StoryGraphNode node in nodes)
        {
            linkedNodesByParent[node] = GetLinkedNodes(node, nodes).Distinct().ToList();
        }

        return linkedNodesByParent;
    }

    private Dictionary<StoryGraphNode, int> BuildIncomingConnectionCounts(
        List<StoryGraphNode> nodes,
        Dictionary<StoryGraphNode, List<StoryGraphNode>> linkedNodesByParent)
    {
        Dictionary<StoryGraphNode, int> incomingConnectionCounts = nodes.ToDictionary(node => node, _ => 0);

        foreach (List<StoryGraphNode> linkedNodes in linkedNodesByParent.Values)
        {
            foreach (StoryGraphNode linkedNode in linkedNodes)
            {
                incomingConnectionCounts[linkedNode]++;
            }
        }

        return incomingConnectionCounts;
    }

    private List<StoryGraphNode> GetLayoutRootNodes(List<StoryGraphNode> nodes, Dictionary<StoryGraphNode, int> incomingConnectionCounts)
    {
        List<StoryGraphNode> rootNodes = nodes
            .Where(node => incomingConnectionCounts[node] == 0)
            .OrderBy(node => string.IsNullOrEmpty(node.Data.requiredFlag) ? 0 : 1)
            .ThenBy(node => node.Data.nodeName)
            .ToList();

        return rootNodes.Count > 0 ? rootNodes : nodes.ToList();
    }

    private Dictionary<StoryGraphNode, int> CalculateNodeDepths(
        List<StoryGraphNode> nodes,
        List<StoryGraphNode> rootNodes,
        Dictionary<StoryGraphNode, List<StoryGraphNode>> linkedNodesByParent)
    {
        Dictionary<StoryGraphNode, int> nodeDepths = new Dictionary<StoryGraphNode, int>();
        Queue<StoryGraphNode> queue = new Queue<StoryGraphNode>();

        foreach (StoryGraphNode rootNode in rootNodes)
        {
            nodeDepths[rootNode] = 0;
            queue.Enqueue(rootNode);
        }

        while (queue.Count > 0)
        {
            StoryGraphNode currentNode = queue.Dequeue();
            int childDepth = nodeDepths[currentNode] + 1;

            foreach (StoryGraphNode childNode in linkedNodesByParent[currentNode])
            {
                if (nodeDepths.ContainsKey(childNode))
                {
                    continue;
                }

                nodeDepths[childNode] = childDepth;
                queue.Enqueue(childNode);
            }
        }

        foreach (StoryGraphNode node in nodes)
        {
            if (!nodeDepths.ContainsKey(node))
            {
                nodeDepths[node] = 0;
            }
        }

        return nodeDepths;
    }

    private IEnumerable<StoryGraphNode> GetLinkedNodes(StoryGraphNode currentNode, List<StoryGraphNode> nodes)
    {
        foreach (string flag in GetOutgoingFlags(currentNode))
        {
            foreach (StoryGraphNode nextNode in nodes.Where(node => node.Data.GetAllRequiredFlags().Contains(flag)))
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
