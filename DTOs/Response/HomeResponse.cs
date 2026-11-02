namespace Rafiq.DTOs.Response
{
    public class HomeResponse
    {
        public List<QuestionResponse> ImportantQuestions { get; set; }
        public List<DidYouKnowAndAdvertisementResponse> Advertisements { get; set; }
        public List<DidYouKnowAndAdvertisementResponse> DidYouKnows { get; set; }
        public List<ProductResponse> TopSellingProducts { get; set; }
        public List<ProductResponse> NewArrivals { get; set; }
    }
}
