using System;
using System.Runtime.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 카드 상세 UI.
// 카드 우클릭 시 띄워지는 뷰와 확인 모드(제거/강화) 두 가지
public class CardDetailView : MonoBehaviour
{
    public static CardDetailView Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private CardView detailCardView;
    [SerializeField] private Button backdropButton; // 바깥 클릭 시 닫기

    [Header("확인 모드")]
    [SerializeField] private GameObject actionButtonGroup; // 확인/취소 버튼 묶음
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI confirmLabelText;

    private bool isConfirmMode = false;
    private Action onConfirm;
    private Action onCancel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (backdropButton != null) backdropButton.onClick.AddListener(HandleBackdropClicked);
        if(confirmButton != null) confirmButton.onClick.AddListener(Confirm);
        if(cancelButton != null) cancelButton.onClick.AddListener(Cancel);
        if (panel != null) panel.SetActive(false);
    }

    // 보기 전용 - 카드 상세 UI
    public void Show(CardData card)
    {
        if (!SetCard(card)) 
        { 
            Debug.LogWarning("CardDetailView.Show called with null card.");
            return;
        }

        isConfirmMode = false;
        onConfirm = null;
        onCancel = null;
        if (actionButtonGroup != null) actionButtonGroup.SetActive(false);
    }

    // 확인 모드 - 확인 버튼 클릭 시 onConfirm 호출, 취소 버튼 클릭 시 onCancel 호출
    // 바깥 배경 클릭 시에도 여전히 취소 처리
    public void ShowWithConfirmation(CardData card, string confirmLabel, Action onConfirm, Action onCancel = null)
    {
        if (!SetCard(card)) return;

        isConfirmMode = true;
        this.onConfirm = onConfirm;
        this.onCancel = onCancel;

        if(actionButtonGroup != null) actionButtonGroup.SetActive(true);
        if (confirmLabelText != null) confirmLabelText.text = confirmLabel;
    }

    public void Hide()
    {
        isConfirmMode = false;
        onConfirm = null;
        onCancel = null;
        if (panel != null) panel.SetActive(false);
    }

    private void Confirm()
    {
        var callback = onConfirm;
        Hide();
        callback?.Invoke();
    }
    private void Cancel()
    {
        var callback = onCancel;
        Hide();
        callback?.Invoke();
    }
    // 확인 모드에서 바깥 클릭은 '취소'로 처리한다
    private void HandleBackdropClicked()
    {
        if (isConfirmMode) Cancel();
        else Hide();
    }

    private bool SetCard(CardData card)
    {
        if (card == null || panel == null) return false;

        detailCardView.SetCard(card);
        panel.SetActive(true);
        return true;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}