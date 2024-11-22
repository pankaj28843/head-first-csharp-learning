namespace AnimalMatchingGame;

public partial class MainPage : ContentPage
{
    private readonly EmojiService _emojiService;
    private double _elapsedTime;
    private bool _isFindingMatch;
    private Button? _lastClickedButton;
    private int _matchesFound;
    private CancellationTokenSource? _timerCts;
    
    const string SolvedEmoji = "✅";
    const int UniqueEmojiCount = 8;

    public MainPage()
    {
        InitializeComponent();
        _emojiService = new EmojiService();
        SetupGame();
    }

    private void SetupGame()
    {
        PlayAgainButton.Clicked += OnPlayAgainButtonClicked;
        foreach (var button in AnimalButtons.Children.OfType<Button>()) button.Clicked += OnAnimalButtonClicked;
    }

    private void OnPlayAgainButtonClicked(object? sender, EventArgs e)
    {
        ResetGame();
        ResetTimer();
        StartTimer();

        var emojiPairs = _emojiService.GenerateEmojiPairs(UniqueEmojiCount);

        var buttons = AnimalButtons.Children.OfType<Button>().ToList();
        for (var i = 0; i < buttons.Count; i++)
        {
            ResetButtonStyle(buttons[i]);
            buttons[i].Text = emojiPairs[i];
        }
    }

    private void ResetGame()
    {
        _matchesFound = 0;
        _isFindingMatch = false;
        _lastClickedButton = null;

        AnimalButtons.IsVisible = true;
        PlayAgainButton.IsVisible = false;
    }

    private async void OnAnimalButtonClicked(object? sender, EventArgs e)
    {
        if (sender is not Button clickedButton || string.IsNullOrWhiteSpace(clickedButton.Text))
            return;
        
        // Ignore clicks on already matched buttons
        if (clickedButton.Text == SolvedEmoji) return;

        if (!_isFindingMatch)
        {
            HighlightButton(clickedButton);
            _lastClickedButton = clickedButton;
            _isFindingMatch = true;
        }
        else
        {
            if (_lastClickedButton != null && clickedButton != _lastClickedButton)
            {
                if (clickedButton.Text == _lastClickedButton.Text)
                {
                    _matchesFound++;
                    MarkButtonAsMatched(_lastClickedButton);
                    MarkButtonAsMatched(clickedButton);
                }
                else
                {
                    await Task.Delay(500); // Add a slight delay for better user experience
                    ResetButtonStyle(_lastClickedButton);
                    ResetButtonStyle(clickedButton);
                }
            }

            _isFindingMatch = false;
            _lastClickedButton = null;

            if (_matchesFound == UniqueEmojiCount) EndGame();
        }
    }

    private void EndGame()
    {
        StopTimer();
        AnimalButtons.IsVisible = false;
        PlayAgainButton.IsVisible = true;
    }

    private void ResetButtonStyle(Button button)
    {
        button.BackgroundColor = Colors.LightBlue;
        button.TextColor = Colors.Black;
        button.IsEnabled = true;
    }

    private void HighlightButton(Button button)
    {
        button.BackgroundColor = Colors.Red;
    }

    private void MarkButtonAsMatched(Button button)
    {
        button.Text = SolvedEmoji;
        button.IsEnabled = false;
        button.BackgroundColor = Colors.LightGray;
        button.TextColor = Colors.Gray;
    }

    private void StartTimer()
    {
        _timerCts = new CancellationTokenSource();
        _elapsedTime = 0;

        Task.Run(async () =>
        {
            while (!_timerCts.Token.IsCancellationRequested)
            {
                _elapsedTime += 0.1; // Increment elapsed time
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    TimeElapsed.Text = $"Time Elapsed: {_elapsedTime:F1} seconds";
                });
                await Task.Delay(100);
            }
        });
    }

    private void StopTimer()
    {
        _timerCts?.Cancel();
        _timerCts = null;
    }

    private void ResetTimer()
    {
        StopTimer();
        TimeElapsed.Text = "Time Elapsed: 0.0 seconds";
    }
}

public class EmojiService
{
    private static readonly List<string> AnimalEmojis =
    [
        "🐶", "🐱", "🐭", "🐹", "🐰", "🦊", "🐻", "🐼", "🐨", "🐯",
        "🦁", "🐮", "🐷", "🐸", "🐵", "🐔", "🐧", "🐦", "🐤", "🦆",
        "🦅", "🦉", "🐴", "🦄", "🐝", "🐛", "🦋", "🐌", "🐞", "🐜",
        "🦟", "🦗", "🐢", "🐍", "🦎", "🦂", "🐙", "🦑", "🦀", "🐡",
        "🐟", "🐬", "🐳", "🐋", "🦈", "🐊", "🦧", "🦥", "🦦", "🦨"
    ];

    public List<string> GenerateEmojiPairs(int count)
    {
        var random = new Random();
        var selectedEmojis = AnimalEmojis.OrderBy(_ => random.Next()).Take(count).ToList();
        selectedEmojis.AddRange(selectedEmojis); // Create pairs
        return selectedEmojis.OrderBy(_ => random.Next()).ToList();
    }
}