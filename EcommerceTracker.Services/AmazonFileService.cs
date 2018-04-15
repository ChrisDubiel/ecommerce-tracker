namespace EcommerceTracker.Services
{
    using CsvHelper;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using CsvHelper.Configuration;

    public class AmazonFileService
    {
        public IEnumerable<AmazonPurchase> ExtractPurchasesFromFile(TextReader amazonFile)
        {
            var csv = new CsvReader(amazonFile);
            csv.Configuration.RegisterClassMap<AmazonPurchaseMap>();
            return csv.GetRecords<AmazonPurchase>().ToList();
        }
    }

    public class AmazonPurchase
    {
        public DateTime OrderDate { get; set; }
        public string OrderId { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string ASINISBN { get; set; }
        public string UNSPSCCode { get; set; }
        public string Website { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string Condition { get; set; }
        public string Seller { get; set; }
        public string SellerCredentials { get; set; }
        public string ListPrice { get; set; }
        public string PurchasePricePerUnit { get; set; }
        public int Quantity { get; set; }
        public string PaymentInstrumentType { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public string POLineNumber { get; set; }
        public string OrderingCustomerEmail { get; set; }
        public DateTime? ShipmentDate { get; set; }
        public string ShippingAddressName { get; set; }
        public string ShippingAddressStreet1 { get; set; }
        public string ShippingAddressStreet2 { get; set; }
        public string ShippingAddressCity { get; set; }
        public string ShippingAddressState { get; set; }
        public string ShippingAddressZip { get; set; }
        public string OrderStatus { get; set; }
        public string CarrierNameAndTrackingNumber { get; set; }
        public string ItemSubtotal { get; set; }
        public string ItemSubtotalTax { get; set; }
        public string ItemTotal { get; set; }
        public string TaxExemptionApplied { get; set; }
        public string TaxExemptionType { get; set; }
        public string ExemptionOptOut { get; set; }
        public string BuyerName { get; set; }
        public string Currency { get; set; }
        public string GroupName { get; set; }
    }

    public sealed class AmazonPurchaseMap : ClassMap<AmazonPurchase>
    {
        public AmazonPurchaseMap()
        {
            Map(m => m.OrderDate).Name("Order Date");
            Map(m => m.OrderId).Name("Order ID");
            Map(m => m.Title).Name("Title");
            Map(m => m.Category).Name("Category");
            Map(m => m.ASINISBN).Name("ASIN/ISBN");
            Map(m => m.UNSPSCCode).Name("UNSPSC Code");
            Map(m => m.Website).Name("Website");
            Map(m => m.ReleaseDate).Name("Release Date");
            Map(m => m.Condition).Name("Condition");
            Map(m => m.Seller).Name("Seller");
            Map(m => m.SellerCredentials).Name("Seller Credentials");
            Map(m => m.ListPrice).Name("List Price Per Unit");
            Map(m => m.PurchasePricePerUnit).Name("Purchase Price Per Unit");
            Map(m => m.Quantity).Name("Quantity");
            Map(m => m.PaymentInstrumentType).Name("Payment Instrument Type");
            Map(m => m.PurchaseOrderNumber).Name("Purchase Order Number");
            Map(m => m.POLineNumber).Name("PO Line Number");
            Map(m => m.OrderingCustomerEmail).Name("Ordering Customer Email");
            Map(m => m.ShipmentDate).Name("Shipment Date");
            Map(m => m.ShippingAddressName).Name("Shipping Address Name");
            Map(m => m.ShippingAddressStreet1).Name("Shipping Address Street 1");
            Map(m => m.ShippingAddressStreet2).Name("Shipping Address Street 2");
            Map(m => m.ShippingAddressCity).Name("Shipping Address City");
            Map(m => m.ShippingAddressState).Name("Shipping Address State");
            Map(m => m.ShippingAddressZip).Name("Shipping Address Zip");
            Map(m => m.OrderStatus).Name("Order Status");
            Map(m => m.CarrierNameAndTrackingNumber).Name("Carrier Name & Tracking Number");
            Map(m => m.ItemSubtotal).Name("Item Subtotal");
            Map(m => m.ItemSubtotalTax).Name("Item Subtotal Tax");
            Map(m => m.ItemTotal).Name("Item Total");
            Map(m => m.TaxExemptionApplied).Name("Tax Exemption Applied");
            Map(m => m.TaxExemptionType).Name("Tax Exemption Type");
            Map(m => m.ExemptionOptOut).Name("Exemption Opt-Out");
            Map(m => m.BuyerName).Name("Buyer Name");
            Map(m => m.Currency).Name("Currency");
            Map(m => m.GroupName).Name("Group Name");
        }
    }
}
