namespace GorillaCosmetics.UI
{
    public class ViewButton : BaseCosmeticButton
    {
        ISelectionManager.SelectionView view;
        bool sendPress;

        public void SetView(ISelectionManager.SelectionView view)
        {
            this.view = view;
            offText = onText = view == ISelectionManager.SelectionView.Hat ? "HATS" : "MATERIALS";
            UpdateColor();
        }

        void Update()
        {
            if (!sendPress)
                return;
            sendPress = false;
            Plugin.SelectionManager.SetView(view);
        }

        public override void ButtonActivation()
        {
            sendPress = true;
        }
    }
}
