using UnityEngine;

namespace GorillaCosmetics.UI
{
    public class BaseCosmeticButton : GorillaPressableButton
    {
        protected GorillaPressableButton original;
        bool wasEnabled;
        string defaultText;

        protected virtual void Awake()
        {
            foreach (GorillaPressableButton button in GetComponents<GorillaPressableButton>())
            {
                if (button != this)
                {
                    original = button;
                    break;
                }
            }
            wasEnabled = original.enabled;
            defaultText = original.myTmpText != null ? original.myTmpText.text : original.myText != null ? original.myText.text : original.offText;
            original.enabled = false;
            pressedMaterial = original.pressedMaterial;
            unpressedMaterial = original.unpressedMaterial;
            buttonRenderer = original.buttonRenderer;
            debounceTime = original.debounceTime;
            pressButtonSoundIndex = original.pressButtonSoundIndex;
            offText = original.offText;
            onText = original.onText;
            myText = original.myText;
            myTmpText = original.myTmpText;
            myTmpText2 = original.myTmpText2;
        }

        protected virtual void OnDestroy()
        {
            if (original != null)
            {
                original.enabled = wasEnabled;
                original.SetText(defaultText);
            }
        }

        public override void UpdateColor()
        {
            if (buttonRenderer != null)
                buttonRenderer.sharedMaterial = isOn ? pressedMaterial : unpressedMaterial;
            SetText(isOn ? onText : offText);
        }
    }
}
