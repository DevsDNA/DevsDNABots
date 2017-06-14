namespace DevsDNABot.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using System.Collections.Generic;
    using System.Linq;

    [Serializable]
    public class RootDialog : IDialog<object>
    {
        const string Services = "I want to know more about your services [Type 'services']";
        const string Team = "I want to know more about your team [Type 'team']";
        const string Work = "I want to know more about your work [Type 'work']";
        const string Contact = "I want to contact you [Type 'contact']";


        IEnumerable<string> options = new List<string> { "services", "team", "work", "contact" };
        IEnumerable<string> descriptions = new List<string> { Services, Team, Work, Contact };

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result;

            if(options.Contains(activity))
            {
                await RespondToUserAction(context, result);
            }
            else
            {
                PromptDialog.Choice<string>(context,
                RespondToUserAction, options,
                "What do you want to know?",
                "Sorry, we can't understand you. Please, can you respond again?",
                3, PromptStyle.PerLine, descriptions);
            }
            
        }

        private async Task RespondToUserAction(IDialogContext context, IAwaitable<object> result)
        {
            var selectedOption = await result as string;

            var message = context.MakeMessage();

            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;

            var attachments = GetAttachments(selectedOption);
            message.Attachments = attachments;

            await context.PostAsync(message);

            context.Wait(MessageReceivedAsync);
        }

        private IList<Attachment> GetAttachments(string selectedOption)
        {
            switch(selectedOption)
            {
                case "services":
                    return GetServicesCard();
                case "team":
                    return GetTeamCard();
                case "work":
                    return GetWorkCard();
                case "contact":
                    return GetContactCode();
                default:
                    return GetContactCode();
            }
        }

        private IList<Attachment> GetContactCode()
        {
            return new List<Attachment>
            {
                GetHeroCard("Contact", "", "", null, null)
            };
        }

        private IList<Attachment> GetWorkCard()
        {
            return new List<Attachment>
            {
                GetHeroCard("Project 1", "", "", null, null),
                GetHeroCard("Project 2", "", "", null, null),
                GetHeroCard("Project 3", "", "", null, null),
                GetHeroCard("Project 4", "", "", null, null)
            };
        }

        private IList<Attachment> GetTeamCard()
        {
            return new List<Attachment>
            {
                GetHeroCard("Beatriz Márquez Heredia", "CEO", 
                    "I am an entrepreneur who loves carrying out business and administrative tasks. This melds with my passion for technology to start this new venture. I truly believe a new way of doing things is possible. Yoga practitioner and mother of three beautiful children.", 
                    new CardImage("http://devsdna.com/portals/0/images/team/Beatriz-Marquez.jpg"), 
                    new CardAction(ActionTypes.OpenUrl, "Twitter", value: "https://www.twitter.com/hbeatrizm")),
                GetHeroCard("Yeray Julian Ferreiro", "Technical lead",
                    "Husband and father of three., invested the last fourteen years in Microsoft technologies, and the last three years playing with Xamarin and Windows Development. I was a Windows Phone MVP; now I am a Windows Platform MVP, Xamarin MVP and C# Corner MVP.",
                    new CardImage("http://devsdna.com/portals/0/images/team/Yeray-Julian.jpg"),
                    new CardAction(ActionTypes.OpenUrl, "Twitter", value: "https://www.twitter.com/josueyeray")),
                GetHeroCard("Ciani Afonso Díaz", "Senior Mobile Developer",
                    "I've been programming with Microsoft technologies for last ten years, I developed complex GPS tracking systems, insurance software, mobile apps (I was one of the winner of AppCampus prize) and more. I am always learning.",
                    new CardImage("http://devsdna.com/portals/0/images/team/Ciani-Afonso.jpg"),
                    new CardAction(ActionTypes.OpenUrl, "Twitter", value: "https://www.twitter.com/cianitwiter")),
                GetHeroCard("Oriol Noya Giner", "Senior Mobile Developer",
                    "I like to imagine, design, build and manage innovative digital products that help both people and businesses successfully meet their needs. Xamarin and Microsoft technologies are wonderful caregivers to smoothly achieve that.",
                    new CardImage("http://devsdna.com/portals/0/images/team/Oriol%20Noya%20web.jpg"),
                    new CardAction(ActionTypes.OpenUrl, "Twitter", value: "https://www.twitter.com/oriolnoya")),
                GetHeroCard("Marcos Cobeña Morián", "Senior Mobile Developer",
                    "Engineer from the <3. I noticed during childhood computers were going to be an important part of my life. Married with .NET since its early days, Xamarin has allowed me to make apps for those small devices i like so much. I specially enjoy the UI + UX side. I constandly wonder how people, including me, can be happier :)",
                    new CardImage("http://devsdna.com/portals/0/images/team/marcos.png"),
                    new CardAction(ActionTypes.OpenUrl, "Twitter", value: "https://www.twitter.com/MarcosCobena_")),
                GetHeroCard("Marco Antonio Blanco", "Mobile Developer",
                    "I grew up with a 486, modifying the scoring file of the Columns game with Wordperfect to win my brother. Now i want to learn the .net world where i started with Xamarin and i always go looking for the horizon. The competition against technologies will never end.",
                    new CardImage("http://devsdna.com/portals/0/images/team/marcoblanco.jpg"),
                    new CardAction(ActionTypes.OpenUrl, "Twitter", value: "https://www.twitter.com/marcoablanco"))
            };
        }

        private IList<Attachment> GetServicesCard()
        {
            return new List<Attachment>
            {
                GetHeroCard("Development", "", 
                    "Let us create great performing apps for you.",
                    new CardImage("https://www.bluecloudsolutions.com/wp-content/uploads/2015/03/development.jpg"), null),
                GetHeroCard("Training", "", 
                    "Learn new technologies from experienced teachers.",
                    new CardImage("http://www.vanilla-beans.in/wp-content/uploads/2013/03/training-and-skill-dev.jpg"), null),
                GetHeroCard("Mentoring", "", 
                    "We can help your team kick-start your projects.",
                    new CardImage("http://thepersonalbrandprofessor.com/wp-content/uploads/2014/09/Mentoring-Pic-1.jpg"), null),
                GetHeroCard("Community", "", 
                    "We love community and giving talks and workshops.",
                    new CardImage("https://cdn.elegantthemes.com/blog/wp-content/uploads/2016/04/community-wordpress.jpg"), null)
            };
        }

        private Attachment GetHeroCard(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction)
        {
            var heroCard = new HeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text
            };

            if (cardImage != null)
                heroCard.Images = new List<CardImage> { cardImage };

            if (cardAction != null)
                heroCard.Buttons = new List<CardAction> { cardAction };

            return heroCard.ToAttachment();
        }
    }
}