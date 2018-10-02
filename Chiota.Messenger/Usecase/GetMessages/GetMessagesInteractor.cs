﻿namespace Chiota.Messenger.Usecase.GetMessages
{
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.Linq;
  using System.Text;
  using System.Text.RegularExpressions;
  using System.Threading.Tasks;

  using Chiota.Messenger.Encryption;
  using Chiota.Messenger.Entity;
  using Chiota.Messenger.Exception;
  using Chiota.Messenger.Extensions;
  using Chiota.Messenger.Service;
  using Chiota.Messenger.Service.Parser;

  using Newtonsoft.Json;

  using Tangle.Net.Entity;

  using VTDev.Libraries.CEXEngine.Crypto.Cipher.Asymmetric.Interfaces;

  public class GetMessagesInteractor : IUsecaseInteractor<GetMessagesRequest, GetMessagesResponse>
  {
    public GetMessagesInteractor(IMessenger messenger, IEncryption encryption)
    {
      this.Messenger = messenger;
      this.Encryption = encryption;
    }

    private IEncryption Encryption { get; }

    private IMessenger Messenger { get; }

    private Address CurrentChatAddress { get; set; }

    /// <inheritdoc />
    public async Task<GetMessagesResponse> ExecuteAsync(GetMessagesRequest request)
    {
      try
      {
        this.CurrentChatAddress = request.ChatAddress;
        var messages = await this.LoadMessagesOnAddressAsync(request.ChatKeyPair);

        return new GetMessagesResponse { Code = ResponseCode.Success, Messages = messages, CurrentChatAddress = this.CurrentChatAddress };
      }
      catch (MessengerException exception)
      {
        return new GetMessagesResponse { Code = exception.Code };
      }
    }

    private static Address GenerateNextAddress(Address contactAddress, List<Message> messages)
    {
      if (messages.Count <= 3)
      {
        return contactAddress;
      }

      var rgx = new Regex("[^A-Z]");
      var increment = contactAddress.GetChunk(0, 15).Increment();

      var str = increment + rgx.Replace(messages[messages.Count - 1].Payload.Value.ToUpper(), string.Empty)
                          + rgx.Replace(messages[messages.Count - 3].Payload.Value.ToUpper(), string.Empty) 
                          + rgx.Replace(messages[messages.Count - 2].Payload.Value.ToUpper(), string.Empty);

      str = str.Truncate(70);

      return new Address(str + contactAddress.Value.Substring(str.Length));
    }

    private async Task<List<ChatMessage>> LoadMessagesOnAddressAsync(IAsymmetricKeyPair chatKeyPair)
    {
      var encryptedMessages = await this.Messenger.GetMessagesByAddressAsync(this.CurrentChatAddress, new ChatMessageBundleParser());
      var decryptedMessages = new List<ChatMessage>();

      foreach (var encryptedMessage in encryptedMessages)
      {
        try
        {
          var decryptedMessage = JsonConvert.DeserializeObject<ChatMessage>(
            Encoding.UTF8.GetString(this.Encryption.Decrypt(chatKeyPair, encryptedMessage.Payload.ToBytes())));

          decryptedMessages.Add(decryptedMessage);
        }
        catch
        {
          // ignore messages that are not deserializable
        }
      }

      if (decryptedMessages.Count < Constants.MaxMessagesOnAddress)
      {
        return decryptedMessages;
      }

      this.CurrentChatAddress = GenerateNextAddress(this.CurrentChatAddress, encryptedMessages);
      decryptedMessages.AddRange(await this.LoadMessagesOnAddressAsync(chatKeyPair));

      return decryptedMessages;
    }
  }
}