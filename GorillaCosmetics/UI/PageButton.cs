namespace GorillaCosmetics.UI
{
    public class PageButton : BaseCosmeticButton
    {
        public bool Forward { get; set; }

        public override void ButtonActivation()
        {
            if (Forward)
                Plugin.SelectionManager.NextPage();
            else
                Plugin.SelectionManager.PreviousPage();
        }
    }
}
