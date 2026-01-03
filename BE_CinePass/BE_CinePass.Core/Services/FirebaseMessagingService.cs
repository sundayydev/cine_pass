using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BE_CinePass.Core.Services;

/// <summary>
/// Service for sending push notifications via Firebase Cloud Messaging (FCM HTTP v1)
/// Updated for Firebase Admin SDK 3.x+
/// </summary>
public class FirebaseMessagingService
{
    private readonly ILogger<FirebaseMessagingService> _logger;
    private readonly FirebaseApp? _firebaseApp;
    private readonly bool _isInitialized;

    public FirebaseMessagingService(
        IConfiguration configuration,
        ILogger<FirebaseMessagingService> logger)
    {
        _logger = logger;

        try
        {
            var serviceAccountPath = configuration["Firebase:ServiceAccountPath"];
            
            if (string.IsNullOrEmpty(serviceAccountPath))
            {
                _logger.LogWarning("Firebase ServiceAccountPath not configured. Push notifications disabled.");
                _isInitialized = false;
                return;
            }

            if (!File.Exists(serviceAccountPath))
            {
                _logger.LogError("Firebase service account file not found at: {Path}", serviceAccountPath);
                _isInitialized = false;
                return;
            }

            // ✅ FIX: Use default instance or create named instance
            if (FirebaseApp.DefaultInstance == null)
            {
                _firebaseApp = FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(serviceAccountPath)
                });
                _logger.LogInformation("Firebase Admin SDK initialized (new instance)");
            }
            else
            {
                _firebaseApp = FirebaseApp.DefaultInstance;
                _logger.LogInformation("Firebase Admin SDK initialized (existing instance)");
            }

            _isInitialized = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Firebase Messaging Service");
            _isInitialized = false;
        }
    }

    /// <summary>
    /// Send notification to a single device
    /// </summary>
    public async Task<(bool Success, string? MessageId, string? Error)> SendToDeviceAsync(
        string deviceToken,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        string? imageUrl = null,
        string? platform = null)
    {
        if (!_isInitialized || _firebaseApp == null)
        {
            _logger.LogWarning("Firebase not initialized. Skipping push notification.");
            return (false, null, "Firebase not initialized");
        }

        try
        {
            // ✅ FIX: Build message with FCM HTTP v1 format
            var messageBuilder = new Message
            {
                Token = deviceToken,
                Notification = new Notification
                {
                    Title = title,
                    Body = body,
                    ImageUrl = imageUrl
                },
                Data = data
            };

            // Platform-specific configurations
            if (platform?.ToLower() == "ios")
            {
                messageBuilder.Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        Alert = new ApsAlert
                        {
                            Title = title,
                            Body = body
                        },
                        Badge = 1,
                        Sound = "default"
                    },
                    Headers = new Dictionary<string, string>
                    {
                        ["apns-priority"] = "10"
                    }
                };
            }
            else if (platform?.ToLower() == "android")
            {
                messageBuilder.Android = new AndroidConfig
                {
                    Notification = new AndroidNotification
                    {
                        Title = title,
                        Body = body,
                        Icon = "ic_notification",
                        Color = "#ec1337", // Cinemax red
                        Sound = "default",
                        ChannelId = "cinepass_notifications"
                    },
                    Priority = Priority.High
                };
            }

            var messaging = FirebaseMessaging.GetMessaging(_firebaseApp);
            var messageId = await messaging.SendAsync(messageBuilder);

            _logger.LogInformation("Push notification sent successfully. MessageId: {MessageId}", messageId);
            return (true, messageId, null);
        }
        catch (FirebaseMessagingException ex)
        {
            _logger.LogError(ex, "Failed to send push notification to device: {Token}", deviceToken);
            
            // Check if token is invalid
            if (ex.MessagingErrorCode == MessagingErrorCode.Unregistered ||
                ex.MessagingErrorCode == MessagingErrorCode.InvalidArgument)
            {
                return (false, null, "INVALID_TOKEN");
            }

            return (false, null, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending push notification");
            return (false, null, ex.Message);
        }
    }

    /// <summary>
    /// Send notification to multiple devices (batch)
    /// Uses FCM HTTP v1 SendEachForMulticastAsync (recommended approach)
    /// </summary>
    public async Task<(int SuccessCount, int FailureCount, List<string> InvalidTokens)> SendToDevicesAsync(
        List<string> deviceTokens,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        string? imageUrl = null)
    {
        if (!_isInitialized || _firebaseApp == null)
        {
            _logger.LogWarning("Firebase not initialized. Skipping batch push notifications.");
            return (0, deviceTokens.Count, new List<string>());
        }

        if (!deviceTokens.Any())
        {
            return (0, 0, new List<string>());
        }

        try
        {
            var messaging = FirebaseMessaging.GetMessaging(_firebaseApp);
            var invalidTokens = new List<string>();
            var successCount = 0;
            var failureCount = 0;

            // ✅ FIX: Use SendEachForMulticastAsync instead of SendMulticastAsync
            // FCM HTTP v1 recommends sending to each token individually for better error handling
            // But we can batch them in groups of 500
            const int batchSize = 500;
            var batches = deviceTokens
                .Select((token, index) => new { token, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.token).ToList())
                .ToList();

            foreach (var batch in batches)
            {
                var multicastMessage = new MulticastMessage
                {
                    Tokens = batch,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body,
                        ImageUrl = imageUrl
                    },
                    Data = data,
                    Android = new AndroidConfig
                    {
                        Priority = Priority.High,
                        Notification = new AndroidNotification
                        {
                            ChannelId = "cinepass_notifications",
                            Color = "#ec1337"
                        }
                    },
                    Apns = new ApnsConfig
                    {
                        Aps = new Aps
                        {
                            Badge = 1,
                            Sound = "default"
                        }
                    }
                };

                // ✅ FIX: Use SendEachForMulticastAsync (FCM HTTP v1 compatible)
                var response = await messaging.SendEachForMulticastAsync(multicastMessage);

                successCount += response.SuccessCount;
                failureCount += response.FailureCount;

                // Collect invalid tokens
                if (response.FailureCount > 0)
                {
                    for (int i = 0; i < response.Responses.Count; i++)
                    {
                        var sendResponse = response.Responses[i];
                        if (!sendResponse.IsSuccess)
                        {
                            var exception = sendResponse.Exception as FirebaseMessagingException;
                            if (exception?.MessagingErrorCode == MessagingErrorCode.Unregistered ||
                                exception?.MessagingErrorCode == MessagingErrorCode.InvalidArgument)
                            {
                                invalidTokens.Add(batch[i]);
                                _logger.LogWarning("Invalid token detected: {Token}", batch[i].Substring(0, 10) + "...");
                            }
                            else
                            {
                                _logger.LogWarning(
                                    "Failed to send to token (reason: {Reason})",
                                    exception?.MessagingErrorCode.ToString() ?? "Unknown");
                            }
                        }
                    }
                }

                _logger.LogInformation(
                    "Batch notification sent. Success: {SuccessCount}, Failed: {FailureCount}",
                    response.SuccessCount, response.FailureCount);
            }

            return (successCount, failureCount, invalidTokens);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send batch push notifications");
            return (0, deviceTokens.Count, new List<string>());
        }
    }

    /// <summary>
    /// Send data-only notification (silent notification)
    /// </summary>
    public async Task<(bool Success, string? MessageId, string? Error)> SendDataOnlyAsync(
        string deviceToken,
        Dictionary<string, string> data,
        string? platform = null)
    {
        if (!_isInitialized || _firebaseApp == null)
        {
            return (false, null, "Firebase not initialized");
        }

        try
        {
            var messageBuilder = new Message
            {
                Token = deviceToken,
                Data = data
            };

            // iOS requires content-available for silent notifications
            if (platform?.ToLower() == "ios")
            {
                messageBuilder.Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        ContentAvailable = true
                    },
                    Headers = new Dictionary<string, string>
                    {
                        ["apns-push-type"] = "background",
                        ["apns-priority"] = "5"
                    }
                };
            }

            var messaging = FirebaseMessaging.GetMessaging(_firebaseApp);
            var messageId = await messaging.SendAsync(messageBuilder);

            _logger.LogInformation("Silent push notification sent. MessageId: {MessageId}", messageId);
            return (true, messageId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send silent push notification");
            return (false, null, ex.Message);
        }
    }

    /// <summary>
    /// Subscribe device to a topic
    /// </summary>
    public async Task<bool> SubscribeToTopicAsync(List<string> deviceTokens, string topic)
    {
        if (!_isInitialized || _firebaseApp == null)
        {
            return false;
        }

        try
        {
            var messaging = FirebaseMessaging.GetMessaging(_firebaseApp);
            var response = await messaging.SubscribeToTopicAsync(deviceTokens, topic);

            _logger.LogInformation(
                "Subscribed {SuccessCount} devices to topic '{Topic}'. Failed: {FailureCount}",
                response.SuccessCount, topic, response.FailureCount);

            return response.SuccessCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe devices to topic: {Topic}", topic);
            return false;
        }
    }

    /// <summary>
    /// Unsubscribe device from a topic
    /// </summary>
    public async Task<bool> UnsubscribeFromTopicAsync(List<string> deviceTokens, string topic)
    {
        if (!_isInitialized || _firebaseApp == null)
        {
            return false;
        }

        try
        {
            var messaging = FirebaseMessaging.GetMessaging(_firebaseApp);
            var response = await messaging.UnsubscribeFromTopicAsync(deviceTokens, topic);

            _logger.LogInformation(
                "Unsubscribed {SuccessCount} devices from topic '{Topic}'",
                response.SuccessCount, topic);

            return response.SuccessCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unsubscribe devices from topic: {Topic}", topic);
            return false;
        }
    }

    /// <summary>
    /// Send notification to a topic
    /// </summary>
    public async Task<(bool Success, string? MessageId)> SendToTopicAsync(
        string topic,
        string title,
        string body,
        Dictionary<string, string>? data = null)
    {
        if (!_isInitialized || _firebaseApp == null)
        {
            return (false, null);
        }

        try
        {
            var message = new Message
            {
                Topic = topic,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data
            };

            var messaging = FirebaseMessaging.GetMessaging(_firebaseApp);
            var messageId = await messaging.SendAsync(message);

            _logger.LogInformation("Topic notification sent. MessageId: {MessageId}", messageId);
            return (true, messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send topic notification to: {Topic}", topic);
            return (false, null);
        }
    }

    /// <summary>
    /// Check if Firebase is properly initialized
    /// </summary>
    public bool IsInitialized() => _isInitialized;
}