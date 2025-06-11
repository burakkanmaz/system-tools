namespace SystemTools;

public class BrightnessForm : Form
{
    private readonly TrackBar _slider;
    private readonly Label _valueLabel;
    private readonly System.Windows.Forms.Timer _debounceTimer;
    private readonly BrightnessManager _brightnessManager;

    public BrightnessForm(BrightnessManager brightnessManager)
    {
        _brightnessManager = brightnessManager;
        Text = "Adjust Brightness";
        Width = 300;
        Height = 120;
        FormBorderStyle = FormBorderStyle.FixedToolWindow;
        StartPosition = FormStartPosition.CenterScreen;

        _slider = new TrackBar()
        {
            Minimum = 0,
            Maximum = 100,
            TickFrequency = 10,
            LargeChange = 10,
            SmallChange = 10,
            Value = brightnessManager.GetInternalBrightness() ?? 100,
            Dock = DockStyle.Top
        };

        _valueLabel = new Label()
        {
            Text = $@"{_slider.Value}%",
            Dock = DockStyle.Top,
            TextAlign = ContentAlignment.MiddleCenter
        };

        _debounceTimer = new System.Windows.Forms.Timer
        {
            Interval = 400
        };

        _debounceTimer.Tick += (s, e) =>
        {
            _debounceTimer.Stop();
            _brightnessManager.OverrideBrightnessUntilMidnight(_slider.Value);
        };

        _slider.Scroll += (s, e) =>
        {
            var rounded = (int)Math.Round(_slider.Value / 10.0) * 10;
            if (_slider.Value != rounded)
                _slider.Value = rounded;

            _valueLabel.Text = $"{_slider.Value}%";
            _debounceTimer.Stop();
            _debounceTimer.Start();
        };

        Controls.Add(_slider);
        Controls.Add(_valueLabel);
    }
}