using System.Text;
using JWT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ProvingService.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var jwt = "eyJhbGciOiJSUzI1NiIsImtpZCI6ImMzYWJlNDEzYjIyNjhhZTk3NjQ1OGM4MmMxNTE3OTU0N2U5NzUyN2UiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2FjY291bnRzLmdvb2dsZS5jb20iLCJhenAiOiI3MzcwMjgwNDA4NTgtOHVmcXNvYzdpNWtmc3NkdGt1N3Rtc2dzc25tOGZjOGQuYXBwcy5nb29nbGV1c2VyY29udGVudC5jb20iLCJhdWQiOiI3MzcwMjgwNDA4NTgtOHVmcXNvYzdpNWtmc3NkdGt1N3Rtc2dzc25tOGZjOGQuYXBwcy5nb29nbGV1c2VyY29udGVudC5jb20iLCJzdWIiOiIxMTAxMTcyMDcxMTQyMjExMTU4NjgiLCJhdF9oYXNoIjoiYmFUMlBubzhhUUNjbUdQNFJTbHFMQSIsIm5vbmNlIjoiNDI0MjQyNDI0MjQyNDI0MjQyNDI0MjQyNDI0MjQyNDI0MjQyNDI0MjQyNDI0MjQyNDI0MjQyNDI0MjQyNDI0MiIsImlhdCI6MTcxODI0MjA4NywiZXhwIjoxNzE4MjQ1Njg3fQ.VYfwgdy_Ukos8ScxehkqiCmtMFd5Zi1VgQY_dZvhzNX3A38XzG2JVr3jzGTslA5XNvHvpNHXgcDn3YMHZounA6tpl_rnYH6Pv_Q5yX1m8pO7wkli1-OhoSrVWsUro00PSUGFthGGrZ3ULUfZES3WCPf6Nh_CLM3RPZsOEzwZmlGrmUl98L2729PMprAdYLjlfwoYoVeqJdj36N7mHECfn0ruq9BSdV6ebAAU9O8eNg5xayp_7jSSNCzoK5T5mcbHcWlhIUdbRef72efbe8QviZeYuaq4odCg_JoQOoTDz1wzIbhfWa1ERaIZixPW2XuJv05W_dYz-j8KqRjG0Y-_Rg";
        byte[] bytes2 = new JwtBase64UrlEncoder().Decode(jwt.Split(".")[1]);
        var jsonValue = JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(bytes2));
        var subValue = jsonValue["sub"].ToString();
        var nonceValue = jsonValue["nonce"].ToString();
        Console.WriteLine("abc");
    }
}