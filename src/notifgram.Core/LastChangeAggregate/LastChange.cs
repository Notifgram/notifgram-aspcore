namespace notifgram.Core.LastChangeAggregate;
public class LastChange
{
  public LastChange(int id, string changeListVersion, bool isDelete)
  {
    this.id = id;
    timestamp = changeListVersion;
    this.isDelete = isDelete;
  }

  public int id { get; set; }

  public string timestamp { get; set; }

  public bool isDelete { get; set; }
}
