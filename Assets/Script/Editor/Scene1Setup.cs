using UnityEngine;
using UnityEditor;
using TMPro;

public class Scene1Setup
{
    [MenuItem("Tools/Setup Scene 1 EP1")]
    public static void SetupScene1()
    {
        Debug.Log("=== Scene 1 EP1 Setup Starting ===");

        // Setup Dialogue Panel
        SetupDialoguePanel();

        // Setup Waypoints
        SetupWaypoints();

        // Setup NPCs (Aoyama, Yakuza)
        SetupNPCs();

        // Setup Triggers
        SetupTriggers();

        // Setup Ryu Follow
        SetupRyuFollow();

        Debug.Log("=== Scene 1 EP1 Setup Complete ===");
    }

    static void SetupDialoguePanel()
    {
        Debug.Log("Setting up Dialogue Panel...");

        // Cek apakah sudah ada
        GameObject dialogueManager = GameObject.Find("DialogueManager");
        if (dialogueManager == null)
        {
            dialogueManager = new GameObject("DialogueManager");
        }

        DialogueManagerEP1 dm = dialogueManager.GetComponent<DialogueManagerEP1>();
        if (dm == null)
        {
            dm = dialogueManager.AddComponent<DialogueManagerEP1>();
        }

        // Cek apakah canvas sudah ada
        GameObject dialogueCanvas = GameObject.Find("DialogueCanvas");
        if (dialogueCanvas == null)
        {
            // Buat Canvas
            dialogueCanvas = new GameObject("DialogueCanvas");
            Canvas canvas = dialogueCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            UnityEngine.UI.CanvasScaler scaler = dialogueCanvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            dialogueCanvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            Debug.Log("Created DialogueCanvas");
        }

        // Buat Dialogue Panel
        GameObject dialoguePanel = GameObject.Find("DialoguePanel");
        if (dialoguePanel == null)
        {
            dialoguePanel = new GameObject("DialoguePanel");
            dialoguePanel.transform.SetParent(dialogueCanvas.transform);

            RectTransform rt = dialoguePanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0);
            rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, 50);
            rt.sizeDelta = new Vector2(800, 200);

            UnityEngine.UI.Image img = dialoguePanel.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0, 0, 0, 0.8f);

            Debug.Log("Created DialoguePanel");
        }

        // Buat Name Text
        GameObject nameText = GameObject.Find("NameText");
        if (nameText == null)
        {
            nameText = new GameObject("NameText");
            nameText.transform.SetParent(dialoguePanel.transform);

            RectTransform rt = nameText.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(20, -20);
            rt.sizeDelta = new Vector2(200, 40);

            nameText.AddComponent<TextMeshProUGUI>().text = "";
            TextMeshProUGUI tmp = nameText.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = 28;
            tmp.fontStyle = TMPro.FontStyles.Bold;
            tmp.color = UnityEngine.Color.white;
            tmp.alignment = TextAlignmentOptions.Left;

            Debug.Log("Created NameText");
        }

        // Buat Dialogue Text
        GameObject dialogueText = GameObject.Find("DialogueText");
        if (dialogueText == null)
        {
            dialogueText = new GameObject("DialogueText");
            dialogueText.transform.SetParent(dialoguePanel.transform);

            RectTransform rt = dialogueText.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, 0);
            rt.sizeDelta = new Vector2(-40, -80);
            rt.offsetMin = new Vector2(20, 20);
            rt.offsetMax = new Vector2(-20, -60);

            dialogueText.AddComponent<TextMeshProUGUI>().text = "";
            TextMeshProUGUI tmp = dialogueText.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = 24;
            tmp.color = UnityEngine.Color.white;
            tmp.alignment = TextAlignmentOptions.TopLeft;

            Debug.Log("Created DialogueText");
        }

        // Buat Continue Indicator
        GameObject continueIndicator = GameObject.Find("ContinueIndicator");
        if (continueIndicator == null)
        {
            continueIndicator = new GameObject("ContinueIndicator");
            continueIndicator.transform.SetParent(dialoguePanel.transform);

            RectTransform rt = continueIndicator.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(1, 0);
            rt.anchoredPosition = new Vector2(-20, 20);
            rt.sizeDelta = new Vector2(150, 30);

            continueIndicator.AddComponent<TextMeshProUGUI>().text = "v";
            TextMeshProUGUI tmp = continueIndicator.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = 18;
            tmp.color = UnityEngine.Color.yellow;
            tmp.alignment = TextAlignmentOptions.Right;

            Debug.Log("Created ContinueIndicator");
        }

        // Assign ke DialogueManager
        dm.dialoguePanel = dialoguePanel;
        dm.nameText = nameText.GetComponent<TextMeshProUGUI>();
        dm.dialogueText = dialogueText.GetComponent<TextMeshProUGUI>();
        dm.continueIndicator = continueIndicator;
        dm.textSpeed = 0.03f;
        dm.autoAdvanceDelay = 2f;

        // Set panel inactive initially
        dialoguePanel.SetActive(false);
        continueIndicator.SetActive(false);

        Debug.Log("DialoguePanel setup complete!");
    }

    static void SetupWaypoints()
    {
        Debug.Log("Setting up Waypoints...");

        // Buat parent untuk Waypoints
        GameObject waypointsParent = GameObject.Find("Waypoints");
        if (waypointsParent == null)
        {
            waypointsParent = new GameObject("Waypoints");
        }

        // =====================
        // MICHELLE WAYPOINTS (per-momen)
        // =====================
        // Momen 1: Start di x=0, jalan ke x=5
        CreateWaypoint("WP_M1_Start", new Vector2(0, 0), waypointsParent.transform);
        CreateWaypoint("WP_M1_End", new Vector2(5, 0), waypointsParent.transform);

        // Momen 2: Start di x=5, jalan ke x=15
        CreateWaypoint("WP_M2_Start", new Vector2(5, 0), waypointsParent.transform);
        CreateWaypoint("WP_M2_End", new Vector2(15, 0), waypointsParent.transform);

        // Momen 3: Start di x=15, jalan ke x=25
        CreateWaypoint("WP_M3_Start", new Vector2(15, 0), waypointsParent.transform);
        CreateWaypoint("WP_M3_End", new Vector2(25, 0), waypointsParent.transform);

        // Momen 4: Start di x=25, jalan ke x=35
        CreateWaypoint("WP_M4_Start", new Vector2(25, 0), waypointsParent.transform);
        CreateWaypoint("WP_M4_End", new Vector2(35, 0), waypointsParent.transform);

        // Momen 5: Start di x=35, jalan ke x=40 (Ryu pisah)
        CreateWaypoint("WP_M5_Start", new Vector2(35, 0), waypointsParent.transform);
        CreateWaypoint("WP_M5_End", new Vector2(40, 0), waypointsParent.transform);

        // =====================
        // AOYAMA & YAKUZA WAYPOINTS (Momen 3)
        // =====================
        CreateWaypoint("Waypoint_Aoyama_1", new Vector2(40, 0), waypointsParent.transform);
        CreateWaypoint("Waypoint_Aoyama_2", new Vector2(-10, 0), waypointsParent.transform);

        CreateWaypoint("Waypoint_Yakuza_1", new Vector2(38, 0), waypointsParent.transform);
        CreateWaypoint("Waypoint_Yakuza_2", new Vector2(-12, 0), waypointsParent.transform);

        // =====================
        // RYU SEPARATION WAYPOINTS (Momen 5)
        // =====================
        CreateWaypoint("Waypoint_Ryu_1", new Vector2(36, 0), waypointsParent.transform);
        CreateWaypoint("Waypoint_Ryu_2", new Vector2(50, 0), waypointsParent.transform);

        Debug.Log("All waypoints created!");
    }

    static void CreateWaypoint(string name, Vector2 position, Transform parent)
    {
        GameObject wp = GameObject.Find(name);
        if (wp == null)
        {
            wp = new GameObject(name);
            wp.transform.SetParent(parent);
            wp.transform.position = position;
            Debug.Log($"Created {name} at {position}");
        }
        else
        {
            wp.transform.position = position;
            Debug.Log($"Updated {name} position to {position}");
        }
    }

    static void SetupNPCs()
    {
        Debug.Log("Setting up NPCs...");

        // Aoyama - buat kalau belum ada
        GameObject aoyama = GameObject.Find("Aoyama");
        if (aoyama == null)
        {
            aoyama = CreateNPC("Aoyama", new Vector2(40, 0));
        }
        aoyama.SetActive(false); // Mulai hidden

        // Yakuza - buat kalau belum ada
        GameObject yakuza = GameObject.Find("Yakuza");
        if (yakuza == null)
        {
            yakuza = CreateNPC("Yakuza", new Vector2(38, 0));
        }
        yakuza.SetActive(false); // Mulai hidden

        // Setup Aoyama AutoWalk
        SetupAutoWalkForNPC(aoyama,
            new string[] { "Waypoint_Aoyama_1", "Waypoint_Aoyama_2" },
            2f);

        // Setup Yakuza AutoWalk
        SetupAutoWalkForNPC(yakuza,
            new string[] { "Waypoint_Yakuza_1", "Waypoint_Yakuza_2" },
            1.8f);

        Debug.Log("NPCs setup complete!");
    }

    static GameObject CreateNPC(string name, Vector2 position)
    {
        GameObject npc = new GameObject(name);
        npc.transform.position = position;

        // Add basic components
        SpriteRenderer sr = npc.AddComponent<SpriteRenderer>();
        sr.color = Color.gray; // Placeholder color

        Rigidbody2D rb = npc.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        BoxCollider2D col = npc.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1, 2);
        col.isTrigger = true;

        return npc;
    }

    static void SetupAutoWalkForNPC(GameObject npc, string[] waypointNames, float speed)
    {
        AutoWalk aw = npc.GetComponent<AutoWalk>();
        if (aw == null)
            aw = npc.AddComponent<AutoWalk>();

        // Set waypoints
        Transform[] waypoints = new Transform[waypointNames.Length];
        for (int i = 0; i < waypointNames.Length; i++)
        {
            GameObject wp = GameObject.Find(waypointNames[i]);
            if (wp != null)
                waypoints[i] = wp.transform;
        }
        aw.waypoints = waypoints;
        aw.walkSpeed = speed;
        aw.flipSprite = true;
        aw.autoStart = false;
        aw.disablePlayerControl = false;

        Debug.Log($"Setup AutoWalk for {npc.name}");
    }

    static void SetupRyuFollow()
    {
        Debug.Log("Setting up Ryu Follow...");

        GameObject michelle = GameObject.Find("Michelle");
        GameObject ryu = GameObject.Find("Ryu");

        if (michelle == null || ryu == null)
        {
            Debug.LogError("Michelle or Ryu not found!");
            return;
        }

        // Setup Ryu FollowTarget
        FollowTarget ft = ryu.GetComponent<FollowTarget>();
        if (ft == null)
            ft = ryu.AddComponent<FollowTarget>();

        ft.target = michelle.transform;

        Debug.Log("Ryu FollowTarget setup complete!");

        // Setup Ryu AutoWalk untuk separation
        AutoWalk ryuAw = ryu.GetComponent<AutoWalk>();
        if (ryuAw == null)
            ryuAw = ryu.AddComponent<AutoWalk>();

        Transform[] ryuWaypoints = new Transform[2];
        ryuWaypoints[0] = GameObject.Find("Waypoint_Ryu_1")?.transform;
        ryuWaypoints[1] = GameObject.Find("Waypoint_Ryu_2")?.transform;
        ryuAw.waypoints = ryuWaypoints;
        ryuAw.walkSpeed = 1.5f;
        ryuAw.flipSprite = true;
        ryuAw.autoStart = false;
        ryuAw.disablePlayerControl = false;

        Debug.Log("Ryu AutoWalk setup complete!");
    }

    static void SetupTriggers()
    {
        Debug.Log("Setting up Triggers...");

        GameObject michelle = GameObject.Find("Michelle");
        GameObject ryu = GameObject.Find("Ryu");
        GameObject aoyama = GameObject.Find("Aoyama");
        GameObject yakuza = GameObject.Find("Yakuza");

        // Load dialogue data
        DialogueDataEP1 momen1 = LoadDialogue("Assets/Script/Story/EP1 Dialogue/Scene 1/Dialogue_Scene1_Momen1.asset");
        DialogueDataEP1 momen2 = LoadDialogue("Assets/Script/Story/EP1 Dialogue/Scene 1/Dialogue_Scene1_Momen2.asset");
        DialogueDataEP1 momen3 = LoadDialogue("Assets/Script/Story/EP1 Dialogue/Scene 1/Dialogue_Scene1_Momen3.asset");
        DialogueDataEP1 momen4 = LoadDialogue("Assets/Script/Story/EP1 Dialogue/Scene 1/Dialogue_Scene1_Momen4.asset");

        // =====================
        // TRIGGER_SCENE1_MOMEN1
        // =====================
        CreateTriggerWithWaypoints("Trigger_Scene1_Momen1",
            new Vector2(5, 0),
            momen1,
            michelle,
            new string[] { "WP_M1_Start", "WP_M1_End" });

        // =====================
        // TRIGGER_SCENE1_MOMEN2
        // =====================
        CreateTriggerWithWaypoints("Trigger_Scene1_Momen2",
            new Vector2(15, 0),
            momen2,
            michelle,
            new string[] { "WP_M2_Start", "WP_M2_End" });

        // =====================
        // TRIGGER_SCENE1_MOMEN3 (Aoyama & Yakuza walk by)
        // =====================
        CreateTriggerWithNPCsAndWaypoints("Trigger_Scene1_Momen3",
            new Vector2(25, 0),
            momen3,
            michelle,
            new string[] { "WP_M3_Start", "WP_M3_End" },
            new GameObject[] { aoyama, yakuza },
            new GameObject[] { });

        // =====================
        // TRIGGER_SCENE1_MOMEN4 (Ryu Separation)
        // =====================
        CreateRyuSeparationTriggerWithWaypoints("Trigger_Scene1_Momen4",
            new Vector2(35, 0),
            momen4,
            michelle, ryu,
            new string[] { "WP_M4_Start", "WP_M4_End" });

        Debug.Log("All triggers setup complete!");
    }

    static DialogueDataEP1 LoadDialogue(string path)
    {
        UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject));
        return obj as DialogueDataEP1;
    }

    static void CreateTriggerWithWaypoints(string name, Vector2 position, DialogueDataEP1 dialogue,
        GameObject autoWalkTarget, string[] waypointNames)
    {
        if (GameObject.Find(name) != null)
        {
            Debug.Log($"{name} already exists, skipping.");
            return;
        }

        GameObject triggerObj = new GameObject(name);
        triggerObj.transform.position = position;

        // BoxCollider2D
        BoxCollider2D col = triggerObj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(2, 3);
        col.isTrigger = true;

        // DialogueTriggerEP1
        DialogueTriggerEP1 trigger = triggerObj.AddComponent<DialogueTriggerEP1>();
        trigger.dialogueData = dialogue;
        trigger.triggerOnce = true;
        trigger.autoWalkTarget = autoWalkTarget?.GetComponent<AutoWalk>();

        // Set waypoints override
        Transform[] waypoints = new Transform[waypointNames.Length];
        for (int i = 0; i < waypointNames.Length; i++)
        {
            GameObject wp = GameObject.Find(waypointNames[i]);
            if (wp != null)
                waypoints[i] = wp.transform;
        }
        trigger.waypointsOverride = waypoints;

        Debug.Log($"Created {name} with waypoints at {position}");
    }

    static void CreateTriggerWithNPCsAndWaypoints(string name, Vector2 position, DialogueDataEP1 dialogue,
        GameObject autoWalkTarget, string[] waypointNames,
        GameObject[] npcsToShow, GameObject[] npcsToHide)
    {
        if (GameObject.Find(name) != null)
        {
            Debug.Log($"{name} already exists, skipping.");
            return;
        }

        GameObject triggerObj = new GameObject(name);
        triggerObj.transform.position = position;

        // BoxCollider2D
        BoxCollider2D col = triggerObj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(2, 3);
        col.isTrigger = true;

        // DialogueTriggerEP1
        DialogueTriggerEP1 trigger = triggerObj.AddComponent<DialogueTriggerEP1>();
        trigger.dialogueData = dialogue;
        trigger.triggerOnce = true;
        trigger.autoWalkTarget = autoWalkTarget?.GetComponent<AutoWalk>();
        trigger.npcsToShow = npcsToShow;
        trigger.npcsToHide = npcsToHide;

        // Set waypoints override
        Transform[] waypoints = new Transform[waypointNames.Length];
        for (int i = 0; i < waypointNames.Length; i++)
        {
            GameObject wp = GameObject.Find(waypointNames[i]);
            if (wp != null)
                waypoints[i] = wp.transform;
        }
        trigger.waypointsOverride = waypoints;

        Debug.Log($"Created {name} with NPCs and waypoints at {position}");
    }

    static void CreateRyuSeparationTriggerWithWaypoints(string name, Vector2 position, DialogueDataEP1 dialogue,
        GameObject michelle, GameObject ryu, string[] waypointNames)
    {
        if (GameObject.Find(name) != null)
        {
            Debug.Log($"{name} already exists, skipping.");
            return;
        }

        GameObject triggerObj = new GameObject(name);
        triggerObj.transform.position = position;

        // BoxCollider2D
        BoxCollider2D col = triggerObj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(2, 3);
        col.isTrigger = true;

        // DialogueTriggerEP1
        DialogueTriggerEP1 trigger = triggerObj.AddComponent<DialogueTriggerEP1>();
        trigger.dialogueData = dialogue;
        trigger.triggerOnce = true;
        trigger.autoWalkTarget = michelle?.GetComponent<AutoWalk>();

        // Set waypoints override
        Transform[] waypoints = new Transform[waypointNames.Length];
        for (int i = 0; i < waypointNames.Length; i++)
        {
            GameObject wp = GameObject.Find(waypointNames[i]);
            if (wp != null)
                waypoints[i] = wp.transform;
        }
        trigger.waypointsOverride = waypoints;

        // Ryu Separation
        trigger.ryuFollowTarget = ryu?.GetComponent<FollowTarget>();
        trigger.ryuAutoWalk = ryu?.GetComponent<AutoWalk>();
        trigger.startRyuAutoWalkOnDialogueEnd = true;
        trigger.hideRyuAfterAutoWalk = true;

        Debug.Log($"Created {name} with Ryu Separation at {position}");
    }

    [MenuItem("Tools/Full Reset Scene 1")]
    public static void FullReset()
    {
        if (!EditorUtility.DisplayDialog("Full Reset",
            "This will delete ALL Scene 1 objects and recreate. Continue?",
            "Yes - Delete All", "Cancel"))
        {
            return;
        }

        // Delete all scene 1 objects
        string[] objectsToDelete = {
            "Trigger_Scene1_Momen1", "Trigger_Scene1_Momen2",
            "Trigger_Scene1_Momen3", "Trigger_Scene1_Momen4",
            "Aoyama", "Yakuza", "Waypoints"
        };

        foreach (string objName in objectsToDelete)
        {
            GameObject obj = GameObject.Find(objName);
            if (obj != null)
            {
                Undo.DestroyObjectImmediate(obj);
                Debug.Log($"Deleted {objName}");
            }
        }

        // Re-setup
        SetupScene1();
    }
}
