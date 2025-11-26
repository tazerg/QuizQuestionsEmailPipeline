using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using QuizQuestions.Logger;

namespace QuizQuestions.EmailClient
{
    public class GmailClient : IEmailClient
    {
        private const string LOG_TAG = nameof(GmailClient);
        
        private const string HOST = "imap.gmail.com";
        private const int PORT = 993;
        private const bool USE_SSL = true;
        
        private readonly string _username;
        private readonly string _password;

        public GmailClient(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public async Task<IReadOnlyList<EmailMessage>> GetUnprocessedEmailsAsync(string subject, int mailsCount)
        {
            Log.Debug(LOG_TAG, "Collect messages");
            using (var client = new ImapClient())
            {
                Log.Debug(LOG_TAG, "Start gmail client");
                
                await client.ConnectAsync(HOST, PORT, USE_SSL);
                await client.AuthenticateAsync(_username, _password);
                
                Log.Debug(LOG_TAG, "Authenticated");
                
                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadOnly);
                var uids = await inbox.SearchAsync(SearchQuery.NotSeen);
                var result = new List<EmailMessage>();
                
                Log.Debug(LOG_TAG, $"Collected {uids.Count} messages");
                Log.Debug(LOG_TAG, "Start reading");
                
                var progress = 0;
                foreach (var uid in uids)
                {
                    if (progress == mailsCount)
                        break;
                    
                    var message = await inbox.GetMessageAsync(uid);
                    
                    if (!string.Equals(message.Subject, subject))
                        continue;
                    
                    var textBody = message.TextBody ?? message.HtmlBody ?? string.Empty;
                    result.Add(new EmailMessage
                    {
                        UniqueId = uid,
                        Subject = message.Subject,
                        BodyText = textBody
                    });
                    
                    progress++;
                }
                
                Log.Debug(LOG_TAG, $"Read {result.Count} messages");
                
                await client.DisconnectAsync(true);
                
                Log.Debug(LOG_TAG, "Disconnected");
                return result;
            }
        }

        public async Task MarkAsProcessedAsync(IReadOnlyList<EmailMessage> messages)
        {
            Log.Debug(LOG_TAG, "Mark messages");
            using (var client = new ImapClient())
            {
                Log.Debug(LOG_TAG, "Start gmail client");

                await client.ConnectAsync(HOST, PORT, USE_SSL);
                await client.AuthenticateAsync(_username, _password);
                
                Log.Debug(LOG_TAG, "Authenticated");

                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadWrite);
                Log.Debug(LOG_TAG, "Start mark");
                foreach (var message in messages)
                {
                    await inbox.AddFlagsAsync(message.UniqueId, MessageFlags.Seen, true);
                }
                
                Log.Debug(LOG_TAG, $"Mark {messages.Count} messages");
                await client.DisconnectAsync(true);
                Log.Debug(LOG_TAG, "Disconnected");
            }
        }
    }
}