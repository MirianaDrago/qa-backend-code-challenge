using System.Text;
using Newtonsoft.Json;

public class WalletServiceClient {
    private readonly HttpClient _httpClient;

    public WalletServiceClient(string baseUrl)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    public AmountResponse GetBalance()
    {
        // Making an async call synchronous with GetAwaiter().GetResult()
        var response = _httpClient.GetAsync("/onlinewallet/balance").GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        return JsonConvert.DeserializeObject<AmountResponse>(content);
    }

    public AmountResponse Deposit(int amount)
    {
        var request = new { amount };
        var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        var response = _httpClient.PostAsync("/onlinewallet/deposit", content).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        var responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        return JsonConvert.DeserializeObject<AmountResponse>(responseBody);
    }

    public AmountResponse Withdraw(int amount)
    {
        var request = new { amount };
        var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        var response = _httpClient.PostAsync("/onlinewallet/withdraw", content).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        var responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        return JsonConvert.DeserializeObject<AmountResponse>(responseBody);
    }
}