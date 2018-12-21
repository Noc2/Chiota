﻿#region References

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Chiota.Exceptions;
using Chiota.Extensions;
using Chiota.Models.Binding;
using Chiota.ViewModels.Base;
using Chiota.Views.Chat;
using Chiota.Views.Contact;
using Xamarin.Forms;
using ChatActionsView = Chiota.Views.Chat.ChatActionsView;

#endregion

namespace Chiota.ViewModels.Chat
{
    public class ChatsViewModel : BaseViewModel
    {
        #region Attributes

        private const int ChatItemHeight = 72;

        private List<ChatBinding> _chatList;

        private int _chatListHeight;

        private bool _isChatExist;
        private bool _isNoChatExist;

        private bool _isUpdating;

        #endregion

        #region Properties

        public List<ChatBinding> ChatList
        {
            get => _chatList;
            set
            {
                _chatList = value;
                OnPropertyChanged(nameof(ChatList));
            }
        }

        public int ChatListHeight
        {
            get => _chatListHeight;
            set
            {
                _chatListHeight = value;
                OnPropertyChanged(nameof(ChatListHeight));
            }
        }

        public bool IsChatExist
        {
            get => _isChatExist;
            set
            {
                _isChatExist = value;
                OnPropertyChanged(nameof(IsChatExist));
            }
        }

        public bool IsNoChatExist
        {
            get => _isNoChatExist;
            set
            {
                _isNoChatExist = value;
                OnPropertyChanged(nameof(IsNoChatExist));
            }
        }

        #endregion

        #region Constructors

        public ChatsViewModel()
        {
            _chatList = new List<ChatBinding>();
        }

        #endregion

        #region Init

        public override void Init(object data = null)
        {
            base.Init(data);

            UpdateView();
        }

        #endregion

        #region Reverse

        public override void Reverse(object data = null)
        {
            base.Reverse(data);

            UpdateView();
        }

        #endregion

        #region ViewIsAppearing

        protected override void ViewIsAppearing()
        {
            base.ViewIsAppearing();
        
            _isUpdating = true;
            Device.StartTimer(TimeSpan.FromSeconds(5), UpdateView);
        }

        #endregion

        #region ViewIsDisappearing

        protected override void ViewIsDisappearing()
        {
            base.ViewIsDisappearing();

            _isUpdating = false;
        }

        #endregion

        #region Methods

        #region UpdateView

        /// <summary>
        /// Init the view with the user data of the database and the contact requests by valid connection.
        /// </summary>
        private bool UpdateView()
        {
            //Show the contact requests and the chats of the user.
            Task.Run(async () =>
            {
                try
                {
                    var chats = new List<ChatBinding>();

                    //Load all accepted contacts.
                    var contacts = Database.Contact.GetAcceptedContacts();
                    foreach (var item in contacts)
                    {
                        //Get the last message of the contact.
                        var lastMessage = Database.Message.GetLastMessagesByPublicKeyAddress(item.PublicKeyAddress);

                        if (lastMessage == null) continue;

                        //If there is a message, load the chat of the contact.
                        var contact = new Pact.Palantir.Entity.Contact()
                        {
                            Name = item.Name,
                            ImagePath = item.ImagePath,
                            ChatAddress = item.ChatAddress,
                            ChatKeyAddress = item.ChatKeyAddress,
                            ContactAddress = item.ContactAddress,
                            PublicKeyAddress = item.PublicKeyAddress,
                            Rejected = !item.Accepted
                        };
                        chats.Add(new ChatBinding(contact, lastMessage.Value, lastMessage.Date));
                    }

                    //Update the chat list.
                    var changed = IsChatListChanged(chats);
                    if (changed)
                    {
                        ChatList = chats;
                        ChatListHeight = chats.Count * ChatItemHeight;
                    }

                    //Set flag to show the chats.
                    IsChatExist = chats.Count > 0;
                    IsNoChatExist = !(chats.Count > 0);
                }
                catch (Exception)
                {
                    //Unknown exception during update the chats view.
                    await new UnknownException(new ExcInfo()).ShowAlertAsync();
                    _isUpdating = false;
                }
            });

            return _isUpdating;
        }

        #endregion

        #region IsChatsListChanged

        private bool IsChatListChanged(List<ChatBinding> chats)
        {
            if (ChatList == null || ChatList.Count != chats.Count)
                return true;

            return false;
        }

        #endregion

        #endregion

        #region Commands

        #region Contacts

        public ICommand ContactsCommand
        {
            get
            {
                return new Command(async () =>
                {
                    await PushAsync<ChatActionsView>();
                });
            }
        }

        #endregion

        #region Tap

        public ICommand TapCommand
        {
            get
            {
                return new Command(async (param) =>
                {
                    if (param is ChatBinding chat)
                    {
                        //Show the chat view.
                        await PushAsync<ChatView>(chat.Contact);
                        return;
                    }

                    //Show an unknown exception.
                    await new UnknownException(new ExcInfo()).ShowAlertAsync();
                });
            }
        }

        #endregion

        #endregion
    }
}
