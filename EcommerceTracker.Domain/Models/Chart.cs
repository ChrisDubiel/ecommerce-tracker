namespace EcommerceTracker.Domain.Models
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public enum ReportType
    {
        CategoryPercentage,
        PurchasesByCategory,
        PurchasesByNecessityValue
    }

    public class Data
    {
        public string Type { get; set; }
    }

    public class PieData : Data
    {
        public List<decimal> Values { get; set; }
        public List<string> Labels { get; set; }

        public PieData()
        {
            Values = new List<decimal>();
            Labels = new List<string>();
        }
    }

    public class TraceData : Data
    {
        public string Name { get; set; }
        public List<object> X { get; set; }
        public List<object> Y { get; set; }
        public string Mode { get; set; }
        public Line Line { get; set; }

        public TraceData()
        {
            X = new List<object>();
            Y = new List<object>();
            Line = new Line();
        }
    }

    public class Line
    {
        public string Dash { get; set; }
        public int Width { get; set; }
    }

    public class Layout
    {
        public string Title { get; set; }
        public int Height { get; set; }

        public Layout(string title, int height = 600)
        {
            Title = title;
            Height = height;
        }

        public Layout(int height = 600)
        {
            Height = height;
        }
    }

    public class Chart
    {
        // TODO: Find a better way of accomplishing this/ move JSON converter logic out of this class
        [JsonConverter(typeof(SingleOrArrayConverter<Data>))]
        public List<Data> Data { get; set; }
        public Layout Layout { get; set; }

        public Chart(List<Data> data, Layout layout)
        {
            Data = data;
            Layout = layout;
        }

        public Chart()
        {
            Data = new List<Data>();
            Layout = new Layout();
        }
    }

    public class SingleOrArrayConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<T>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            return token.Type == JTokenType.Array ? token.ToObject<List<T>>() : new List<T> { token.ToObject<T>() };
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
