using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;

namespace QuizQuestions.EmailClient
{
    public class GmailClient : IEmailClient
    {
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

        public async Task<IReadOnlyList<EmailMessage>> GetUnprocessedEmailsAsync()
        {
            using (var client = new ImapClient())
            {
                await client.ConnectAsync(HOST, PORT, USE_SSL);
                await client.AuthenticateAsync(_username, _password);
                
                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadOnly);
                var uids = await inbox.SearchAsync(SearchQuery.NotSeen);
                var result = new List<EmailMessage>();

                const int MAX_MAILS = 1;
                var progress = 0;
                
                foreach (var uid in uids)
                {
                    if (progress == MAX_MAILS)
                        break;
                    
                    var message = await inbox.GetMessageAsync(uid);
                    var textBody = message.TextBody ?? message.HtmlBody ?? string.Empty;
                    result.Add(new EmailMessage
                    {
                        UniqueId = uid,
                        Subject = message.Subject,
                        BodyText = textBody
                    });
                    
                    progress++;
                }
                
                await client.DisconnectAsync(true);
                return result;
            }
        }

        public async Task MarkAsProcessedAsync(IReadOnlyList<EmailMessage> messages)
        {
            using (var client = new ImapClient())
            {
                await client.ConnectAsync(HOST, PORT, USE_SSL);
                await client.AuthenticateAsync(_username, _password);

                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadWrite);
                foreach (var message in messages)
                {
                    await inbox.AddFlagsAsync(message.UniqueId, MessageFlags.Seen, true);
                }
                
                await client.DisconnectAsync(true);
            }
        }
    }
}