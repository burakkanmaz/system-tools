namespace SystemTools;

public class MessageWindow : NativeWindow
{
    public MessageWindow(DisplayScalingManager scalingManager)
    {
        CreateHandle(new CreateParams());
    }
}