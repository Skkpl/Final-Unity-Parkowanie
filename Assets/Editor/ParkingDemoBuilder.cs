using System.IO;
using AutomaticParking;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ParkingDemoBuilder
{
    private const string ScenesPath = "Assets/Scenes";
    private const string PrefabsPath = "Assets/Prefabs";
    private const string MaterialsPath = "Assets/Materials/ParkingDemo";

    private static Material asphalt;
    private static Material grass;
    private static Material line;
    private static Material egoCar;
    private static Material parkedCar;
    private static Material obstacle;
    private static Material wall;
    private static Material tire;
    private static Material window;

    [MenuItem("Tools/Parking Project/Build Demo Scenes")]
    public static void BuildDemoScenes()
    {
        EnsureFolders();
        CreateMaterials();
        CreatePrefabs();
        CreateMainMenuScene();
        CreatePerpendicularScene();
        CreateParallelScene(false);
        CreateDynamicLeftParkingScene();
        UpdateBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog(
                "Automatic Parking",
                "Demo scenes, prefabs and build settings were generated.",
                "OK");
        }
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets", "Scripts");
        EnsureFolder("Assets/Scripts", "Parking");
        EnsureFolder("Assets", "Editor");
        EnsureFolder("Assets", "Scenes");
        EnsureFolder("Assets", "Prefabs");
        EnsureFolder("Assets", "Materials");
        EnsureFolder("Assets/Materials", "ParkingDemo");
        EnsureFolder("Assets", "ImportedVisuals");
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = $"{parent}/{child}";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }

    private static void CreateMaterials()
    {
        asphalt = CreateMaterial("Asphalt", new Color(0.18f, 0.19f, 0.20f));
        grass = CreateMaterial("Concrete", new Color(0.48f, 0.48f, 0.43f));
        line = CreateMaterial("ParkingLine", new Color(0.95f, 0.90f, 0.55f));
        egoCar = CreateMaterial("EgoCarBlue", new Color(0.10f, 0.45f, 0.95f));
        parkedCar = CreateMaterial("ParkedCarGrey", new Color(0.38f, 0.40f, 0.43f));
        obstacle = CreateMaterial("DynamicObstacleRed", new Color(0.62f, 0.08f, 0.08f));
        wall = CreateMaterial("Wall", new Color(0.28f, 0.28f, 0.31f));
        tire = CreateMaterial("TireBlack", new Color(0.02f, 0.02f, 0.025f));
        window = CreateMaterial("WindowDark", new Color(0.05f, 0.09f, 0.12f));
    }

    private static Material CreateMaterial(string name, Color color)
    {
        string path = $"{MaterialsPath}/{name}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            shader = shader != null ? shader : Shader.Find("Standard");
            shader = shader != null ? shader : Shader.Find("Sprites/Default");
            material = new Material(shader);

            AssetDatabase.CreateAsset(material, path);
        }

        material.color = color;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void CreatePrefabs()
    {
        SavePrefab(CreateParkingCar(ParkingType.Parallel), $"{PrefabsPath}/ParkingCar.prefab");
        SavePrefab(CreateVehicleObject("ParkedCar", parkedCar), $"{PrefabsPath}/ParkedCar.prefab");
        SavePrefab(CreateMovingObstaclePrefab(), $"{PrefabsPath}/MovingObstacle.prefab");
    }

    private static void SavePrefab(GameObject instance, string path)
    {
        PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);
    }

    private static GameObject CreateParkingCar(ParkingType type)
    {
        GameObject root = new GameObject("ParkingCar");
        root.transform.position = Vector3.zero;

        AddVehicleVisual(root, egoCar, true);

        Rigidbody rb = root.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        ParkingCarController controller = root.AddComponent<ParkingCarController>();
        controller.maxForwardSpeed = 3.8f;
        controller.maxReverseSpeed = 2.5f;

        ParkingSensors sensors = root.AddComponent<ParkingSensors>();
        sensors.parkingType = type;
        sensors.parallelRequiredLength = 6.3f;
        sensors.perpendicularRequiredWidth = 2.7f;

        root.AddComponent<VehicleVisualRig>();
        root.AddComponent<ParkingManeuverPlanner>();
        root.AddComponent<ParkingStateMachine>();

        return root;
    }

    private static GameObject CreateMovingObstaclePrefab()
    {
        GameObject root = CreateVehicleObject("MovingObstacle", obstacle);
        root.AddComponent<MovingObstacle>();
        return root;
    }

    private static GameObject CreateVehicleObject(string name, Material bodyMaterial)
    {
        GameObject root = new GameObject(name);
        AddVehicleVisual(root, bodyMaterial, false);
        root.AddComponent<VehicleVisualRig>();
        return root;
    }

    private static void AddVehicleVisual(GameObject root, Material bodyMaterial, bool isEgo)
    {
        const float width = 2.0f;
        const float length = 4.45f;

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(root.transform);
        body.transform.localPosition = new Vector3(0.0f, 0.52f, 0.0f);
        body.transform.localScale = new Vector3(width, 0.72f, length);
        body.GetComponent<Renderer>().sharedMaterial = bodyMaterial;

        GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cabin.name = "Cabin";
        cabin.transform.SetParent(root.transform);
        cabin.transform.localPosition = new Vector3(0.0f, 1.02f, -0.25f);
        cabin.transform.localScale = new Vector3(width * 0.72f, 0.45f, length * 0.38f);
        cabin.GetComponent<Renderer>().sharedMaterial = window;
        Object.DestroyImmediate(cabin.GetComponent<BoxCollider>());

        GameObject front = GameObject.CreatePrimitive(PrimitiveType.Cube);
        front.name = isEgo ? "YellowFrontMarker" : "FrontLights";
        front.transform.SetParent(root.transform);
        front.transform.localPosition = new Vector3(0.0f, 0.66f, length * 0.52f);
        front.transform.localScale = new Vector3(width * 0.72f, 0.12f, 0.08f);
        front.GetComponent<Renderer>().sharedMaterial = isEgo ? line : window;
        Object.DestroyImmediate(front.GetComponent<BoxCollider>());

        AddWheel(root.transform, -width * 0.58f, -length * 0.34f);
        AddWheel(root.transform, width * 0.58f, -length * 0.34f);
        AddWheel(root.transform, -width * 0.58f, length * 0.34f);
        AddWheel(root.transform, width * 0.58f, length * 0.34f);
    }

    private static void AddWheel(Transform parent, float x, float z)
    {
        GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        wheel.name = "Wheel";
        wheel.transform.SetParent(parent);
        wheel.transform.localPosition = new Vector3(Mathf.Sign(x) * 1.03f, 0.26f, z);
        wheel.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
        wheel.transform.localScale = new Vector3(0.24f, 0.08f, 0.24f);
        wheel.GetComponent<Renderer>().sharedMaterial = tire;
        Object.DestroyImmediate(wheel.GetComponent<Collider>());
    }

    private static GameObject CreateBox(string name, Vector3 scale, Material material)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.localScale = scale;
        box.GetComponent<Renderer>().sharedMaterial = material;
        return box;
    }

    private static void CreateMainMenuScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "MainMenu";

        CreateLightAndCamera(new Vector3(0.0f, 8.0f, -10.0f), new Vector3(35.0f, 0.0f, 0.0f));
        GameObject manager = new GameObject("MapManager");
        manager.AddComponent<MapManager>();
        manager.AddComponent<ImmediateMapUi>();

        Canvas canvas = CreateCanvas();
        CreatePanel(canvas.transform, "Automatic Parking - Unity 3D", 28, new Vector2(0.0f, 145.0f), new Vector2(760.0f, 70.0f));
        CreateButton(canvas.transform, "Mapa 1: parking prostopadly", new Vector2(0.0f, 45.0f), 1);
        CreateButton(canvas.transform, "Mapa 2: parkowanie rownolegle", new Vector2(0.0f, -35.0f), 2);
        CreateButton(canvas.transform, "Mapa 3: przeszkoda dynamiczna", new Vector2(0.0f, -115.0f), 3);

        EditorSceneManager.SaveScene(scene, $"{ScenesPath}/MainMenu.unity");
    }

    private static void CreatePerpendicularScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Map_01_Perpendicular";

        CreateBaseWorld("Map 1 - Perpendicular parking");
        CreateRoad(new Vector3(0.0f, 0.0f, -6.0f), new Vector3(8.0f, 0.08f, 58.0f));
        CreateParkingLinesPerpendicular();

        CreatePerpendicularParkedCar(-23.0f);
        CreatePerpendicularParkedCar(-20.1f);
        CreateNarrowGapBoundaryCar(-17.2f, -4.0f);
        CreateNarrowGapBoundaryCar(-13.3f, 4.0f);
        CreatePerpendicularParkedCar(-5.0f);
        CreatePerpendicularParkedCar(-1.6f);
        CreatePerpendicularParkedCar(1.8f);

        GameObject car = CreateSceneCar(ParkingType.Perpendicular, new Vector3(0.0f, 0.0f, -27.0f), Quaternion.identity);
        car.GetComponent<ParkingStateMachine>().scanThrottle = 0.32f;
        ConfigurePerpendicularScenario(car);

        CreateHud(car, "Map 1: wszystkie miejsca zajete poza za waska luka i miejscem docelowym.");
        CreateFollowCamera(car.transform);
        EditorSceneManager.SaveScene(scene, $"{ScenesPath}/Map_01_Perpendicular.unity");
    }

    private static void CreateParallelScene(bool dynamicObstacle)
    {
        string sceneName = dynamicObstacle ? "Map_03_Dynamic" : "Map_02_Parallel";
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = sceneName;

        CreateBaseWorld(dynamicObstacle ? "Map 3 - Dynamic obstacle" : "Map 2 - Parallel parking");
        CreateRoad(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(8.0f, 0.08f, 54.0f));
        CreateWall(new Vector3(-4.2f, 1.0f, 0.0f), new Vector3(0.35f, 2.0f, 54.0f));
        CreateParallelParkedCar(-22.0f);
        CreateParallelParkedCar(-17.0f);
        CreateParallelParkedCar(-12.0f);
        CreateParallelParkedCar(-4.0f);
        CreateParallelParkedCar(11.2f);

        GameObject car = CreateSceneCar(ParkingType.Parallel, new Vector3(0.0f, 0.0f, -26.0f), Quaternion.identity);
        car.GetComponent<ParkingStateMachine>().scanThrottle = 0.33f;
        if (dynamicObstacle)
        {
            ConfigureDynamicParallelScenario(car);
        }
        else
        {
            ConfigureParallelScenario(car);
        }

        if (dynamicObstacle)
        {
            CreateDynamicObstacle(car.GetComponent<ParkingStateMachine>());
            CreateWall(new Vector3(0.0f, 1.4f, 15.5f), new Vector3(7.8f, 2.8f, 0.35f));
            CreatePillar(new Vector3(-2.7f, 0.8f, -6.0f));
            CreatePillar(new Vector3(-2.7f, 0.8f, 6.0f));
            CreatePillar(new Vector3(2.7f, 0.8f, -9.0f));
            CreatePillar(new Vector3(2.7f, 0.8f, 11.0f));
        }

        CreateHud(car, dynamicObstacle
            ? "Map 3: moving obstacle should trigger EmergencyStop."
            : "Map 2: short gap is ignored; second gap is accepted.");
        CreateFollowCamera(car.transform);
        EditorSceneManager.SaveScene(scene, $"{ScenesPath}/{sceneName}.unity");
    }

    private static void CreateDynamicLeftParkingScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Map_03_Dynamic";

        CreateBaseWorld("Map 3 - Oncoming traffic and left-side parking");
        CreateRoad(new Vector3(0.0f, 0.0f, -6.0f), new Vector3(8.0f, 0.08f, 58.0f));
        CreateParkingLinesLeftPerpendicular();

        CreateLeftPerpendicularParkedCar(-23.0f);
        CreateLeftPerpendicularParkedCar(-20.1f);
        CreateLeftPerpendicularParkedCar(-17.2f);
        CreateLeftPerpendicularParkedCar(-13.3f);
        CreateLeftPerpendicularParkedCar(-5.0f);
        CreateLeftPerpendicularParkedCar(-1.6f);
        CreateLeftPerpendicularParkedCar(1.8f);

        GameObject car = CreateSceneCar(ParkingType.Perpendicular, new Vector3(1.35f, 0.0f, -27.0f), Quaternion.identity);
        ConfigureDynamicLeftScenario(car);
        CreateOncomingTrafficCar(car.GetComponent<ParkingStateMachine>());

        CreateHud(car, "Map 3: auto z naprzeciwka; niebieskie czeka i parkuje po lewej stronie.");
        CreateFollowCamera(car.transform);
        EditorSceneManager.SaveScene(scene, $"{ScenesPath}/Map_03_Dynamic.unity");
    }

    private static void CreateBaseWorld(string title)
    {
        CreateLightAndCamera(new Vector3(-8.0f, 10.0f, -12.0f), new Vector3(45.0f, 35.0f, 0.0f));
        CreateGround();

        GameObject manager = new GameObject("MapManager");
        manager.AddComponent<MapManager>();
        manager.AddComponent<ImmediateMapUi>();

        Canvas canvas = CreateCanvas();
        CreatePanel(canvas.transform, title, 20, new Vector2(0.0f, -24.0f), new Vector2(620.0f, 44.0f), TextAnchor.MiddleCenter, Anchor.TopCenter);
        CreateSmallButton(canvas.transform, "Menu", new Vector2(-155.0f, -78.0f), 0);
        CreateSmallButton(canvas.transform, "Restart", new Vector2(0.0f, -78.0f), -1);
        CreateSmallButton(canvas.transform, "Map 3", new Vector2(155.0f, -78.0f), 3);
    }

    private static void CreateGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.position = new Vector3(0.0f, -0.08f, 0.0f);
        ground.transform.localScale = new Vector3(28.0f, 0.08f, 62.0f);
        ground.GetComponent<Renderer>().sharedMaterial = grass;
    }

    private static void CreateRoad(Vector3 position, Vector3 scale)
    {
        GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
        road.name = "Road";
        road.transform.position = position;
        road.transform.localScale = scale;
        road.GetComponent<Renderer>().sharedMaterial = asphalt;
    }

    private static void CreateWall(Vector3 position, Vector3 scale)
    {
        GameObject objectWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        objectWall.name = "LeftWall";
        objectWall.transform.position = position;
        objectWall.transform.localScale = scale;
        objectWall.GetComponent<Renderer>().sharedMaterial = wall;
    }

    private static void CreateParkingLinesPerpendicular()
    {
        for (int i = 0; i < 13; i++)
        {
            float z = -23.7f + i * 3.4f;
            GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = "ParkingLine";
            stripe.transform.position = new Vector3(5.1f, 0.04f, z);
            stripe.transform.localScale = new Vector3(4.8f, 0.04f, 0.08f);
            stripe.GetComponent<Renderer>().sharedMaterial = line;
            Object.DestroyImmediate(stripe.GetComponent<BoxCollider>());
        }

        GameObject narrowLineA = GameObject.CreatePrimitive(PrimitiveType.Cube);
        narrowLineA.name = "TooNarrowSlotLine";
        narrowLineA.transform.position = new Vector3(5.1f, 0.05f, -16.0f);
        narrowLineA.transform.localScale = new Vector3(4.8f, 0.04f, 0.06f);
        narrowLineA.GetComponent<Renderer>().sharedMaterial = line;
        Object.DestroyImmediate(narrowLineA.GetComponent<BoxCollider>());

        GameObject narrowLineB = GameObject.CreatePrimitive(PrimitiveType.Cube);
        narrowLineB.name = "TooNarrowSlotLine";
        narrowLineB.transform.position = new Vector3(5.1f, 0.05f, -14.55f);
        narrowLineB.transform.localScale = new Vector3(4.8f, 0.04f, 0.06f);
        narrowLineB.GetComponent<Renderer>().sharedMaterial = line;
        Object.DestroyImmediate(narrowLineB.GetComponent<BoxCollider>());
    }

    private static void CreateParkingLinesLeftPerpendicular()
    {
        for (int i = 0; i < 13; i++)
        {
            float z = -23.7f + i * 3.4f;
            GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = "LeftParkingLine";
            stripe.transform.position = new Vector3(-5.1f, 0.04f, z);
            stripe.transform.localScale = new Vector3(4.8f, 0.04f, 0.08f);
            stripe.GetComponent<Renderer>().sharedMaterial = line;
            Object.DestroyImmediate(stripe.GetComponent<BoxCollider>());
        }
    }

    private static void CreatePerpendicularParkedCar(float z)
    {
        GameObject car = CreateVehicleObject("ParkedCar_Perpendicular", parkedCar);
        car.transform.position = new Vector3(5.25f, 0.0f, z);
        car.transform.rotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
    }

    private static void CreateNarrowGapBoundaryCar(float z, float yaw)
    {
        GameObject car = CreateVehicleObject("ParkedCar_NarrowGapBoundary", parkedCar);
        car.transform.position = new Vector3(5.25f, 0.0f, z);
        car.transform.localScale = new Vector3(1.18f, 1.0f, 1.0f);
        car.transform.rotation = Quaternion.Euler(0.0f, 90.0f + yaw, 0.0f);
    }

    private static void CreateLeftPerpendicularParkedCar(float z)
    {
        GameObject car = CreateVehicleObject("ParkedCar_LeftPerpendicular", parkedCar);
        car.transform.position = new Vector3(-5.25f, 0.0f, z);
        car.transform.rotation = Quaternion.Euler(0.0f, -90.0f, 0.0f);
    }

    private static void CreateParallelParkedCar(float z)
    {
        GameObject car = CreateVehicleObject("ParkedCar_Parallel", parkedCar);
        car.transform.position = new Vector3(4.0f, 0.0f, z);
    }

    private static GameObject CreateSceneCar(ParkingType type, Vector3 position, Quaternion rotation)
    {
        GameObject car = CreateParkingCar(type);
        car.transform.position = position;
        car.transform.rotation = rotation;
        return car;
    }

    private static void ConfigurePerpendicularScenario(GameObject car)
    {
        ParkingScenario scenario = car.AddComponent<ParkingScenario>();
        scenario.parkingType = ParkingType.Perpendicular;
        scenario.scanAcceptPoint = CreateWaypoint("Map1_ScanAccept", new Vector3(0.0f, 0.0f, -10.4f), 0.0f);
        scenario.scanSpeed = 2.15f;
        scenario.spotStart = new Vector3(4.0f, 0.0f, -9.9f);
        scenario.spotEnd = new Vector3(6.4f, 0.0f, -6.9f);
        scenario.spotWidth = 3.0f;
        scenario.steps = new[]
        {
            Step(ParkingState.Positioning, CreateWaypoint("Map1_Positioning", new Vector3(0.0f, 0.0f, -9.8f), 0.0f), false, 1.45f, 0.12f, 1.4f),
            Step(ParkingState.ReverseTurn, CreateWaypoint("Map1_Rail_25", new Vector3(0.55f, 0.0f, -9.35f), 25.0f), false, 1.00f, 0.10f, 1.1f),
            Step(ParkingState.ReverseTurn, CreateWaypoint("Map1_Rail_45", new Vector3(1.45f, 0.0f, -8.85f), 45.0f), false, 0.92f, 0.10f, 1.1f),
            Step(ParkingState.CounterTurn, CreateWaypoint("Map1_Rail_65", new Vector3(2.75f, 0.0f, -8.55f), 65.0f), false, 0.82f, 0.10f, 1.0f),
            Step(ParkingState.CounterTurn, CreateWaypoint("Map1_Rail_82", new Vector3(4.10f, 0.0f, -8.43f), 82.0f), false, 0.72f, 0.10f, 0.9f),
            Step(ParkingState.Straighten, CreateWaypoint("Map1_Final", new Vector3(5.65f, 0.0f, -8.4f), 90.0f), false, 0.55f, 0.08f, 0.8f)
        };
    }

    private static void ConfigureParallelScenario(GameObject car)
    {
        ParkingScenario scenario = car.AddComponent<ParkingScenario>();
        scenario.parkingType = ParkingType.Parallel;
        scenario.scanAcceptPoint = CreateWaypoint("Map2_ScanAccept", new Vector3(0.0f, 0.0f, 1.7f), 0.0f);
        scenario.scanSpeed = 2.25f;
        scenario.spotStart = new Vector3(3.1f, 0.0f, -1.7f);
        scenario.spotEnd = new Vector3(3.1f, 0.0f, 8.8f);
        scenario.spotWidth = 2.6f;
        scenario.steps = new[]
        {
            Step(ParkingState.Positioning, CreateWaypoint("Map2_Positioning", new Vector3(0.0f, 0.0f, 5.9f), 0.0f), false, 1.20f, 0.12f, 2.2f),
            Step(ParkingState.ReverseTurn, CreateWaypoint("Map2_BackTurn_12", new Vector3(0.85f, 0.0f, 5.15f), -12.0f), true, 0.60f, 0.10f, 1.7f),
            Step(ParkingState.ReverseTurn, CreateWaypoint("Map2_BackTurn_26", new Vector3(1.85f, 0.0f, 4.35f), -26.0f), true, 0.56f, 0.10f, 1.7f),
            Step(ParkingState.CounterTurn, CreateWaypoint("Map2_CounterTurn_16", new Vector3(2.85f, 0.0f, 3.55f), -16.0f), true, 0.54f, 0.10f, 1.5f),
            Step(ParkingState.CounterTurn, CreateWaypoint("Map2_CounterTurn_4", new Vector3(3.58f, 0.0f, 2.90f), -4.0f), true, 0.50f, 0.10f, 1.2f),
            Step(ParkingState.Straighten, CreateWaypoint("Map2_Final", new Vector3(4.0f, 0.0f, 3.25f), 0.0f), false, 0.36f, 0.08f, 0.8f)
        };
    }

    private static void ConfigureDynamicParallelScenario(GameObject car)
    {
        ParkingScenario scenario = car.AddComponent<ParkingScenario>();
        scenario.parkingType = ParkingType.Parallel;
        scenario.scanAcceptPoint = CreateWaypoint("Map3_WaitBeforeOncoming", new Vector3(0.0f, 0.0f, 1.9f), 0.0f);
        scenario.scanSpeed = 2.05f;
        scenario.spotStart = new Vector3(3.1f, 0.0f, -1.7f);
        scenario.spotEnd = new Vector3(3.1f, 0.0f, 8.8f);
        scenario.spotWidth = 2.6f;
        scenario.steps = new[]
        {
            Step(ParkingState.Positioning, CreateWaypoint("Map3_LeftRoadEdgeAfterTraffic", new Vector3(-1.35f, 0.0f, 5.85f), 0.0f), false, 1.15f, 0.20f, 3.2f),
            Step(ParkingState.ReverseTurn, CreateWaypoint("Map3_BackIntoSpot", new Vector3(2.05f, 0.0f, 4.55f), -30.0f), true, 0.72f, 0.22f, 3.4f),
            Step(ParkingState.CounterTurn, CreateWaypoint("Map3_CounterIntoSpot", new Vector3(3.62f, 0.0f, 2.75f), 2.0f), true, 0.66f, 0.16f, 2.7f),
            Step(ParkingState.Straighten, CreateWaypoint("Map3_FinalParked", new Vector3(4.0f, 0.0f, 2.62f), 0.0f), false, 0.40f, 0.08f, 1.2f)
        };
    }

    private static void ConfigureDynamicLeftScenario(GameObject car)
    {
        ParkingScenario scenario = car.AddComponent<ParkingScenario>();
        scenario.parkingType = ParkingType.Perpendicular;
        scenario.scanAcceptPoint = CreateWaypoint("Map3_StopForOncoming", new Vector3(1.35f, 0.0f, -12.8f), 0.0f);
        scenario.scanSpeed = 2.05f;
        scenario.spotStart = new Vector3(-6.4f, 0.0f, -10.1f);
        scenario.spotEnd = new Vector3(-4.0f, 0.0f, -6.7f);
        scenario.spotWidth = 3.0f;
        scenario.steps = new[]
        {
            Step(ParkingState.Positioning, CreateWaypoint("Map3_MoveLeft_1", new Vector3(0.80f, 0.0f, -10.6f), -8.0f), false, 0.95f, 0.10f, 2.0f),
            Step(ParkingState.Positioning, CreateWaypoint("Map3_MoveLeft_2", new Vector3(-0.05f, 0.0f, -7.8f), -6.0f), false, 0.90f, 0.10f, 1.8f),
            Step(ParkingState.Positioning, CreateWaypoint("Map3_PassTargetSlot", new Vector3(-0.45f, 0.0f, -4.1f), 0.0f), false, 0.82f, 0.10f, 1.5f),
            Step(ParkingState.ReverseTurn, CreateWaypoint("Map3_ReverseRail_04", new Vector3(-0.65f, 0.0f, -5.5f), 4.0f), true, 0.46f, 0.08f, 1.1f),
            Step(ParkingState.ReverseTurn, CreateWaypoint("Map3_ReverseRail_10", new Vector3(-1.00f, 0.0f, -6.6f), 10.0f), true, 0.46f, 0.08f, 1.1f),
            Step(ParkingState.CounterTurn, CreateWaypoint("Map3_ReverseRail_28", new Vector3(-1.80f, 0.0f, -7.4f), 28.0f), true, 0.44f, 0.08f, 1.0f),
            Step(ParkingState.CounterTurn, CreateWaypoint("Map3_ReverseRail_50", new Vector3(-3.00f, 0.0f, -8.05f), 50.0f), true, 0.42f, 0.08f, 0.9f),
            Step(ParkingState.Straighten, CreateWaypoint("Map3_ReverseRail_75", new Vector3(-4.45f, 0.0f, -8.35f), 75.0f), true, 0.38f, 0.08f, 0.8f),
            Step(ParkingState.Straighten, CreateWaypoint("Map3_FinalLeftSpot", new Vector3(-5.62f, 0.0f, -8.4f), 90.0f), true, 0.32f, 0.06f, 0.6f)
        };
    }

    private static Transform CreateWaypoint(string name, Vector3 position, float yaw)
    {
        GameObject waypoint = new GameObject(name);
        waypoint.transform.position = position;
        waypoint.transform.rotation = Quaternion.Euler(0.0f, yaw, 0.0f);
        return waypoint.transform;
    }

    private static ParkingScenarioStep Step(ParkingState state, Transform target, bool reverse, float speed, float arriveDistance, float curveHandle = 2.2f)
    {
        return new ParkingScenarioStep
        {
            state = state,
            target = target,
            reverse = reverse,
            speed = speed,
            turnSpeed = 120.0f,
            arriveDistance = arriveDistance,
            angleTolerance = 3.5f,
            timeout = 15.0f,
            curveHandle = curveHandle
        };
    }

    private static void CreatePillar(Vector3 position)
    {
        GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pillar.name = "ConcretePillar";
        pillar.transform.position = position;
        pillar.transform.localScale = new Vector3(0.75f, 0.8f, 0.75f);
        pillar.GetComponent<Renderer>().sharedMaterial = wall;
    }

    private static void CreateDynamicObstacle(ParkingStateMachine observedCar)
    {
        GameObject pointA = new GameObject("DynamicPointA");
        pointA.transform.position = new Vector3(-2.35f, 0.0f, 12.5f);
        GameObject pointB = new GameObject("DynamicPointB");
        pointB.transform.position = new Vector3(-2.35f, 0.0f, -14.5f);

        GameObject moving = CreateVehicleObject("DynamicCarBlockingLane", obstacle);
        moving.transform.position = pointA.transform.position;
        moving.transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
        MovingObstacle script = moving.AddComponent<MovingObstacle>();
        script.pointA = pointA.transform;
        script.pointB = pointB.transform;
        script.speed = 2.45f;
        script.waitForParkingManeuver = true;
        script.observedCar = observedCar;
        script.triggerState = ParkingState.Positioning;
        script.triggerDelay = 0.05f;
        script.blockObservedCarUntilTarget = true;
        script.emergencyDistance = 5.0f;
    }

    private static void CreateOncomingTrafficCar(ParkingStateMachine observedCar)
    {
        GameObject pointA = new GameObject("OncomingTrafficStart");
        pointA.transform.position = new Vector3(-1.35f, 0.0f, 6.5f);
        GameObject pointB = new GameObject("OncomingTrafficEnd");
        pointB.transform.position = new Vector3(-1.35f, 0.0f, -27.5f);

        GameObject moving = CreateVehicleObject("RedOncomingCar", obstacle);
        moving.transform.position = pointA.transform.position;
        moving.transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
        MovingObstacle script = moving.AddComponent<MovingObstacle>();
        script.pointA = pointA.transform;
        script.pointB = pointB.transform;
        script.speed = 2.8f;
        script.waitForParkingManeuver = true;
        script.observedCar = observedCar;
        script.triggerState = ParkingState.Scan;
        script.triggerDelay = 0.2f;
        script.blockObservedCarUntilTarget = true;
        script.emergencyDistance = 5.5f;
        script.stopAfterPointB = true;
    }

    private static void CreateLightAndCamera(Vector3 cameraPosition, Vector3 cameraRotation)
    {
        GameObject lightObject = new GameObject("Directional Light");
        Light sun = lightObject.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.intensity = 2.2f;
        lightObject.transform.rotation = Quaternion.Euler(50.0f, -30.0f, 0.0f);

        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.fieldOfView = 55.0f;
        cameraObject.transform.position = cameraPosition;
        cameraObject.transform.rotation = Quaternion.Euler(cameraRotation);
    }

    private static void CreateFollowCamera(Transform target)
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        CameraFollow follow = camera.gameObject.AddComponent<CameraFollow>();
        follow.target = target;
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280.0f, 720.0f);
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<InputSystemUIInputModule>();
        return canvas;
    }

    private static void CreateHud(GameObject car, string title)
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        Text header = CreateText(canvas.transform, title, 16, new Vector2(18.0f, -18.0f), new Vector2(520.0f, 36.0f), TextAnchor.UpperLeft, Anchor.TopLeft);
        header.color = Color.white;

        Text debug = CreateText(canvas.transform, "", 14, new Vector2(18.0f, -62.0f), new Vector2(520.0f, 168.0f), TextAnchor.UpperLeft, Anchor.TopLeft);
        debug.color = Color.white;

        DebugHud hud = canvas.gameObject.AddComponent<DebugHud>();
        hud.stateMachine = car.GetComponent<ParkingStateMachine>();
        hud.car = car.GetComponent<ParkingCarController>();
        hud.sensors = car.GetComponent<ParkingSensors>();
        hud.label = debug;
    }

    private static void CreateButton(Transform parent, string label, Vector2 anchoredPosition, int mapIndex)
    {
        GameObject buttonObject = CreateButtonObject(parent, label, anchoredPosition, new Vector2(430.0f, 54.0f), 18);
        Button button = buttonObject.GetComponent<Button>();
        MapManager manager = Object.FindObjectOfType<MapManager>();
        UnityEventTools.AddIntPersistentListener(button.onClick, manager.LoadMap, mapIndex);
        EditorUtility.SetDirty(button);
    }

    private static void CreateSmallButton(Transform parent, string label, Vector2 anchoredPosition, int mapIndex)
    {
        GameObject buttonObject = CreateButtonObject(parent, label, anchoredPosition, new Vector2(130.0f, 38.0f), 15, Anchor.TopCenter);
        Button button = buttonObject.GetComponent<Button>();
        MapManager manager = Object.FindObjectOfType<MapManager>();
        if (mapIndex < 0)
        {
            UnityEventTools.AddPersistentListener(button.onClick, manager.RestartCurrent);
        }
        else
        {
            UnityEventTools.AddIntPersistentListener(button.onClick, manager.LoadMap, mapIndex);
        }

        EditorUtility.SetDirty(button);
    }

    private static GameObject CreateButtonObject(Transform parent, string label, Vector2 anchoredPosition, Vector2 size, int fontSize, Anchor anchor = Anchor.MiddleCenter)
    {
        GameObject buttonObject = new GameObject(label);
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        ApplyAnchor(rect, anchor);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.13f, 0.25f, 0.38f, 0.95f);
        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.18f, 0.38f, 0.58f, 1.0f);
        button.colors = colors;

        Text text = CreateText(buttonObject.transform, label, fontSize, Vector2.zero, size, TextAnchor.MiddleCenter, Anchor.Stretch);
        text.color = Color.white;
        return buttonObject;
    }

    private static void CreatePanel(Transform parent, string text, int fontSize, Vector2 anchoredPosition, Vector2 size, TextAnchor alignment = TextAnchor.MiddleCenter, Anchor anchor = Anchor.MiddleCenter)
    {
        Text label = CreateText(parent, text, fontSize, anchoredPosition, size, alignment, anchor);
        label.color = Color.white;
    }

    private static Text CreateText(Transform parent, string value, int fontSize, Vector2 anchoredPosition, Vector2 size, TextAnchor alignment, Anchor anchor)
    {
        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.AddComponent<RectTransform>();
        ApplyAnchor(rect, anchor);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Text text = textObject.AddComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
        {
            text.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        }
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static void ApplyAnchor(RectTransform rect, Anchor anchor)
    {
        switch (anchor)
        {
            case Anchor.TopLeft:
                rect.anchorMin = rect.anchorMax = new Vector2(0.0f, 1.0f);
                rect.pivot = new Vector2(0.0f, 1.0f);
                break;
            case Anchor.TopCenter:
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1.0f);
                rect.pivot = new Vector2(0.5f, 1.0f);
                break;
            case Anchor.Stretch:
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                break;
            default:
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                break;
        }
    }

    private static void UpdateBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene($"{ScenesPath}/MainMenu.unity", true),
            new EditorBuildSettingsScene($"{ScenesPath}/Map_01_Perpendicular.unity", true),
            new EditorBuildSettingsScene($"{ScenesPath}/Map_02_Parallel.unity", true),
            new EditorBuildSettingsScene($"{ScenesPath}/Map_03_Dynamic.unity", true)
        };
    }

    private enum Anchor
    {
        MiddleCenter,
        TopLeft,
        TopCenter,
        Stretch
    }
}
