using APRSFunction.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace APRSFunction
{
    public class APRSApi
    {
        public string BaseAddress { get; set; }
        public string ApiKey { get; set; }

        public string CallSign { get; set; }

        private string FormatRequestAddress ()
        {
            return string.Format("{0}/api/get?name={1}&what=loc&apikey={2}&format=json", BaseAddress, CallSign, ApiKey);
        }


        public async Task<APRSResponse> GetLatest()
        {
            APRSResponse response = null;

            using (var httpClient = new HttpClient())
            {
                var result = await httpClient.GetAsync(FormatRequestAddress());

                if (result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    response = JsonConvert.DeserializeObject<APRSResponse>(content);
                }
            }

            return response;
        }

    }
}
