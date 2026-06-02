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
    private List<StoryNodeData> masterStoryNodes = new List<StoryNodeData>();

    [MenuItem("Window/Story Graph Editor")]
    public static void OpenWindow()
    {
        var window = GetWindow<StoryGraphEditorWindow>();
        window.titleContent = new GUIContent("Story Graph");
    }

    private void OnEnable()
    {
        CreateGraphView();
        LoadRealDatabases();
        PopulateGraph();
    }

    private void CreateGraphView()
    {
        graphView = new StoryGraphView { style = { flexGrow = 1 } };
        rootVisualElement.Add(graphView);
    }

    private void LoadRealDatabases()
    {
        masterStoryNodes.Clear();

        // 1. Lade alle DialogueDatabaseSO aus dem Projekt
        string[] dialogueDBGuids = AssetDatabase.FindAssets("t:DialogueDatabaseSO");
        foreach (var guid in dialogueDBGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var db = AssetDatabase.LoadAssetAtPath<DialogueDatabaseSO>(path);
            if (db == null) continue;

            foreach (var entry in db.dialogueEntries)
            {
                var node = new StoryNodeData
                {
                    nodeName = $"Dialog: {entry.key}",
                    nodeType = NodeType.Dialogue,
                    requiredFlag = "", // Dialog-Conditions checken wir gleich separat
                    resultFlag = entry.resultFlag,
                    descriptionText = entry.conversationLines.Length > 0 ? entry.conversationLines[0] : "",
                    dialogueChoices = new List<DialogueOptionData>(),
                    originalDialogue = entry,
                    originalDialogueAssetReference = db
                };

                // Wenn der Dialog eigene Verzweigungen (Choices) hat
                if (entry.choices != null)
                {
                    foreach (var choice in entry.choices)
                    {
                        node.dialogueChoices.Add(new DialogueOptionData
                        {
                            optionText = choice.displayText,
                            resultFlag = choice.resultFlag
                        });
                    }
                }

                masterStoryNodes.Add(node);
            }
        }

        // 2. Lade alle StoryCutsceneDatabaseSO aus dem Projekt
        string[] cutsceneDBGuids = AssetDatabase.FindAssets("t:StoryCutsceneDatabaseSO");
        foreach (var guid in cutsceneDBGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var db = AssetDatabase.LoadAssetAtPath<StoryCutsceneDatabaseSO>(path);
            if (db == null) continue;

            // Da cutsceneDialogues private ist, greifen wir über das Caching-Muster darauf zu oder 
            // spiegeln die Struktur. Falls du sie im SO auf 'public' stellst, kannst du sie direkt loopen:
            // Hier simulieren wir den Zugriff auf deine Cutscene-Einträge:
            var cutsceneField = db.GetType().GetField("cutsceneDialogues", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (cutsceneField != null)
            {
                var list = cutsceneField.GetValue(db) as System.Collections.IEnumerable;
                if (list != null)
                {
                    foreach (var item in list)
                    {
                        var entry = (CutsceneDialogueEntry)item;
                        masterStoryNodes.Add(new StoryNodeData
                        {
                            nodeName = $"Cutscene: {entry.speakerName}",
                            nodeType = NodeType.Cutscene,
                            requiredFlag = entry.triggerStoryFlag, // Cutscene braucht dieses Flag zum Starten
                            resultFlag = "", // Kann durch Folge-Flags erweitert werden
                            descriptionText = $"Speaker: {entry.speakerName} with key {entry.dialogueKey}",
                            originalCutscene = entry
                        });
                    }
                }
            }
        }

        // 3. Füge reine Story-Flags aus deiner globalen Flag-Liste als Hilfs-Nodes hinzu
        string[] flagDBGuids = AssetDatabase.FindAssets("t:StoryFlagDatabaseSO");
        foreach (var guid in flagDBGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var db = AssetDatabase.LoadAssetAtPath<StoryFlagDatabaseSO>(path);
            if (db == null) continue;

            foreach (var flag in db.allFlags)
            {
                // Verhindere doppelte Nodes für reine Flags
                if (masterStoryNodes.Any(n => n.resultFlag == flag || n.requiredFlag == flag)) continue;

                masterStoryNodes.Add(new StoryNodeData
                {
                    nodeName = $"Flag: {flag}",
                    nodeType = NodeType.StoryFlag,
                    requiredFlag = flag,
                    resultFlag = flag,
                    descriptionText = "Global Story Milestone"
                });
            }
        }
    }

    private void PopulateGraph()
    {
        List<StoryGraphNode> visualNodes = new List<StoryGraphNode>();

        // 1. Nodes in GraphView einspeisen
        foreach (var data in masterStoryNodes)
        {
            var vNode = new StoryGraphNode(data);
            graphView.AddElement(vNode);
            visualNodes.Add(vNode);
        }

        // 2. Automatisches Alignment (Von links nach rechts sortiert)
        AutoLayoutStoryGraph(visualNodes);

        // 3. Visuelle Verbindungen (String-Matching)
        foreach (var outputNode in visualNodes)
        {
            foreach (var inputNode in visualNodes)
            {
                if (outputNode == inputNode) continue;

                // Szenario A: Linearer Ausgang matcht Eingang
                if (outputNode.Data.nodeType != NodeType.Dialogue &&
                    !string.IsNullOrEmpty(outputNode.Data.resultFlag) &&
                    outputNode.Data.resultFlag == inputNode.Data.requiredFlag)
                {
                    ConnectPorts(outputNode.OutputPorts.FirstOrDefault(), inputNode.InputPort);
                }

                // Szenario B: Ein Dialog-Choice-Ausgang matcht den Eingang einer Folgnode
                if (outputNode.Data.nodeType == NodeType.Dialogue && outputNode.Data.dialogueChoices != null)
                {
                    for (int c = 0; c < outputNode.Data.dialogueChoices.Count; c++)
                    {
                        var choice = outputNode.Data.dialogueChoices[c];
                        if (!string.IsNullOrEmpty(choice.resultFlag) && choice.resultFlag == inputNode.Data.requiredFlag)
                        {
                            if (c < outputNode.OutputPorts.Count)
                            {
                                ConnectPorts(outputNode.OutputPorts[c], inputNode.InputPort);
                            }
                        }
                    }
                }
            }
        }

        // 4. Unverbundene Nodes validieren (Färbt verwaiste Einträge Gelb)
        foreach (var node in visualNodes)
        {
            node.RefreshNodeColor();
        }
    }

    private void ConnectPorts(Port output, Port input)
    {
        if (output == null || input == null) return;
        Edge edge = output.ConnectTo(input);
        graphView.AddElement(edge);
    }

    private void AutoLayoutStoryGraph(List<StoryGraphNode> visualNodes)
    {
        float startX = 50f;
        float startY = 50f;

        float horizontalSpacing = 360f;
        float verticalSpacing = 220f;

        Dictionary<StoryGraphNode, int> nodeDepths = new Dictionary<StoryGraphNode, int>();

        // 1. Start nodes are nodes without required flags.
        List<StoryGraphNode> startNodes = visualNodes
            .Where(node => string.IsNullOrEmpty(node.Data.requiredFlag))
            .ToList();

        Queue<StoryGraphNode> queue = new Queue<StoryGraphNode>();

        foreach (StoryGraphNode startNode in startNodes)
        {
            nodeDepths[startNode] = 0;
            queue.Enqueue(startNode);
        }

        // 2. Walk through the story graph and calculate depth.
        while (queue.Count > 0)
        {
            StoryGraphNode currentNode = queue.Dequeue();

            int currentDepth = nodeDepths[currentNode];

            List<string> outgoingFlags = GetOutgoingFlags(currentNode);

            foreach (string outgoingFlag in outgoingFlags)
            {
                if (string.IsNullOrEmpty(outgoingFlag))
                    continue;

                List<StoryGraphNode> nextNodes = visualNodes
                    .Where(node => node.Data.requiredFlag == outgoingFlag)
                    .ToList();

                foreach (StoryGraphNode nextNode in nextNodes)
                {
                    int nextDepth = currentDepth + 1;

                    if (!nodeDepths.ContainsKey(nextNode) || nodeDepths[nextNode] < nextDepth)
                    {
                        nodeDepths[nextNode] = nextDepth;
                        queue.Enqueue(nextNode);
                    }
                }
            }
        }

        // 3. Give disconnected nodes their own fallback column.
        int fallbackDepth = 0;

        foreach (StoryGraphNode node in visualNodes)
        {
            if (!nodeDepths.ContainsKey(node))
            {
                nodeDepths[node] = fallbackDepth;
            }
        }

        // 4. Group nodes by depth.
        Dictionary<int, List<StoryGraphNode>> nodesByDepth = visualNodes
            .GroupBy(node => nodeDepths[node])
            .ToDictionary(group => group.Key, group => group.ToList());

        // 5. Position nodes: depth = X column, index in column = Y position.
        foreach (KeyValuePair<int, List<StoryGraphNode>> depthGroup in nodesByDepth)
        {
            int depth = depthGroup.Key;
            List<StoryGraphNode> nodesAtDepth = depthGroup.Value;

            for (int i = 0; i < nodesAtDepth.Count; i++)
            {
                float x = startX + depth * horizontalSpacing;
                float y = startY + i * verticalSpacing;

                nodesAtDepth[i].SetPosition(new Rect(x, y, 260, 160));
            }
        }
    }

    private List<string> GetOutgoingFlags(StoryGraphNode node)
    {
        List<string> outgoingFlags = new List<string>();

        if (node.Data.nodeType == NodeType.Dialogue &&
            node.Data.dialogueChoices != null &&
            node.Data.dialogueChoices.Count > 0)
        {
            foreach (DialogueOptionData choice in node.Data.dialogueChoices)
            {
                if (!string.IsNullOrEmpty(choice.resultFlag))
                {
                    outgoingFlags.Add(choice.resultFlag);
                }
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(node.Data.resultFlag))
            {
                outgoingFlags.Add(node.Data.resultFlag);
            }
        }

        return outgoingFlags;
    }
}
#endif