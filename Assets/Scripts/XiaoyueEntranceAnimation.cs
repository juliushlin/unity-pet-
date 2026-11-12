using UnityEngine;

/// <summary>Painted entrance poses followed by a six-frame side-view run cycle.</summary>
public sealed class XiaoyueEntranceAnimation : MonoBehaviour
{
    public const float RunStartsAt = 3.05f;
    public const float RunDuration = 2.1f;
    public const float Duration = RunStartsAt + RunDuration + .25f;
    Mesh mesh;
    Material material;
    int currentFrame = -1;

    public void Initialize()
    {
        var texture = Resources.Load<Texture2D>("XiaoyueEntrance");
        if (!texture) throw new System.InvalidOperationException("缺少小月出场动画图集。");
        mesh = new Mesh { name = "Xiaoyue entrance quad" };
        mesh.vertices = new[] { new Vector3(-2.5f,-2.5f), new Vector3(-2.5f,2.5f),
            new Vector3(2.5f,2.5f), new Vector3(2.5f,-2.5f) };
        mesh.triangles = new[] { 0,1,2,0,2,3 };
        mesh.colors = new[] { Color.white,Color.white,Color.white,Color.white };
        mesh.RecalculateBounds();
        gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
        material = new Material(Resources.Load<Material>("PetSprite"));
        material.SetTexture("_MainTex", texture);
        material.SetFloat("_RevealMinY",-100);
        gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
        Sample(0);
    }

    public void Sample(float age)
    {
        int frame;
        if (age < .9f) frame = 0;
        else if (age < 1.45f) frame = 1;
        else if (age < 1.9f) frame = 2;
        else if (age < 2.5f) frame = 3;
        else if (age < 2.75f) frame = 4;
        else if (age < RunStartsAt) frame = 5;
        else if (age < RunStartsAt + RunDuration)
            frame = 6 + Mathf.FloorToInt((age - RunStartsAt) * 10) % 6;
        else frame = 5;
        if (currentFrame != frame)
        {
            currentFrame = frame;
            float left = frame % 4 / 4f, bottom = (2 - frame / 4) / 3f;
            mesh.uv = new[] { new Vector2(left,bottom),new Vector2(left,bottom+1f/3),
                new Vector2(left+.25f,bottom+1f/3),new Vector2(left+.25f,bottom) };
        }
        // The atlas supplies actual hand and limb articulation. Only the leap uses translation.
        float leap = age >= 1.9f && age < 2.5f
            ? Mathf.Sin((age-1.9f)/.6f*Mathf.PI)*.55f : 0;
        transform.localPosition = new Vector3(0,1.35f+leap,0);
        transform.localRotation = Quaternion.identity;
    }

    public static float TravelProgress(float age)
    {
        return Mathf.Clamp01((age-RunStartsAt)/RunDuration);
    }

    void OnDestroy()
    {
        if (mesh) Destroy(mesh);
        if (material) Destroy(material);
    }
}
