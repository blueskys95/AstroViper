using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Tools > Monster AI > Build Patrol Demo Scene
// 블로그 글의 순찰/경계/공격 데모 씬을 자동으로 만든다
public static class MonsterPatrolSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/MonsterPatrolAI.unity";
    private const string MaterialFolder = "Assets/Scripts/MonsterAI/Materials";
    private const string GroundLayerName = "Ground";

    private const string BuildRequestFile = "Temp/BuildMonsterPatrolScene.request";

    // Temp/에 요청 파일이 있으면 스크립트 리로드 후 한 번만 자동으로 씬을 만든다
    [InitializeOnLoadMethod]
    private static void BuildOnRequest()
    {
        if (!System.IO.File.Exists(BuildRequestFile)) return;

        System.IO.File.Delete(BuildRequestFile);
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            Build();
        };
    }

    [MenuItem("Tools/Monster AI/Build Patrol Demo Scene")]
    public static void Build()
    {
        // 열려 있는 씬에 저장 안 된 변경이 있으면 먼저 저장할지 묻는다
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        int groundLayer = EnsureLayer(GroundLayerName);
        int groundMask = 1 << groundLayer;

        Material groundMat = CreateMaterial("Ground", new Color(0.35f, 0.33f, 0.38f));
        Material playerMat = CreateMaterial("Player", new Color(0.25f, 0.5f, 0.95f));
        Material monsterMat = CreateMaterial("Monster", new Color(0.3f, 0.75f, 0.35f));
        Material eyeMat = CreateMaterial("Eye", new Color(0.1f, 0.1f, 0.1f));

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // 플랫폼: 시작 지점 | 틈 | 순찰 플랫폼 | 틈 | 작은 플랫폼
        CreatePlatform("StartPlatform", -9f, 10f, 0f, groundLayer, groundMat);
        CreatePlatform("PatrolPlatform", 3f, 10f, 0f, groundLayer, groundMat);
        CreatePlatform("SmallPlatform", 14f, 5f, 0f, groundLayer, groundMat);
        CreateBlock("LeftWall", new Vector3(-14.5f, 2f, 0f), new Vector3(1f, 5f, 3f), groundLayer, groundMat);

        // 플레이어
        GameObject player = CreateCharacter("Player", new Vector3(-8f, 1f, 0f), Vector3.one, playerMat, eyeMat, 1f);
        var playerMove = player.AddComponent<PlayerMove>();
        SetFields(playerMove, so => so.FindProperty("groundLayer").intValue = groundMask);
        player.AddComponent<FallRespawn>();

        // 몬스터 2마리: 순찰 플랫폼(메인), 작은 플랫폼(낭떠러지 뒤돌기 확인용)
        CreateMonster("Monster", new Vector3(3f, 0.8f, 0f), player.transform, groundMask, monsterMat, eyeMat);
        CreateMonster("Monster_Small", new Vector3(14f, 0.8f, 0f), player.transform, groundMask, monsterMat, eyeMat);

        // 카메라: 횡스크롤 시점으로 플레이어를 따라간다
        Camera cam = Camera.main;
        cam.transform.SetPositionAndRotation(player.transform.position + new Vector3(0f, 2f, -12f), Quaternion.Euler(5f, 0f, 0f));
        cam.fieldOfView = 50f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.14f, 0.2f);
        var follow = cam.gameObject.AddComponent<SideCameraFollow>();
        SetFields(follow, so => so.FindProperty("target").objectReferenceValue = player.transform);

        var light = Object.FindObjectOfType<Light>();
        if (light != null)
            light.transform.rotation = Quaternion.Euler(40f, -30f, 0f);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log($"[MonsterPatrolSceneBuilder] Saved {ScenePath}");
    }

    private static void CreateMonster(string name, Vector3 position, Transform player, int groundMask, Material bodyMat, Material eyeMat)
    {
        GameObject monster = CreateCharacter(name, position, Vector3.one * 0.8f, bodyMat, eyeMat, 5f);

        var controller = monster.AddComponent<MonsterController>();
        SetFields(controller, so =>
        {
            so.FindProperty("player").objectReferenceValue = player;
            so.FindProperty("groundLayer").intValue = groundMask;
        });

        // 상태 라벨 (몬스터가 뒤돌아도 글자가 뒤집히지 않도록 자식이 아닌 별도 오브젝트)
        var labelGo = new GameObject(name + "_StateLabel");
        var text = labelGo.AddComponent<TextMesh>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.GetComponent<MeshRenderer>().sharedMaterial = text.font.material;
        text.fontSize = 48;
        text.characterSize = 0.06f;
        text.anchor = TextAnchor.LowerCenter;
        text.alignment = TextAlignment.Center;
        text.text = "Patrol";
        labelGo.transform.position = position + new Vector3(0f, 1.4f, 0f);

        var indicator = monster.AddComponent<MonsterStateIndicator>();
        SetFields(indicator, so =>
        {
            so.FindProperty("monster").objectReferenceValue = controller;
            so.FindProperty("body").objectReferenceValue = monster.GetComponent<Renderer>();
            so.FindProperty("label").objectReferenceValue = text;
        });
    }

    private static GameObject CreateCharacter(string name, Vector3 position, Vector3 scale, Material bodyMat, Material eyeMat, float mass)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = name;
        go.transform.position = position;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = bodyMat;

        var rigid = go.AddComponent<Rigidbody>();
        rigid.mass = mass;
        rigid.interpolation = RigidbodyInterpolation.Interpolate;
        rigid.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rigid.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;

        // 바라보는 방향을 알 수 있도록 눈(앞쪽 +x)을 붙인다
        GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Cube);
        eye.name = "Eye";
        Object.DestroyImmediate(eye.GetComponent<Collider>());
        eye.transform.SetParent(go.transform, false);
        eye.transform.localPosition = new Vector3(0.35f, 0.45f, -0.2f);
        eye.transform.localScale = new Vector3(0.25f, 0.2f, 0.2f);
        eye.GetComponent<Renderer>().sharedMaterial = eyeMat;

        return go;
    }

    private static void CreatePlatform(string name, float centerX, float width, float topY, int layer, Material mat)
    {
        CreateBlock(name, new Vector3(centerX, topY - 0.5f, 0f), new Vector3(width, 1f, 3f), layer, mat);
    }

    private static void CreateBlock(string name, Vector3 position, Vector3 scale, int layer, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.layer = layer;
        go.transform.position = position;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = mat;
    }

    private static void SetFields(Object target, System.Action<SerializedObject> apply)
    {
        var so = new SerializedObject(target);
        apply(so);
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Material CreateMaterial(string name, Color color)
    {
        if (!AssetDatabase.IsValidFolder(MaterialFolder))
            AssetDatabase.CreateFolder("Assets/Scripts/MonsterAI", "Materials");

        string path = $"{MaterialFolder}/{name}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.color = color;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    // TagManager의 빈 사용자 레이어(8~31)에 Ground를 추가한다
    private static int EnsureLayer(string layerName)
    {
        int existing = LayerMask.NameToLayer(layerName);
        if (existing != -1) return existing;

        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(layer.stringValue))
            {
                layer.stringValue = layerName;
                tagManager.ApplyModifiedPropertiesWithoutUndo();
                return i;
            }
        }

        throw new System.InvalidOperationException("빈 레이어 슬롯이 없습니다.");
    }
}
