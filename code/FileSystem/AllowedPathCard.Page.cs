namespace BCRemoteFunctions
{
    public class AllowedPathCard : Page<AllowedPathCard, AllowedPath>
    {
        public AllowedPathCard()
        {
            UnitCaption = Label("Allowed path");

            var area = Controls.ContentArea.Create(this);
            {
                var general = new Controls.Group(area, Label("General"));
                {
                    new Controls.Field(general, Rec.UserID);
                    new Controls.Field(general, Rec.Path);
                    new Controls.Field(general, Rec.CredentialCode);
                }
            }
        }
    }
}
