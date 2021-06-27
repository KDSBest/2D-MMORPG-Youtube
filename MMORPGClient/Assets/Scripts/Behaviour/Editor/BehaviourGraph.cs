using Assets.Scripts.Behaviour.Data;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Behaviour.Editor
{
    public class BehaviourGraph : EditorWindow
    {
        private BehaviourGraphView graphView;
        private readonly static string displayTitle = "Behvaiour Graph";

		public BlackboardProvider BlackboardProvider { get; private set; }
        private BehaviourGraphData graph = new BehaviourGraphData();

        [MenuItem("KDSBest/Behaviour Graph")]
        public static void CreateGraphViewWindow()
        {
            var window = GetWindow<BehaviourGraph>();
            window.titleContent = new GUIContent(displayTitle);
        }

        private void CreateToolbar()
        {
            var toolbar = new Toolbar();
            toolbar.Add(new Button(Save) { text = "Save" });
            toolbar.Add(new Button(Load) { text = "Load" });

            graphView.Add(toolbar);
        }

        private void Save()
        {
            BlackboardProvider.UpdateGraph();
            graphView.UpdateGraph();

            var path = EditorUtility.SaveFilePanel("Save Behaviour", string.Empty, string.Empty, "behaviour");

            if (path.Length != 0)
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(graph, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    Formatting = Formatting.Indented
                }));
            }
        }

        private void Load()
		{
            var path = EditorUtility.OpenFilePanel("Open Behaviour", string.Empty, "behaviour");

            if (path.Length != 0)
            {
                graph = JsonConvert.DeserializeObject<BehaviourGraphData>(File.ReadAllText(path), new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
                BlackboardProvider.OnGraphChanged(graph);
                graphView.OnGraphChanged(graph);
            }
        }

        private void OnEnable()
        {
            graphView = new BehaviourGraphView(graph)
            {
                name = displayTitle
            };

            CreateToolbar();
            CreateMiniMap();
            CreateSearchWindow();

            BlackboardProvider = new BlackboardProvider(graph);
			graphView.StretchToParentSize();
			graphView.Add(BlackboardProvider.Blackboard);
			rootVisualElement.Add(graphView);
        }

        private void CreateSearchWindow()
        {
            var searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            searchWindow.Configure(this, graphView);
            graphView.nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        private void CreateMiniMap()
        {
            var miniMap = new MiniMap { anchored = true };
            miniMap.SetPosition(new Rect(250, 30, miniMap.maxWidth, miniMap.maxHeight));
            miniMap.capabilities |= Capabilities.Movable;
            graphView.Add(miniMap);
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(graphView);
        }
    }
}