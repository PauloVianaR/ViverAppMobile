namespace ViverApp.Shared.Models.PagBank
{
    public class CheckoutResponse
    {
        public string? Id { get; set; }
        public string? Reference_id { get; set; }
        public DateTime? Expiration_date { get; set; }
        public DateTime Created_at { get; set; }
        public string? Status { get; set; }
        public CustomerResponse? Customer { get; set; }
        public bool? CustomerModifiable { get; set; }
        public List<ItemResponse>? Items { get; set; }
        public int? Additional_amount { get; set; }
        public int? Discount_amount { get; set; }
        public List<PaymentMethodRequest>? Payment_methods { get; set; }
        public List<PaymentMethodsConfigRequest>? Payment_methods_configs { get; set; }
        public string? Soft_descriptor { get; set; }
        public List<string>? Notification_urls { get; set; }
        public List<string>? Payment_notification_urls { get; set; }        
        public List<LinkResponse>? Links { get; set; }
        public string? Origin { get; set; }
    }
    public class CustomerResponse
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Tax_id { get; set; }
        public PhoneResponse? Phone { get; set; }
    }
    public class PhoneResponse
    {
        public string? Country { get; set; }
        public string? Area { get; set; }
        public string? Number { get; set; }
    }
    public class ItemResponse
    {
        public string? Reference_id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public int UnitAmount { get; set; }
        public string? Image_url { get; set; }
    }
    public class LinkResponse
    {
        public string? Rel { get; set; }
        public string? Href { get; set; }
        public string? Method { get; set; }
    }
}
