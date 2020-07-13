using LargeJson.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LargeJson.Server.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class LargeDataController : ControllerBase
	{

		static CosmosQuery<MyItem> _cached;
		static readonly Random _rnd = new Random(1);

		static string CreateRandomString(int minLength, int rndLength)
		{
			return string.Create<object>(minLength + _rnd.Next(rndLength), null, (chars, _) => {
				for (int i = 0; i < chars.Length; i++) chars[i] = (char)('a' + _rnd.Next(26));
			});
		}

		static LargeDataController()
		{
			_cached = new CosmosQuery<MyItem>();
			var items = new List<MyItem>();
			for (int i = 0; i < 4600; i++)
			{
				items.Add(new MyItem
				{
					id = CreateRandomString(7, 8),
					Name = CreateRandomString(7, 8),
					FullName = CreateRandomString(18, 8),
					PrefVendorRef = new Ref
					{
						FullName = CreateRandomString(7, 8),
						ListID = CreateRandomString(3, 2)
					},
				});
			}
			_cached.Documents = items;
		}


		private readonly ILogger<WeatherForecastController> logger;

		public LargeDataController(ILogger<WeatherForecastController> logger)
		{
			this.logger = logger;
		}

		[HttpGet]
		public CosmosQuery<MyItem> Get()
		{
			return _cached;
		}
	}
}
