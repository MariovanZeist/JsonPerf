using LargeJson.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LargeJson.Client
{

	public class MyItemConverter : JsonConverter<MyItem>
	{
		public override MyItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
			{
				throw new JsonException();
			}
			reader.Read();

            var item = new MyItem();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return item;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    reader.Read();
                    switch (propertyName)
                    {
                        case nameof(MyItem.id):
                            item.id = reader.GetString();
                            break;
                        case nameof(MyItem.Name):
                            item.Name = reader.GetString();
                            break;
                        case nameof(MyItem.FullName):
                            item.FullName = reader.GetString();
                            break;
                        case nameof(MyItem.PrefVendorRef):
                            item.PrefVendorRef = JsonSerializer.Deserialize<Ref>(ref reader , options);
                            break;
                    }
                }
            }

            throw new JsonException();

		}

		public override void Write(Utf8JsonWriter writer, MyItem value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
	}

    public class RefConverter : JsonConverter<Ref>
    {
        public override Ref Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }
            reader.Read();

            var item = new Ref();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return item;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    reader.Read();
                    switch (propertyName)
                    {
                        case nameof(Ref.ListID):
                            item.ListID = reader.GetString();
                            break;
                        case nameof(Ref.FullName):
                            item.FullName = reader.GetString();
                            break;
                    }
                }
            }
            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Ref value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }


}
