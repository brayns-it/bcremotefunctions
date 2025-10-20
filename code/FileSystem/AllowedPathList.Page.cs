namespace BCRemoteFunctions
{
    public class AllowedPathList : Page<AllowedPathList, AllowedPath>
    {
        public AllowedPathList()
        {
            UnitCaption = Label("Allowed path");
            Card = typeof(AllowedPathCard);

            var area = Controls.ContentArea.Create(this);
            {
                var grid = new Controls.Grid(area);
                {
                    new Controls.Field(grid, Rec.UserID);
                    new Controls.Field(grid, Rec.Path);
                    new Controls.Field(grid, Rec.CredentialCode);
                }
            }
        }
    }
}
