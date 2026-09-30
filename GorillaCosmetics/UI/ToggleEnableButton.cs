namespace GorillaCosmetics.UI
{
    public class ToggleEnableButton : BaseCosmeticButton
    {
        bool sendPress;

        protected override void Awake()
        {
            base.Awake();
            offText = onText = "CUSTOM";
            UpdateColor();
        }

        void Update()
        {
            if (!sendPress)
                return;
            sendPress = false;
            isOn = !isOn;
            if (isOn)
                Plugin.SelectionManager.Enable();
            else
                Plugin.SelectionManager.Disable();
            UpdateColor();
        }

        public override void ButtonActivation()
        {
            sendPress = true;
        }
    }
}
