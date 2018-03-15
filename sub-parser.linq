<Query Kind="Program">
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>Newtonsoft.Json</Namespace>
</Query>

void Main()
{
	const string subKey = "CSP Subscription Id";
	const string userKey = "Azure UserName";
	const string passKey = "Azure Password";
	const string displayNameKey = "Azure DisplayName";
	
	var subs = new Dictionary<string, List<Tuple<string, string, string>>>();
	using (var file = File.OpenRead(@"C:\...\taiwanOH.txt"))
	using (var reader = new StreamReader(file))
	{
		while (!reader.EndOfStream)
		{
			var line = reader.ReadLine();
			
			if (string.IsNullOrWhiteSpace(line))
			{
				continue;
			}
			
			var data = line
				.Split(',')
				.ToDictionary(s => s.Substring(0, s.IndexOf(':')).Trim(), s => s.Substring(s.IndexOf(':') + 1).Trim(), StringComparer.InvariantCultureIgnoreCase);
				
			//data.Dump();
			
			var sub = data[subKey];
			var azUser = data[userKey];
			var azPass = data[passKey];
			
			string displayName;
			if (!data.TryGetValue(displayNameKey, out displayName))
			{
				// skip admin user
				continue;
			}
			
			var logins = subs.ContainsKey(sub) ? subs[sub] : new List<Tuple<string, string, string>>();
			logins.Add(new Tuple<string,string,string>(azUser, azPass, displayName));
			subs[sub] = logins;
		}
	}

	var teams = subs.Select((sub, index) =>
	{
		//$"Looking at team index {index}".Dump();
		
		var hubInfo = IndexToHubInfo(index);
		
		return new TeamInfo
		{
			//DayOfWeek = IndexToDayOfWeek(index),
			SubscriptionId = sub.Key,
			Usernames = sub.Value.Select(v => v.Item1).ToArray(),
			Passwords = sub.Value.Select(v => v.Item2).ToArray(),
			//DisplayNames = sub.Value.Select(v => v.Item3).ToArray(),
			TeamName = $"Team{IndexToTeamNumber(index)}",
//			EventHubNamespace = hubInfo.EventHubNamespace,
//			EventHubConsumerGroupName = hubInfo.EventHubConsumerGroupName,
//			EventHubPartitionCount = 4,
//			EventHubPolicyKey = hubInfo.EventHubPolicyKey,
//			EventHubName = hubInfo.EventHubName,
//			EventHubPolicyName = hubInfo.EventHubPolicyName
		};
	});


	// Teams as JSON
	//JsonConvert.SerializeObject(teams).Dump();


	// First login of each team as csv for login scripting
	foreach (var team in teams)
	{
		//$"{team.SubscriptionId},{team.Usernames.First()},{team.Passwords.First()},{team.TeamName}".Dump();
	}

	// CSV for logistics card printing

	//	"Day,Technology,Team,Azure DisplayName,Azure UserName,Azure Password,Secret Code".Dump();
	"Azure Username,Azure Password,Team Name,Subscription Id".Dump();
	foreach(var team in teams)
	{
		for(var i = 0; i < team.Usernames.Length; i++)
		{
			$"{team.Usernames[i]},{team.Passwords[i]},{team.TeamName},{team.SubscriptionId}".Dump();
		}
	}

}

private string IndexToDayOfWeek(int index)
{
	var days = new[] {"Wednesday","Thursday","Friday"};
	var day = (index / 40);
	return days[day];
}

private string IndexToTeamNumber(int index)
{
	var number = index + 1;
	var day = (index / 40) + 1;
	var team = (index % 40) + 1;
	return number.ToString();
	//return $"{day}{team.ToString().PadLeft(2, '0')}";
}

private HubInfo IndexToHubInfo(int index)
{
	var hubs = new[]
	{
		"Endpoint=sb://iothub-ns-ohdatasimh-329189-19eddd7a9e.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=ujPdStp6qdB1q9UZgnhnFT3QfaHlrkEGP/XVV4jnTrE=",
		"Endpoint=sb://iothub-ns-ohdatasimg-12345-19eddd7a9e.servicebus.windows.net/;SharedAccessKeyName=openhack;SharedAccessKey=fake2==",
		"Endpoint=sb://iothub-ns-ohdatasimg-abcde-19eddd7a9e.servicebus.windows.net/;SharedAccessKeyName=openhack;SharedAccessKey=fake3",
		"Endpoint=sb://iothub-ns-ohdatasimg-foobar-19eddd7a9e.servicebus.windows.net/;SharedAccessKeyName=openhack;SharedAccessKey=fake4",
	};
	
	var hubIndex = ((index) % 40) / 10;

	var connectionString = hubs[hubIndex];
	
	var parts = connectionString.Split(';').ToDictionary(s => s.Split('=').First(), s => s.Substring(s.IndexOf('=') + 1));

	//$"Raw index {index} becomes Team{IndexToTeamNumber(index)} on hub index {hubIndex}".Dump();
	
	var hubInfo = new HubInfo
	{
		EventHubPartitionCount = 4,
		EventHubConsumerGroupName = $"team{IndexToTeamNumber(index)}",
		EventHubName = $"ohdatasimhub{hubIndex + 1}",
		EventHubNamespace = parts["Endpoint"].Substring(5).Split('.')[0],
		EventHubPolicyKey = parts["SharedAccessKey"],
		EventHubPolicyName = parts["SharedAccessKeyName"]
	};
	
	//hubInfo.Dump();
	return hubInfo;
}

public class TeamInfo
{
	/*
	IoT Hub Namespace(EH compatible Endpoint)
	Event Hub Name
	Policy Name
	Policy Key
	Partition Count
	Consumer Group
	*/
	//public string DayOfWeek { get; set; }
	public string SubscriptionId { get; set; }
	public string[] Usernames { get; set; }
	//public string[] DisplayNames { get; set; }
	public string[] Passwords { get; set; }
	public string TeamName { get; set; }

//	public string EventHubNamespace { get; set; }
//	public string EventHubName { get; set; }
//	public string EventHubConsumerGroupName { get; set; }
//	public string EventHubPolicyName { get; set; }
//	public string EventHubPolicyKey {get;set;}
//	public int EventHubPartitionCount {get;set;}
}

public class HubInfo
{
	public string EventHubNamespace { get; set; }
	public string EventHubName { get; set; }
	public string EventHubConsumerGroupName { get; set; }
	public string EventHubPolicyName { get; set; }
	public string EventHubPolicyKey { get; set; }
	public int EventHubPartitionCount { get; set; }
}