using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;

namespace Clean.Messaging.Inbox.Mqtt;

internal sealed partial class MqttInboxWorker(
    IServiceScopeFactory scopes,
    MqttInboxRouter router,
    IOptions<MqttInboxOptions> options,
    ILogger<MqttInboxWorker> logger)
    : BackgroundService
{
    private readonly MqttInboxOptions _options = options.Value;
    private readonly MqttClientFactory _mqtt = new();
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private readonly SemaphoreSlim _reconnect = new(0, 1);

    private CancellationToken _stoppingToken;
    private int _disconnecting;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _options.Validate();
        _stoppingToken = stoppingToken;

        await using (var scope = scopes.CreateAsyncScope())
        {
            _ = scope.ServiceProvider
                .GetRequiredService<IInbox>();
        }

        using var client = _mqtt.CreateMqttClient();

        client.ApplicationMessageReceivedAsync += Receive;
        client.DisconnectedAsync += Disconnected;

        try
        {
            await Run(
                client,
                stoppingToken);
        }
        finally
        {
            client.ApplicationMessageReceivedAsync -= Receive;
            client.DisconnectedAsync -= Disconnected;

            try
            {
                await Disconnect(
                    client,
                    CancellationToken.None);
            }
            catch (Exception exception)
            {
                DisconnectFailed(
                    logger,
                    exception);
            }
        }
    }

    private async Task Run(
        IMqttClient client,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Connect(
                    client,
                    cancellationToken);

                await _reconnect.WaitAsync(
                    cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                ConnectFailed(
                    logger,
                    exception,
                    _options.Host,
                    _options.Port);
            }

            try
            {
                await Disconnect(
                    client,
                    cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                DisconnectFailed(
                    logger,
                    exception);
            }

            await Task.Delay(
                _options.ReconnectDelay,
                cancellationToken);
        }
    }

    private async Task Connect(
        IMqttClient client,
        CancellationToken cancellationToken)
    {
        await _connectionLock.WaitAsync(
            cancellationToken);

        try
        {
            var result = await client.ConnectAsync(
                CreateOptions(),
                cancellationToken);

            EnsureConnected(result);

            foreach (var route in router.Routes)
            {
                await Subscribe(
                    client,
                    route,
                    cancellationToken);
            }

            Connected(
                logger,
                _options.Host,
                _options.Port,
                router.Routes.Count);
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private async Task Subscribe(
        IMqttClient client,
        IMqttInboxRoute route,
        CancellationToken cancellationToken)
    {
        var options = _mqtt
            .CreateSubscribeOptionsBuilder()
            .WithTopicFilter(
                route.TopicFilter,
                _options.QualityOfServiceLevel)
            .Build();

        var result = await client.SubscribeAsync(
            options,
            cancellationToken);

        EnsureSubscribed(
            route,
            result);
    }

    private static void EnsureConnected(
        MqttClientConnectResult result)
    {
        if (result.ResultCode ==
            MqttClientConnectResultCode.Success)
        {
            return;
        }

        throw new InvalidOperationException(
            $"MQTT connection was refused with result code " +
            $"'{result.ResultCode}' and reason '{result.ReasonString}'.");
    }

    private static void EnsureSubscribed(
        IMqttInboxRoute route,
        MqttClientSubscribeResult result)
    {
        var item = result.Items
            .SingleOrDefault();

        if (result.Items.Count == 1 &&
            item is not null &&
            item.ResultCode is
                MqttClientSubscribeResultCode.GrantedQoS1 or
                MqttClientSubscribeResultCode.GrantedQoS2)
        {
            return;
        }

        var reason = item is not null
            ? item.ResultCode.ToString()
            : $"Expected one subscription result but received {result.Items.Count}.";

        throw new InvalidOperationException(
            $"MQTT subscription to '{route.TopicFilter}' was not granted " +
            $"at QoS 1 or 2: {reason}. Reason: '{result.ReasonString}'.");
    }

    private MqttClientOptions CreateOptions()
    {
        var builder = new MqttClientOptionsBuilder()
            .WithProtocolVersion(
                MQTTnet.Formatter.MqttProtocolVersion.V500)
            .WithClientId(
                _options.ClientId)
            .WithTcpServer(
                _options.Host,
                _options.Port)
            .WithCleanStart(false)
            .WithSessionExpiryInterval(
                _options.SessionExpiryInterval);

        if (!string.IsNullOrWhiteSpace(
                _options.Username))
        {
            builder.WithCredentials(
                _options.Username,
                _options.Password);
        }

        if (_options.UseTls)
        {
            builder.WithTlsOptions(_ => { });
        }

        return builder.Build();
    }

    private async Task Disconnect(
        IMqttClient client,
        CancellationToken cancellationToken)
    {
        if (!client.IsConnected)
        {
            return;
        }

        await _connectionLock.WaitAsync(
            cancellationToken);

        try
        {
            if (!client.IsConnected)
            {
                return;
            }

            Interlocked.Exchange(
                ref _disconnecting,
                1);

            await client.DisconnectAsync(
                new MqttClientDisconnectOptions(),
                cancellationToken);
        }
        finally
        {
            Interlocked.Exchange(
                ref _disconnecting,
                0);

            _connectionLock.Release();
        }
    }

    private async Task Receive(
        MqttApplicationMessageReceivedEventArgs args)
    {
        args.AutoAcknowledge = false;

        try
        {
            IMqttInboxRoute route;

            try
            {
                route = router.Resolve(
                    args.ApplicationMessage.Topic);
            }
            catch (Exception exception)
            {
                await AcknowledgePoison(
                    args,
                    exception);

                return;
            }

            await using var scope =
                scopes.CreateAsyncScope();

            await route.Accept(
                scope.ServiceProvider,
                args.ApplicationMessage,
                _stoppingToken);

            await args.AcknowledgeAsync(
                _stoppingToken);
        }
        catch (OperationCanceledException)
            when (_stoppingToken.IsCancellationRequested)
        {
            // Leave the delivery unacknowledged during shutdown.
        }
        catch (MqttInboxPoisonMessageException exception)
        {
            await AcknowledgePoison(
                args,
                exception);
        }
        catch (Exception exception)
        {
            AdmissionFailed(
                logger,
                exception,
                args.ApplicationMessage.Topic);

            SignalReconnect();
        }
    }

    private async Task AcknowledgePoison(
        MqttApplicationMessageReceivedEventArgs args,
        Exception exception)
    {
        PoisonMessage(
            logger,
            exception,
            args.ApplicationMessage.Topic);

        try
        {
            await args.AcknowledgeAsync(
                _stoppingToken);
        }
        catch (OperationCanceledException)
            when (_stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception acknowledgeException)
        {
            PoisonAcknowledgeFailed(
                logger,
                acknowledgeException,
                args.ApplicationMessage.Topic);

            SignalReconnect();
        }
    }

    private Task Disconnected(
        MqttClientDisconnectedEventArgs _)
    {
        if (Volatile.Read(
                ref _disconnecting) == 0)
        {
            SignalReconnect();
        }

        return Task.CompletedTask;
    }

    private void SignalReconnect()
    {
        try
        {
            _reconnect.Release();
        }
        catch (SemaphoreFullException)
        {
        }
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Connected MQTT inbox to {Host}:{Port} with {RouteCount} routes.")]
    private static partial void Connected(
        ILogger logger,
        string host,
        int port,
        int routeCount);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Warning,
        Message = "MQTT inbox connection to {Host}:{Port} failed.")]
    private static partial void ConnectFailed(
        ILogger logger,
        Exception exception,
        string host,
        int port);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "MQTT message on topic {Topic} was rejected as poison and acknowledged.")]
    private static partial void PoisonMessage(
        ILogger logger,
        Exception exception,
        string topic);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Warning,
        Message = "MQTT poison acknowledgement failed on topic {Topic}; requesting reconnect for redelivery.")]
    private static partial void PoisonAcknowledgeFailed(
        ILogger logger,
        Exception exception,
        string topic);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Error,
        Message = "MQTT message on topic {Topic} could not be admitted to the inbox; requesting reconnect for redelivery.")]
    private static partial void AdmissionFailed(
        ILogger logger,
        Exception exception,
        string topic);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Debug,
        Message = "MQTT inbox disconnect failed during shutdown.")]
    private static partial void DisconnectFailed(
        ILogger logger,
        Exception exception);
}