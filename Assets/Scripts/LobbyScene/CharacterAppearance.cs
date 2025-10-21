using UnityEngine;

public class CharacterAppearance : MonoBehaviour
{
    [Header("Main Body Renderer")]
    [SerializeField] private SkinnedMeshRenderer fullBodyRenderer;

    [Header("Optional Parts (Can be null if not used)")]
    [SerializeField] private SkinnedMeshRenderer hatRenderer;
    [SerializeField] private SkinnedMeshRenderer accessoriesRenderer;
    [SerializeField] private SkinnedMeshRenderer faceRenderer;
    [SerializeField] private SkinnedMeshRenderer glassesRenderer;
    [SerializeField] private SkinnedMeshRenderer glovesRenderer;
    [SerializeField] private SkinnedMeshRenderer hairstyleRenderer;
    [SerializeField] private SkinnedMeshRenderer outerwearRenderer;
    [SerializeField] private SkinnedMeshRenderer pantsRenderer;
    [SerializeField] private SkinnedMeshRenderer shoesRenderer;

    [Header("Mesh Library")]
    [SerializeField] private Mesh[] fullBodyMeshes;
    [SerializeField] private Mesh[] hatMeshes;
    [SerializeField] private Mesh[] accessoriesMeshes;
    [SerializeField] private Mesh[] faceMeshes;
    [SerializeField] private Mesh[] glassesMeshes;
    [SerializeField] private Mesh[] glovesMeshes;
    [SerializeField] private Mesh[] hairstyleMeshes;
    [SerializeField] private Mesh[] outerwearMeshes;
    [SerializeField] private Mesh[] pantsMeshes;
    [SerializeField] private Mesh[] shoesMeshes;

    // 범용 적용 함수
    public void ApplyAppearanceBase(int fullBody, int hat, int accessories, int faces, int glasses,
                                    int gloves, int hairstyle, int outerwear, int pants, int shoes, string nameTag = "")
    {
        Debug.Log($"[CharacterAppearance] Applying meshes for {nameTag}");

        SetMesh(fullBodyRenderer, fullBodyMeshes, fullBody);
        SetMesh(hatRenderer, hatMeshes, hat);
        SetMesh(accessoriesRenderer, accessoriesMeshes, accessories);
        SetMesh(faceRenderer, faceMeshes, faces);
        SetMesh(glassesRenderer, glassesMeshes, glasses);
        SetMesh(glovesRenderer, glovesMeshes, gloves);
        SetMesh(hairstyleRenderer, hairstyleMeshes, hairstyle);
        SetMesh(outerwearRenderer, outerwearMeshes, outerwear);
        SetMesh(pantsRenderer, pantsMeshes, pants);
        SetMesh(shoesRenderer, shoesMeshes, shoes);
    }

    public void ApplyAppearance(AppearanceResponse info)
    {
        if (info == null) return;
        ApplyAppearanceBase(info.FullBody, info.Hat, info.Accessories, info.Faces, info.Glasses,
                            info.Gloves, info.Hairstyle, info.Outerwear, info.Pants, info.Shoes, info.nickname);
    }

    public void ApplyAppearance(EnemyAppearanceData info)
    {
        if (info == null) return;
        ApplyAppearanceBase(info.FullBody, info.Hat, info.Accessories, info.Faces, info.Glasses,
                            info.Gloves, info.Hairstyle, info.Outerwear, info.Pants, info.Shoes, info.enemyName);
    }

    private void SetMesh(SkinnedMeshRenderer renderer, Mesh[] meshes, int index)
    {
        if (renderer == null || meshes == null || meshes.Length == 0) return;
        if (index < 0 || index >= meshes.Length)
        {
            Debug.LogWarning($"[CharacterAppearance] Invalid mesh index {index} for {renderer.name}");
            return;
        }
        renderer.sharedMesh = meshes[index];
    }
}
