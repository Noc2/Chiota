﻿namespace Chiota.UWP.Persistence
{
  using System.IO;

  using Chiota.Messenger.Service;
  using Chiota.Persistence;

  using SQLite;

  using Tangle.Net.Cryptography.Signing;

  using Windows.Storage;

  /// <summary>
  /// The sql lite contact repository.
  /// </summary>
  public class SqlLiteContactRepository : AbstractSqlLiteContactRepository
  {
    /// <inheritdoc />
    public SqlLiteContactRepository(IMessenger messenger, ISignatureValidator signatureValidator)
      : base(messenger, signatureValidator)
    {
    }

    /// <summary>
    /// Gets the connection.
    /// </summary>
    public override SQLiteAsyncConnection Connection
    {
      get
      {
        var documentsPath = ApplicationData.Current.LocalFolder.Path;
        var path = Path.Combine(documentsPath, "ChiotaSQLite.db3");
        return new SQLiteAsyncConnection(path);
      }
    }
  }
}