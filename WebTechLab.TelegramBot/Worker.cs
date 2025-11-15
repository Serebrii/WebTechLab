using System.Text;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;


namespace WebTechLab.TelegramBot
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ITelegramBotClient _botClient;
        private readonly string _eventsApiUrl;
        private readonly HttpClient _httpClient;

        private record EventData(int id, string title, string description);
        private record ApiResponse(List<EventData> data);

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            var botToken = configuration["BotConfiguration:BotToken"]!;
            _eventsApiUrl = configuration["ApiSettings:EventsApiUrl"]!;

            _botClient = new TelegramBotClient(botToken);

            _httpClient = new HttpClient();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: stoppingToken
            );

            var me = await _botClient.GetMeAsync(stoppingToken);
            _logger.LogInformation("Бот {Username} запущений та слухає...", me.Username);
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { Text: { } messageText })
                return;

            var chatId = update.Message.Chat.Id;
            _logger.LogInformation("Отримано повідомлення '{MessageText}' у чаті {ChatId}.", messageText, chatId);

            if (messageText.ToLower() == "/start")
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Привіт! Я бот для `WebTechLab`. Введи /events, щоб побачити список подій.",
                    cancellationToken: cancellationToken);
                return;
            }

            if (messageText.ToLower() == "/events")
            {
                await SendEventsListAsync(chatId, cancellationToken);
                return;
            }
        }

        private async Task SendEventsListAsync(long chatId, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Роблю запит до API: {Url}", _eventsApiUrl);

                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                using var client = new HttpClient(handler);

                var response = await client.GetAsync($"{_eventsApiUrl}?pageSize=5", cancellationToken);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(jsonString, options);

                if (apiResponse == null || apiResponse.data.Count == 0)
                {
                    await _botClient.SendTextMessageAsync(chatId, "Подій не знайдено.", cancellationToken: cancellationToken);
                    return;
                }

                var messageBuilder = new StringBuilder();
                messageBuilder.AppendLine("🎉 *Ось 5 останніх подій:* \n");

                foreach (var ev in apiResponse.data)
                {
                    messageBuilder.AppendLine($"🔹 *{ev.title}*");
                    messageBuilder.AppendLine($"   _{ev.description}_\n");
                }

                await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: messageBuilder.ToString(),
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при отриманні подій з API");
                await _botClient.SendTextMessageAsync(chatId, "Ой, сталася помилка. Не можу підключитися до API.", cancellationToken: cancellationToken);
            }
        }

        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Помилка API Telegram:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.LogError(errorMessage);
            return Task.CompletedTask;
        }
    }
}