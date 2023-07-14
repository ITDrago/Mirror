using System;
using System.Collections.Generic;
using System.Linq;

public class RequestLimiter
{
    private static Dictionary<string, int> _requestCounters = new Dictionary<string, int>();
    private static object _lockObject = new object();
    private const int _maxRequestsPerInterval = 1;
    private const int _requestIntervalSeconds = 5;

    public static bool IsRequestAllowed(string userId)
    {
        lock (_lockObject)
        {
            // Удаляем записи о запросах, которые были отправлены более _requestIntervalSeconds назад.
            var expiredRequests = _requestCounters.Where(x => (Convert.ToInt32(DateTime.Now) - x.Value) > _requestIntervalSeconds).ToList();
            foreach (var expiredRequest in expiredRequests)
            {
                _requestCounters.Remove(expiredRequest.Key);
            }

            // Проверяем, сколько запросов было отправлено за последние _requestIntervalSeconds секунд.
            if (_requestCounters.ContainsKey(userId) && _requestCounters[userId] >= _maxRequestsPerInterval)
            {
                return false;
            }

            // Увеличиваем счетчик запросов для данного пользователя.
            if (!_requestCounters.ContainsKey(userId))
            {
                _requestCounters[userId] = 1;
            }
            else
            {
                _requestCounters[userId]++;
            }

            return true;
        }
    }
}