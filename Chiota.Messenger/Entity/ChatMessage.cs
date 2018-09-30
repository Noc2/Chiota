﻿namespace Chiota.Messenger.Entity
{
  using System;

  public class ChatMessage
  {
    public DateTime Date { get; set; }

    public string Message { get; set; }

    public string Signature { get; set; }

    internal bool IsFirstPart { get; set; }
  }
}