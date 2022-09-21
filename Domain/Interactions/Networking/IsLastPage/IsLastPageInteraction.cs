namespace Domain.Interactions.Networking.IsLastPage
{
    public class IsLastPageInteraction : InteractionBase
    {
        public int CurrentPage { get; set; }
        public int TotalResults { get; set; }
        public bool VerifyWithWebDriver { get; set; }
    }
}
