[System.Serializable]
public class TodoItem
{
    public string text;
    public bool completed;

    public TodoItem(string text)
    {
        this.text = text;
        completed = false;
    }
}