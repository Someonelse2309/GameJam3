using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UISoundTrigger : MonoBehaviour
{
    public enum SoundType
    {
        DefaultClick,
        QuestToggle,
        InventoryToggle,
        DialogueAdvance
    }

    public SoundType soundType = SoundType.DefaultClick;

    private void Start()
    {
        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(TriggerSound);
    }

    private void TriggerSound()
    {
        if (GameAudioManager.Instance == null) return;

        switch (soundType)
        {
            case SoundType.DefaultClick:
                GameAudioManager.Instance.PlaySFX(GameAudioManager.Instance.uiClickDefault);
                break;
            case SoundType.QuestToggle:
                GameAudioManager.Instance.PlayQuestTrackerSFX();
                break;
            case SoundType.InventoryToggle:
                GameAudioManager.Instance.PlayInventoryToggleSFX();
                break;
            case SoundType.DialogueAdvance:
                GameAudioManager.Instance.PlayDialogueNextSFX();
                break;
        }
    }
}