using Newtonsoft.Json;
using System;
using System.Text;

namespace Domain
{
    public static class Extensions
    {
        public static T Deserialize<T>(this ReadOnlyMemory<byte> body)
            where T : class
        {
            T message = null;
            try
            {

                string rawMessage = Encoding.UTF8.GetString(body.ToArray());
                message = JsonConvert.DeserializeObject<T>(rawMessage);
            }
            catch (Exception ex)
            {
                return null;
            }

            return message;
        }
    }
}
