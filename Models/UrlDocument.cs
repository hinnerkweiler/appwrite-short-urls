namespace DotNetRuntime.Models;

public class UrlDocument
{
    public string id { get; set; }
    public string permissions { get; set; }  //required
    public string destination { get; set; } //required
    public string slug { get; set; }
    public string comment { get; set; }
    public int recheck { get; set; }   //Days
    public DateTime created { get; set; }
    public DateTime updated { get; set; }
    public string userId { get; set; }

    public UrlDocument(string id= "", string permissions= "", string destination= "", string comment= "", string slug= "", string url= "", string created= "", string updated= "", string expire= "", string userId= "")
    {
        this.id = id;
        this.permissions = permissions;
        this.destination = destination;
        this.comment = comment;
        this.slug = slug;
        this.created = DateTime.Parse(created);
        this.updated = DateTime.Parse(updated);
        this.userId = userId;
    }
}