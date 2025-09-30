using System.Text.Json.Serialization;

namespace Models
{
    public class PurchaseOrder
    {
        [JsonPropertyName("poNumber")]
        public string? PoNumber { get; set; }
        
        [JsonPropertyName("subTotal")]
        public double SubTotal { get; set; }
        
        [JsonPropertyName("tax")]
        public double Tax { get; set; }
        
        [JsonPropertyName("grandTotal")]
        public double GrandTotal { get; set; }
        
        [JsonPropertyName("supplierName")]
        public string? SupplierName { get; set; }
        
        [JsonPropertyName("buyerDepartment")]
        public string? BuyerDepartment { get; set; }
        
        [JsonPropertyName("notes")]
        public string? Notes { get; set; }
    }
}