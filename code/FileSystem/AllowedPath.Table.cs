﻿namespace BCRemoteFunctions
{
    public class AllowedPath : Table<AllowedPath>
    {
        public Fields.Code UserID { get; } = new("User ID", Label("User ID"), 50);
        public Fields.Text Path { get; } = new("Path", Label("Path"), 250);

        public AllowedPath()
        {
            TableName = "Allowed path";
            UnitCaption = Label("Allowed path");
            TablePrimaryKey.Add(UserID, Path);

            AddRelation<Brayns.System.User>(UserID);
        }
    }
}
