namespace NativeBotClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Foundation;
    using JSQMessagesViewController;
    using Microsoft.Bot.Connector.DirectLine;
    using Newtonsoft.Json;
    using UIKit;

    public partial class ViewController : MessagesViewController
    {
        DirectLineClient _client;
        Conversation _conversation;

        static MessagesBubbleImageFactory bubbleFactory = new MessagesBubbleImageFactory();
        static UIImage _outgoingAvatarImage = UIImage.FromBundle("logo");
        static UIImage _incomingAvatarImage = UIImage.FromBundle("logo");

        MessagesBubbleImage _outgoingBubbleImageData = bubbleFactory.CreateOutgoingMessagesBubbleImage(UIColorExtensions.MessageBubbleLightGrayColor);
        MessagesBubbleImage _incomingBubbleImageData = bubbleFactory.CreateIncomingMessagesBubbleImage(new UIColor(red: 0.51f, green: 0.18f, blue: 0.51f, alpha: 1.0f));

        MessagesAvatarImage _outgoingAvatar = new MessagesAvatarImage(_outgoingAvatarImage, _outgoingAvatarImage, _outgoingAvatarImage);
        MessagesAvatarImage _incomingAvatar = new MessagesAvatarImage(_incomingAvatarImage, _incomingAvatarImage, _incomingAvatarImage);

        List<Message> _messages = new List<Message>();
        List<string> _activitiesPrinted = new List<string>();

        string directLineSecret = "2dqJSatIlAY.cwA.v6E.5Git6FP_atwyBDZHH0Xam-ysJZGeyJ590VS41w1KGHQ";
        string botId = "devsdna-web";

		User sender = new User { Id = "2CC8343", DisplayName = "You" };
		User friend = new User { Id = "BADB229", DisplayName = "Xamarin Bot" };


        protected ViewController(IntPtr handle) : base(handle)
        {
            
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitMessagesVC();

            await InitBotFramework();

            ShowWelcomeMessage();
        }

        void ShowWelcomeMessage()
        {
			_messages.Add(new Message(friend.Id, friend.DisplayName, NSDate.DistantPast, "Welcome to DevsDNA Base Bot Client! Say anything to start our conversation"));

			FinishReceivingMessage(true);
        }

        async Task InitBotFramework()
        {
			_client = new DirectLineClient(directLineSecret);
			_conversation = await _client.Conversations.StartConversationAsync();
        }

        void InitMessagesVC()
        {
			SenderId = sender.Id;
			SenderDisplayName = sender.DisplayName;

			InputToolbar.ContentView.LeftBarButtonItem = null;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
		{
			var cell = base.GetCell(collectionView, indexPath) as MessagesCollectionViewCell;

			var message = _messages[indexPath.Row];
			if (message.SenderId == SenderId)
				cell.TextView.TextColor = UIColor.Black;

			return cell;
		}

		public override nint GetItemsCount(UICollectionView collectionView, nint section)
		{
			return _messages.Count;
		}

		public override IMessageData GetMessageData(MessagesCollectionView collectionView, NSIndexPath indexPath)
		{
			return _messages[indexPath.Row];
		}

		public override IMessageBubbleImageDataSource GetMessageBubbleImageData(MessagesCollectionView collectionView, NSIndexPath indexPath)
		{
			var message = _messages[indexPath.Row];
			if (message.SenderId == SenderId)
				return _outgoingBubbleImageData;
			return _incomingBubbleImageData;
		}

		public override IMessageAvatarImageDataSource GetAvatarImageData(MessagesCollectionView collectionView, NSIndexPath indexPath)
		{
			var message = _messages[indexPath.Row];
			if (message.SenderId == SenderId)
				return _incomingAvatar;
			return _outgoingAvatar;
		}

		public override async void PressedSendButton(UIButton button, string text, string senderId, string senderDisplayName, NSDate date)
		{
			InputToolbar.ContentView.TextView.Text = "";
			InputToolbar.ContentView.RightBarButtonItem.Enabled = false;
			SystemSoundPlayer.PlayMessageSentSound();

            var myMessage = new Message("2CC8343", "You", NSDate.Now, text);
			_messages.Add(myMessage);
			FinishReceivingMessage(true);

			ShowTypingIndicator = true;

			await SendMessage(text);

            await ReadBotMessagesAsync();
		}

		public async Task SendMessage(string messageText)
		{
			try
			{
				Activity userMessage = new Activity
				{
                    From = new ChannelAccount(sender.DisplayName),
                    Text = messageText,
					Type = ActivityTypes.Message
				};

                await _client.Conversations.PostActivityAsync(_conversation.ConversationId, userMessage);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		async Task ReadBotMessagesAsync()
		{
			string watermark = null;

            var activitySet = await _client.Conversations.GetActivitiesAsync(_conversation.ConversationId, watermark);
			watermark = activitySet?.Watermark;

			var activities = from x in activitySet.Activities
							 where x.From.Id == botId
							 select x;

			foreach (Activity activity in activities)
			{
                if (!_activitiesPrinted.Contains(activity.Id))
                {
					_activitiesPrinted.Add(activity.Id);

					if (!string.IsNullOrEmpty(activity.Text))
					{
						ScrollToBottom(true);

						SystemSoundPlayer.PlayMessageReceivedSound();

						var messageBot = new Message(friend.Id, friend.DisplayName, NSDate.Now, activity.Text);
						_messages.Add(messageBot);

						FinishReceivingMessage(true);
						InputToolbar.ContentView.RightBarButtonItem.Enabled = true;
					}

					if (activity.Attachments != null)
					{
						foreach (Attachment attachment in activity.Attachments)
						{
							switch (attachment.ContentType)
							{
								case "application/vnd.microsoft.card.hero":
									RenderHeroCard(attachment);
									break;

								case "image/png":
									Console.WriteLine($"Opening the requested image '{attachment.ContentUrl}'");
									break;
							}
						}
					}   
                }
			}

		}

		private static void RenderHeroCard(Attachment attachment)
		{
			const int Width = 70;
			Func<string, string> contentLine = (content) => string.Format($"{{0, -{Width}}}", string.Format("{0," + ((Width + content.Length) / 2).ToString() + "}", content));

			var heroCard = JsonConvert.DeserializeObject<HeroCard>(attachment.Content.ToString());

			if (heroCard != null)
			{
				Console.WriteLine("/{0}", new string('*', Width + 1));
				Console.WriteLine("*{0}*", contentLine(heroCard.Title));
				Console.WriteLine("*{0}*", new string(' ', Width));
				Console.WriteLine("*{0}*", contentLine(heroCard.Text));
				Console.WriteLine("{0}/", new string('*', Width + 1));
			}
		}
    }

	public class User
	{
		public string Id { get; set; }
		public string DisplayName { get; set; }
	}
}
