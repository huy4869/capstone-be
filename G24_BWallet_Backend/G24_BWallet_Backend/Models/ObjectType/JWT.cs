namespace G24_BWallet_Backend.Models.ObjectType
{
    public class JWT
    {
        public string AccessToken { get; set; }

        public JWT(string AccessToken)
        {
            this.AccessToken = AccessToken;
        }
    }
}
