namespace ViverApp.Shared.Models.PagBank
{
    public class CheckoutCreateRequest
    {
        public string? Reference_id { get; set; }
        public DateTime? Expiration_date { get; set; }
        public CustomerRequest? Customer { get; set; }
        public bool? Customer_modifiable { get; set; }
        public List<ItemRequest>? Items { get; set; }
        public int? Additional_amount { get; set; }
        public int? Discount_amount { get; set; }
        public ShippingRequest? Shipping { get; set; }
        public List<PaymentMethodRequest>? Payment_methods { get; set; }
        public List<PaymentMethodsConfigRequest>? Payment_methods_configs { get; set; }
        public string? Redirect_url { get; set; }
        public string? Soft_descriptor { get; set; }
        public string? Return_url { get; set; }        
        public List<string>? Notification_urls { get; set; }
        public List<string>? Payment_notification_urls { get; set; }
        public DateTime Created_at { get; } = DateTime.Now;
    }
    public class CustomerRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Tax_id { get; set; }
        public PhoneRequest? Phone { get; set; }
    }
    public class PhoneRequest
    {
        public string? Country { get; set; }
        public string? Area { get; set; }
        public string? Number { get; set; }
    }
    public class ItemRequest
    {
        public string? Reference_id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? Quantity { get; set; }
        public int? Unit_amount { get; set; }
        public string? Image_url { get; set; }
    }
    public class ShippingRequest
    {
        public string? Type { get; set; }         // FIXED, FREE, CALCULATE
        public string? Service_type { get; set; }  // e.g., SEDEX, PAC
        public bool? Address_modifiable { get; set; }
        public int? Amount { get; set; }
        public AddressRequest? Address { get; set; }
        public BoxRequest? Box { get; set; }
    }
    public class AddressRequest
    {
        public string? Street { get; set; }
        public string? Number { get; set; }
        public string? Complement { get; set; }
        public string? Locality { get; set; }
        public string? City { get; set; }
        public string? Region_code { get; set; }
        public string? Country { get; set; }
        public string? Postal_code { get; set; }
    }
    public class BoxRequest
    {
        public int Weight { get; set; }
        public BoxDimensionsRequest? Dimensions { get; set; }
    }
    public class BoxDimensionsRequest
    {
        public int Length { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class PaymentMethodRequest
    {
        public string? Type { get; set; }         // CREDIT_CARD, DEBIT_CARD, PIX, BOLETO
        public List<string>? Brands { get; set; }
    }
    public class PaymentMethodsConfigRequest
    {
        public string? Type { get; set; }         // CREDIT_CARD or DEBIT_CARD
        public List<ConfigOptionRequest>? ConfigOptions { get; set; }
    }
    public class ConfigOptionRequest
    {
        public string? Option { get; set; }       // INSTALLMENTS_LIMIT, INTEREST_FREE_INSTALLMENTS
        public string? Value { get; set; }
    }

}
