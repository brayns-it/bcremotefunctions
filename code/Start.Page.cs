namespace Brayns.System
{
    public partial class Start
    {
        [Extended]
        protected void Extend_BCR()
        {
            var navigationPane = Control<Controls.NavigationPane>()!;

            var remfun = new Controls.ActionGroup(navigationPane, Label("Remote Functions"));
            {
                var actSetup = new Controls.Action(remfun, Label("Setup"), Icon.FromName("fas fa-cog"));
                {
                    new Controls.Action(actSetup, Label("Allowed path"), Icon.FromName("fas fa-user-lock"))
                    {
                        Run = typeof(BCRemoteFunctions.AllowedPathList)
                    };
                }
            }
        }
    }
}