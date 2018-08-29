﻿using Chiota.ViewModels.Classes;

namespace Chiota.ViewModels
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Linq;
  using System.Threading.Tasks;
  using System.Windows.Input;

  using Chiota.Chatbot;
  using Chiota.Events;

  using Microsoft.Bot.Connector.DirectLine;

  using Newtonsoft.Json;

  using Plugin.Connectivity;

  using Xamarin.Forms;

  public class BotChatViewModel : BaseViewModel
  {
    private readonly BotConnection connection;

    private readonly ListView messagesListView;

    private readonly StackLayout quickReplyStack;

    private readonly string profileImageUrl;

    private readonly string botId;

    private string outgoingText;

    private ObservableCollection<MessageViewModel> messagesList;

    public BotChatViewModel(BotObject bot, ListView messagesListView, StackLayout quickReplyStack)
        {
      this.connection = new BotConnection(bot);
      this.botId = bot.BotId;
      this.messagesListView = messagesListView;
      this.quickReplyStack = quickReplyStack;
      this.profileImageUrl = bot.ImageUrl;

      this.OutGoingText = null;

      this.Messages = new ObservableCollection<MessageViewModel>();
      this.GetMessagesAsync(this.Messages);
      this.outgoingText = null;
    }

    public static event EventHandler ActivityReceived;

    public ObservableCollection<MessageViewModel> Messages
    {
      get => this.messagesList;
      set
      {
        this.messagesList = value;
        this.OnPropertyChanged();
      }
    }

    public string OutGoingText
    {
      get => this.outgoingText;
      set
      {
        this.outgoingText = value;
        this.OnPropertyChanged();
      }
    }

    public ICommand SendCommand => new Command(async () => { await this.SendMessage(); });

    private async Task SendMessage()
    {
      this.Messages.Add(new MessageViewModel { Text = this.outgoingText, IsIncoming = false, MessagDateTime = DateTime.Now });
      this.CreateTyping();
      this.ScrollToNewMessage();
      await this.connection.SendMessageAsync(this.outgoingText);
      this.OutGoingText = null;
    }

    /// <summary>
    /// Start listening to messages from Bot framework
    /// </summary>
    /// <param name="messages">
    /// The messages.
    /// </param>
    private async void GetMessagesAsync(ICollection<MessageViewModel> messages)
    {
      while (true)
      {
        if (CrossConnectivity.Current.IsConnected)
        {
          var activitySet = await this.connection.GetActivitiesAsync();

          var i = 0;
          foreach (var activity in activitySet.Activities.Where(n => n.From.Id == this.botId))
          {
            // render different message types
            if (activity.Attachments.Count > 0)
            {
              foreach (var attachment in activity.Attachments)
              {
                switch (attachment.ContentType)
                {
                  case "application/vnd.microsoft.card.hero":
                    var heroCard = JsonConvert.DeserializeObject<HeroCard>(attachment.Content.ToString());
                    var title = string.IsNullOrEmpty(heroCard.Title) ? string.Empty : heroCard.Title + ": ";

                    messages.Add(
                      new MessageViewModel
                      {
                        Text = $"{title}{heroCard.Text}",
                        IsIncoming = true,
                        ImageSource = heroCard.Images != null ? heroCard.Images[0].Url : string.Empty,
                        MessagDateTime = DateTime.Now,
                        ProfileImage = this.profileImageUrl
                      });

                    this.quickReplyStack.Children.Clear();
                    if (heroCard.Buttons != null)
                    {
                      foreach (var button in heroCard.Buttons)
                      {
                        this.CreateQuickReply(button.Value.ToString());
                      }
                    }

                    break;
                  default:
                    messages.Add(
                      new MessageViewModel
                      {
                        Text = activity.Text,
                        IsIncoming = true,
                        MessagDateTime = DateTime.Now,
                        ProfileImage = this.profileImageUrl
                      });
                    break;
                }
              }
            }
            else
            {
              i++;
              if (i > 1)
              {
                this.CreateTyping();
                await Task.Delay(1000);
              }

              messages.Add(
                new MessageViewModel
                {
                  Text = activity.Text,
                  IsIncoming = true,
                  MessagDateTime = DateTime.Now,
                  ProfileImage = this.profileImageUrl
                });
              this.quickReplyStack.Children.Clear(); 
              if (activity.SuggestedActions != null)
              {
                foreach (var quickReply in activity.SuggestedActions.Actions)
                {
                  this.CreateQuickReply(quickReply.Value.ToString());
                }
              }
            }

            ActivityReceived?.Invoke(this, new ActivityEventArgs { Activity = activity });
            this.ScrollToNewMessage();
          }
        }

        await Task.Delay(3000);
      }
    }

    /// <summary>
    /// Scrolls ListView at the end
    /// </summary>
    private void ScrollToNewMessage()
    {
      // Todo causes everything to reload on windows
      var lastMessage = this.messagesListView?.ItemsSource?.Cast<object>().LastOrDefault();
      if (lastMessage != null)
      {
        this.messagesListView.ScrollTo(lastMessage, ScrollToPosition.MakeVisible, false);
      }
    }

    /// <summary>
    /// Creates a quick Reply
    /// </summary>
    /// <param name="buttonText">
    /// Quick reply text
    /// </param>
    private void CreateQuickReply(string buttonText)
    {
      var buttonOne = new Button
      {
        Text = buttonText,
        TextColor = Color.FromHex("#4286f4"),
        CornerRadius = 10,
        BorderColor = Color.FromHex("#4286f4"),
        BorderWidth = 2,
        HeightRequest = 40,
        BackgroundColor = Color.FromHex("#ffffff"),
        Command = new Command(
          async () =>
        {
          this.messagesList.Add(new MessageViewModel { Text = buttonText, IsIncoming = false, MessagDateTime = DateTime.Now, ProfileImage = this.profileImageUrl });
          this.CreateTyping();
          this.ScrollToNewMessage();
          await this.connection.SendMessageAsync(buttonText);
        })
      };
      this.quickReplyStack.Children.Add(buttonOne);
    }

    private void CreateTyping()
    {
        this.quickReplyStack.Children.Clear();
        for (var i = 0; i < 3; i++)
        {
          var roundFrame = new Button
                             {
                               VerticalOptions = LayoutOptions.Start,
                               HorizontalOptions = LayoutOptions.Start,
                               HeightRequest = 20,
                               WidthRequest = 20,
                               CornerRadius = 10,
                               BackgroundColor = Color.FromHex("#4286f4"),
                             };
          this.quickReplyStack.Children.Add(roundFrame);
        }
    }
  }
}