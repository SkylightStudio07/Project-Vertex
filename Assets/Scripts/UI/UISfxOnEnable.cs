using UnityEngine;

// 화면(시설 화면·패널)이 켜질 때 효과음을 한 번 낸다. 예: 무기고·의뢰·훈련장 화면이 열릴 때 ui_panel_open.
public class UISfxOnEnable : MonoBehaviour
{
    [SerializeField] private string sfx = "ui_panel_open";

    private void OnEnable() => UISfx.Play(sfx);

    public void Configure(string name) => sfx = name;
}
