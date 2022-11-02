namespace G24_BWallet_Backend.Models.ObjectType
{
    public class UserDebtReturn
    {
        public int UserId { get; set; }
        public string ReceiptName { get; set; }
        public string Date { get; set; }
        public string OwnerName { get; set; }   
        public double DebtLeft { get; set; }   
    }
}
