#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

public class StoryGraphView : GraphView
{
    public event Action<Edge> EdgeCreated;
    public event Action<Edge> EdgeRemoved;

    public StoryGraphView()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();

        graphViewChanged = OnGraphViewChanged;
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();

        ports.ForEach(port =>
        {
            if (startPort == port || startPort.node == port.node || startPort.direction == port.direction)
            {
                return;
            }

            if (startPort.portType != port.portType)
            {
                return;
            }

            compatiblePorts.Add(port);
        });

        return compatiblePorts;
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        if (graphViewChange.edgesToCreate != null)
        {
            foreach (Edge edge in graphViewChange.edgesToCreate)
            {
                EdgeCreated?.Invoke(edge);
            }
        }

        if (graphViewChange.elementsToRemove != null)
        {
            foreach (GraphElement element in graphViewChange.elementsToRemove)
            {
                if (element is Edge edge)
                {
                    EdgeRemoved?.Invoke(edge);
                }
            }
        }

        return graphViewChange;
    }
}
#endif
