﻿#region References

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Chiota.Exceptions;
using Chiota.Extensions;
using Chiota.Models.Binding;
using Chiota.Models.Database;
using Chiota.Services.Ipfs;
using Chiota.ViewModels.Base;
using Chiota.Views.Contact;
using Xamarin.Forms;

#endregion

namespace Chiota.ViewModels.Contact
{
    public class ContactRequestsViewModel : BaseViewModel
    {
        #region Attributes

        private const int RequestItemHeight = 65;

        private List<ContactBinding> _requestList;

        private int _requestListHeight;

        private bool _isRequestExist;
        private bool _isNoRequestExist;

        private bool _isUpdating;

        #endregion

        #region Properties

        public List<ContactBinding> RequestList
        {
            get => _requestList;
            set
            {
                _requestList = value;
                OnPropertyChanged(nameof(RequestList));
            }
        }

        public int RequestListHeight
        {
            get => _requestListHeight;
            set
            {
                _requestListHeight = value;
                OnPropertyChanged(nameof(RequestListHeight));
            }
        }

        public bool IsRequestExist
        {
            get => _isRequestExist;
            set
            {
                _isRequestExist = value;
                OnPropertyChanged(nameof(IsRequestExist));
            }
        }

        public bool IsNoRequestExist
        {
            get => _isNoRequestExist;
            set
            {
                _isNoRequestExist = value;
                OnPropertyChanged(nameof(IsNoRequestExist));
            }
        }

        #endregion

        #region Constructors

        public ContactRequestsViewModel()
        {
            _requestList = new List<ContactBinding>();
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
                    var contactRequests = new List<ContactBinding>();

                    var requests = Database.Contact.GetUnacceptedContacts();

                    /*if (requests == null || requests.Count == 0)
                    {
                        requests = new List<DbContact>();
                        for (var i = 0; i < 16; i++)
                        {
                            requests.Add(new DbContact()
                            {
                                Name = "Test",
                                ChatKeyAddress = "QRUJNINNFGOUVRSVHHIFOVDPWYMZTUQZOSFVUDFHBQGAIVMFQHESCIRWGSOM9GRKRXIWWGGBNUFJGLXWE",
                                ChatAddress = "HRIGIBGTKNHHFOVJWFULBHHIOEVBCAGYKMEN9HGLLNKUGRZMNKGSAY9KZEDWSYICPJYQZPNGMBJNVUGNB",
                                ContactAddress = "JVBKBETSMRYWLBMIAOICHG9JYBMNVSJTPMYTLCNANGOULMKMJGIBLQQBPXNCZYMMEZONPVFDZKDVB99MD",
                                PublicKeyAddress = "EL9XDHECBKNKAZJMIWLCMZNURJZOZEPQEMVMLHSDJCXBIBKOFELLDMLHWZGNV9GOUSNSJCU9HKKCGPVTV",
                                Accepted = false
                            });
                        }
                    }*/

                    if (requests != null && requests.Count > 0)
                    {
                        foreach (var item in requests)
                        {
                            if (item.Name == null || item.ChatAddress == null || item.ChatKeyAddress == null || item.ContactAddress == null) continue;
                            contactRequests.Add(new ContactBinding(item));
                        }

                        //Update the request list.
                        if (RequestList == null || RequestList.Count != contactRequests.Count)
                        {
                            RequestList = contactRequests;
                            RequestListHeight = contactRequests.Count * RequestItemHeight;
                        }
                    }
                    else
                    {
                        //Reset the request list.
                        RequestList = null;
                        IsRequestExist = false;
                        RequestListHeight = 0;
                    }

                    //Set flag to show the contact requests.
                    IsRequestExist = contactRequests.Count > 0;
                    IsNoRequestExist = !(contactRequests.Count > 0);
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

        #endregion

        #region Commands

        #region ContactAddress

        public ICommand ContactAddressCommand
        {
            get
            {
                return new Command(async () =>
                {
                    await PushAsync<ContactAddressView>();
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
                    if (param is ContactBinding contact)
                    {
                        //Show the chat view, or a dialog for a contact request acceptation.
                        if (!contact.Contact.Accepted)
                        {
                            await PushAsync<AnswerContactRequestView>(contact.Contact);
                            return;
                        }
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
