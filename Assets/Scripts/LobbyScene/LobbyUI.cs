using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    [Header("Text UI")]
    [SerializeField] private TMP_Text NickNameText;
    [SerializeField] private TMP_Text LevelText;
    [SerializeField] private TMP_Text Coin0Text;
    [SerializeField] private TMP_Text Coin1Text;
    [SerializeField] private TMP_Text RankText;

    [Header("Slider UI")]
    [SerializeField] private Slider PlayerEXP;
    [SerializeField] private Slider RankScoreSlider;

    [Header("Image UI")]
    [SerializeField] private Image RankImage;
    [SerializeField] private Image IconImage;
    [SerializeField] private Image IconBorderImage;

    [Header("Sprites")]
    [SerializeField] private Sprite[] RankSprites;
    [SerializeField] private Sprite[] IconSprites;
    [SerializeField] private Sprite[] IconBorderSprites;

    public void ApplyPlayerInfo(PlayerInfoResponse info)
    {
        // === Text ===
        NickNameText.text = info.nickname;
        LevelText.text = $"Lv. {info.level}";
        Coin0Text.text = info.coin0.ToString();
        Coin1Text.text = info.coin1.ToString();

        string[] rankNames = {
            "Iron", "Bronze", "Silver", "Gold",
            "Platinum", "Diamond", "Master",
            "Grandmaster", "Challenger"
        };

        string rankName = (info.rank >= 0 && info.rank < rankNames.Length)
            ? rankNames[info.rank]
            : "Unknown";

        RankText.text = rankName;

        // === Slider ===
        PlayerEXP.maxValue = info.level * 100;
        PlayerEXP.value = info.exp;

        RankScoreSlider.maxValue = info.rank * 100;
        RankScoreSlider.value = info.rankScore;

        // === Images ===
        if (info.rank >= 0 && info.rank < RankSprites.Length)
            RankImage.sprite = RankSprites[info.rank];

        if (info.icon >= 0 && info.icon < IconSprites.Length)
            IconImage.sprite = IconSprites[info.icon];

        if (info.iconBorder >= 0 && info.iconBorder < IconBorderSprites.Length)
            IconBorderImage.sprite = IconBorderSprites[info.iconBorder];

        Debug.Log($"[UI] Updated player info: {info.nickname}, Lv.{info.level}, Rank:{info.rank}");
    }
}
