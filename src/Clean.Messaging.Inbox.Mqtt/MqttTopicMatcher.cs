namespace Clean.Messaging.Inbox.Mqtt;

internal static class MqttTopicMatcher
{
    public static bool IsMatch(
        string filter,
        string topic)
    {
        if (topic.StartsWith('$') &&
            (filter.StartsWith('+') || filter.StartsWith('#')))
        {
            return false;
        }

        var filters = filter.Split('/');
        var topics = topic.Split('/');

        var topicIndex = 0;

        for (var filterIndex = 0;
             filterIndex < filters.Length;
             filterIndex++)
        {
            var part = filters[filterIndex];

            if (part == "#")
            {
                return filterIndex == filters.Length - 1;
            }

            if (topicIndex >= topics.Length)
            {
                return false;
            }

            if (part != "+" &&
                !string.Equals(
                    part,
                    topics[topicIndex],
                    StringComparison.Ordinal))
            {
                return false;
            }

            topicIndex++;
        }

        return topicIndex == topics.Length;
    }
}
